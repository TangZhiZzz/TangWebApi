using AspNetCoreRateLimit;
using TangWebApi.Extensions;
using TangWebApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// 配置API统一响应格式
builder.Services.AddApiResponseFormat();

// 添加自定义服务
builder.Services.AddSqlSugarService(builder.Configuration);
builder.Services.AddSwaggerService();
builder.Services.AddCorsService();
builder.Services.AddJwtAuthenticationService(builder.Configuration);
builder.Services.AddCacheService(builder.Configuration);
builder.Services.AddSerilogService(builder.Configuration);
builder.Services.AddMessageQueueService(builder.Configuration);
builder.Services.AddEmailService(builder.Configuration);
builder.Services.AddFileService(builder.Configuration);
builder.Services.AddOpenAIService(builder.Configuration);
builder.Services.AddDistributedLockService(builder.Configuration);
builder.Services.AddSystemInfoService();
builder.Services.AddRateLimitingService(builder.Configuration);
builder.Services.AddSignalRService(builder.Configuration);

// 配置消息队列设置
builder.Services.Configure<TangWebApi.Options.MessageQueueConfig>(builder.Configuration.GetSection("MessageQueue"));

// 添加消息队列消费者后台服务
builder.Services.AddHostedService<TangWebApi.Services.MessageConsumerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseDevelopmentEnvironment(app.Environment);

// 配置请求日志中间件（放在最前面以记录所有请求）
app.UseRequestLogging();

// 配置限流中间件
app.UseIpRateLimiting();
app.UseClientRateLimiting();

// 配置静态文件服务（为Knife4j提供支持）
app.UseStaticFiles();

// 配置文件上传目录的静态文件访问
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});

// 配置Knife4j UI
app.UseKnife4jService();

// 配置CORS
app.UseCorsService();

// 配置SignalR
app.UseSignalRService();

app.UseHttpsRedirection();

app.UseJwtAuthentication();

app.UseAuthorization();

app.MapControllers();

// 初始化数据库
await app.InitializeDatabaseAsync();
// 输出TangZhi字符画
Console.WriteLine(@"
████████╗ █████╗ ███╗   ██╗ ██████╗ ███████╗██╗  ██╗██╗
╚══██╔══╝██╔══██╗████╗  ██║██╔════╝ ╚══███╔╝██║  ██║██║
   ██║   ███████║██╔██╗ ██║██║  ███╗  ███╔╝ ███████║██║
   ██║   ██╔══██║██║╚██╗██║██║   ██║ ███╔╝  ██╔══██║██║
   ██║   ██║  ██║██║ ╚████║╚██████╔╝███████╗██║  ██║██║
   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝╚═╝  ╚═╝╚═╝
");
// 获取实际运行的地址
var addresses = app.Urls.Any() ? app.Urls : new[] { "http://localhost:5238" };
var primaryAddress = addresses.First();

Console.WriteLine("🚀 TangWebApi 启动成功！");
Console.WriteLine($"📅 启动时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"🌐 环境: {app.Environment.EnvironmentName}");
Console.WriteLine($"📍 访问地址: {primaryAddress}");
Console.WriteLine($"📖 API文档: {primaryAddress}/swagger");
Console.WriteLine($"📖 API文档: {primaryAddress}/kapi");
Console.WriteLine("=".PadRight(60, '='));
app.Run();
