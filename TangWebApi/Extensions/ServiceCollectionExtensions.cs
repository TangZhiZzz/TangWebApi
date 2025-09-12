using SqlSugar;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TangWebApi.Models;
using TangWebApi.Services;
using TangWebApi.Filter;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using AspNetCoreRateLimit;

namespace TangWebApi.Extensions
{
    /// <summary>
    /// 服务集合扩展方法
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 用于跟踪操作名称的列表
        /// </summary>
        private static readonly List<string> ActionNameList = new List<string>();
        /// <summary>
        /// 添加SqlSugar服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlSugarService(this IServiceCollection services, IConfiguration configuration)
        {
            // 配置SqlSugar
            services.AddScoped<ISqlSugarClient>(provider =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                var config = new ConnectionConfig
                {
                    ConnectionString = connectionString,
                    DbType = DbType.Sqlite,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                };
                return new SqlSugarClient(config);
            });

            // 注册SqlSugarService
            services.AddScoped<Services.SqlSugarService>();

            return services;
        }

        /// <summary>
        /// 添加Swagger服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns></returns>
        public static IServiceCollection AddSwaggerService(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "TangWebApi",
                    Version = "v1",
                    Description = "基于.NET 8的Web API项目，集成SqlSugar、Swagger和Knife4j",
                    Contact = new OpenApiContact
                    {
                        Name = "TangWebApi",
                        Email = "admin@tangwebapi.com"
                    }
                });

                // 添加XML注释
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // 添加JWT认证
                c.AddSecurityDefinition("Authorization", new OpenApiSecurityScheme()
                {
                    Description = "JWT授权token前面需要加上字段Bearer与一个空格,如Bearer token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                c.OperationFilter<SwaggerFilter>();

                c.CustomOperationIds(apiDesc =>
                {
                    var controllerAction = apiDesc.ActionDescriptor as ControllerActionDescriptor;
                    if (controllerAction == null)
                    {
                        return apiDesc.ActionDescriptor.DisplayName ?? "Unknown";
                    }
                    if (ActionNameList.Contains(controllerAction.ActionName))
                    {
                        return controllerAction.ActionName + $" ({controllerAction.ControllerName})";
                    }
                    ActionNameList.Add(controllerAction.ActionName);
                    return controllerAction.ActionName;
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            return services;
        }

        /// <summary>
        /// 添加CORS服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns></returns>
        public static IServiceCollection AddCorsService(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            return services;
        }

        /// <summary>
        /// 添加JWT认证服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns></returns>
        public static IServiceCollection AddJwtAuthenticationService(this IServiceCollection services, IConfiguration configuration)
        {
            // 绑定JWT配置
            var jwtSettings = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettings);
            
            var jwtConfig = jwtSettings.Get<JwtSettings>();
            if (jwtConfig == null)
            {
                throw new InvalidOperationException("JWT配置不能为空");
            }

            var key = Encoding.UTF8.GetBytes(jwtConfig.SecretKey);

            // 添加认证服务
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtConfig.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // 添加授权服务
            services.AddAuthorization();

            // 注册JWT服务
            services.AddScoped<JwtService>();

            return services;
        }

        /// <summary>
        /// 添加缓存服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns></returns>
        public static IServiceCollection AddCacheService(this IServiceCollection services, IConfiguration configuration)
        {
            // 添加内存缓存（作为备用）
            services.AddMemoryCache();
            services.AddScoped<MemoryCacheService>();

            // 尝试配置Redis缓存
            var redisConnectionString = configuration.GetConnectionString("Redis");
            
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                try
                {
                    // 尝试连接Redis
                    var redis = ConnectionMultiplexer.Connect(redisConnectionString);
                    
                    // 测试连接
                    var database = redis.GetDatabase();
                    database.Ping();
                    
                    // 如果连接成功，注册Redis缓存服务
                    services.AddSingleton<IConnectionMultiplexer>(redis);
                    services.AddScoped<RedisCacheService>();
                    services.AddScoped<TangWebApi.Services.ICacheService>(provider => provider.GetRequiredService<RedisCacheService>());
                    
                    Console.WriteLine("Redis缓存服务已启用");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Redis连接失败，使用内存缓存: {ex.Message}");
                    // Redis连接失败，使用内存缓存
                    services.AddScoped<TangWebApi.Services.ICacheService>(provider => provider.GetRequiredService<MemoryCacheService>());
                }
            }
            else
            {
                Console.WriteLine("未配置Redis连接字符串，使用内存缓存");
                // 未配置Redis，使用内存缓存
                services.AddScoped<TangWebApi.Services.ICacheService>(provider => provider.GetRequiredService<MemoryCacheService>());
            }

            return services;
        }

        /// <summary>
        /// 添加Serilog日志服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns></returns>
        public static IServiceCollection AddSerilogService(this IServiceCollection services, IConfiguration configuration)
        {
            // 配置Serilog
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName();

            // 控制台输出
            loggerConfiguration.WriteTo.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");

            // 文件输出
            var logPath = configuration["Logging:FilePath"] ?? "logs/app-.log";
            loggerConfiguration.WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                formatter: new CompactJsonFormatter(),
                shared: true);

            // 错误日志单独文件
            loggerConfiguration.WriteTo.File(
                path: "logs/errors-.log",
                restrictedToMinimumLevel: LogEventLevel.Error,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                formatter: new CompactJsonFormatter(),
                shared: true);

            // SQLite数据库输出（可选）
            var enableDbLogging = configuration.GetValue<bool>("Logging:EnableDatabaseLogging");
            if (enableDbLogging)
            {
                var dbPath = configuration["Logging:DatabasePath"] ?? "logs/app.db";
                loggerConfiguration.WriteTo.SQLite(
                    sqliteDbPath: dbPath,
                    tableName: "Logs",
                    restrictedToMinimumLevel: LogEventLevel.Information);
            }

            // 根据环境调整日志级别
            var environment = configuration["ASPNETCORE_ENVIRONMENT"];
            if (environment == "Development")
            {
                loggerConfiguration.MinimumLevel.Debug();
            }
            else if (environment == "Production")
            {
                loggerConfiguration.MinimumLevel.Information();
            }

            // 创建Logger
            var logger = loggerConfiguration.CreateLogger();
            
            // 设置全局Logger
            Log.Logger = logger;
            
            // 注册服务
            services.AddSingleton<Serilog.ILogger>(logger);
            services.AddSingleton<TangWebApi.Services.ILoggingService, SerilogService>();

            return services;
        }

        /// <summary>
        /// 添加消息队列服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns></returns>
        public static IServiceCollection AddMessageQueueService(this IServiceCollection services, IConfiguration configuration)
        {
            // 配置消息队列设置
            services.Configure<MessageQueueSettings>(configuration.GetSection("MessageQueue"));

            // 注册消息队列服务
            services.AddSingleton<IMessageQueueService, RabbitMQService>();

            return services;
        }

        /// <summary>
        /// 添加健康检查服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns></returns>
        public static IServiceCollection AddHealthCheckService(this IServiceCollection services, IConfiguration configuration)
        {
            // 注册健康检查服务，使用工厂模式处理可选依赖
            services.AddScoped<IHealthCheckService>(provider =>
            {
                var sqlSugarClient = provider.GetRequiredService<ISqlSugarClient>();
                var messageQueueService = provider.GetRequiredService<IMessageQueueService>();
                var logger = provider.GetRequiredService<ILogger<HealthCheckService>>();
                
                // 尝试获取Redis连接，如果失败则传入null
                IConnectionMultiplexer? redis = null;
                try
                {
                    redis = provider.GetService<IConnectionMultiplexer>();
                }
                catch
                {
                    // Redis连接不可用时忽略错误
                }
                
                return new HealthCheckService(sqlSugarClient, redis, messageQueueService, logger);
            });

            return services;
        }

        /// <summary>
        /// 添加邮件服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns></returns>
        public static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
        {
            // 配置邮件设置
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // 注册邮件服务
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }

        /// <summary>
        /// 添加文件服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns></returns>
        public static IServiceCollection AddFileService(this IServiceCollection services, IConfiguration configuration)
        {
            // 配置文件设置
            services.Configure<FileSettings>(configuration.GetSection("FileSettings"));

            // 注册文件服务
            services.AddScoped<IFileService, FileService>();

            return services;
        }

        /// <summary>
        /// 配置API统一响应格式
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddApiResponseFormat(this IServiceCollection services)
        {
            // 注册ApiResponseFilter到DI容器
            services.AddScoped<TangWebApi.Filter.ApiResponseFilter>();
            
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<TangWebApi.Filter.ApiResponseFilter>();
            });

            return services;
        }

        /// <summary>
        /// 添加系统信息服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddSystemInfoService(this IServiceCollection services)
        {
            // 注册系统信息服务
            services.AddScoped<ISystemInfoService, SystemInfoService>();

            return services;
        }

        /// <summary>
        /// 添加限流服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddRateLimitingService(this IServiceCollection services, IConfiguration configuration)
        {
            // 添加内存缓存（限流需要）
            services.AddMemoryCache();

            // 配置IP限流
            services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));

            // 配置客户端限流
            services.Configure<ClientRateLimitOptions>(configuration.GetSection("ClientRateLimiting"));

            // 添加限流策略存储
            services.AddInMemoryRateLimiting();

            // 注册限流服务
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            return services;
        }

        /// <summary>
        /// 添加SignalR服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddSignalRService(this IServiceCollection services, IConfiguration configuration)
        {
            // 添加SignalR服务
            services.AddSignalR();

            // 注册SignalR相关服务
            services.AddScoped<ISignalRService, SignalRService>();

            return services;
        }
    }
}