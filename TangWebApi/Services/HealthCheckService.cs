using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Extensions.Options;
using SqlSugar;
using StackExchange.Redis;
using TangWebApi.Models;
using TangWebApi.Services;

namespace TangWebApi.Services
{
    /// <summary>
    /// 健康检查服务实现
    /// </summary>
    public class HealthCheckService : IHealthCheckService
    {
        private readonly ISqlSugarClient _sqlSugarClient;
        private readonly IConnectionMultiplexer? _redis;
        private readonly IMessageQueueService _messageQueueService;
        private readonly ILogger<HealthCheckService> _logger;
        private readonly DateTime _startTime;
        private TangWebApi.Options.HealthCheckConfiguration _configuration;

        public HealthCheckService(
            ISqlSugarClient sqlSugarClient,
            IConnectionMultiplexer? redis,
            IMessageQueueService messageQueueService,
            ILogger<HealthCheckService> logger)
        {
            _sqlSugarClient = sqlSugarClient;
            _redis = redis;
            _messageQueueService = messageQueueService;
            _logger = logger;
            _startTime = DateTime.UtcNow;
            _configuration = new TangWebApi.Options.HealthCheckConfiguration();
        }

        /// <summary>
        /// 执行完整的健康检查
        /// </summary>
        public async Task<OverallHealthCheckResult> CheckHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new OverallHealthCheckResult
            {
                CheckTime = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("开始执行健康检查");

                // 并行执行各项检查
                var tasks = new List<Task<HealthCheckResult>>
                {
                    CheckDatabaseHealthAsync(),
                    CheckRedisHealthAsync(),
                    CheckMessageQueueHealthAsync(),
                    CheckExternalApiHealthAsync()
                };

                var results = await Task.WhenAll(tasks);
                result.ServiceResults.AddRange(results);

                // 获取系统信息
                if (_configuration.IncludeSystemInfo)
                {
                    result.SystemInfo = await GetSystemInfoAsync();
                }

                // 计算整体状态
                result.OverallStatus = CalculateOverallStatus(results);
                
                stopwatch.Stop();
                result.TotalResponseTimeMs = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation($"健康检查完成，整体状态: {result.OverallStatus}, 耗时: {result.TotalResponseTimeMs}ms");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "健康检查执行失败");
                result.OverallStatus = HealthStatus.Unhealthy;
                stopwatch.Stop();
                result.TotalResponseTimeMs = stopwatch.ElapsedMilliseconds;
            }

            return result;
        }

        /// <summary>
        /// 检查数据库健康状态
        /// </summary>
        public async Task<HealthCheckResult> CheckDatabaseHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new HealthCheckResult
            {
                ServiceName = "Database",
                CheckTime = DateTime.UtcNow
            };

            try
            {
                // 执行简单查询测试数据库连接
                var query = "SELECT 1";
                var queryResult = await _sqlSugarClient.Ado.GetScalarAsync(query);
                
                stopwatch.Stop();
                result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                result.Status = HealthStatus.Healthy;
                result.AdditionalInfo = new Dictionary<string, object>
                {
                    { "QueryResult", queryResult?.ToString() ?? "null" },
                    { "DatabaseType", _sqlSugarClient.CurrentConnectionConfig.DbType.ToString() }
                };

                _logger.LogDebug($"数据库健康检查成功，响应时间: {result.ResponseTimeMs}ms");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                result.Status = HealthStatus.Unhealthy;
                result.ErrorMessage = ex.Message;
                _logger.LogWarning(ex, "数据库健康检查失败");
            }

            return result;
        }

        /// <summary>
        /// 检查Redis缓存健康状态
        /// </summary>
        public async Task<HealthCheckResult> CheckRedisHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new HealthCheckResult
            {
                ServiceName = "Redis",
                CheckTime = DateTime.UtcNow
            };

            try
            {
                if (_redis == null)
                {
                    result.Status = HealthStatus.Warning;
                    result.ErrorMessage = "Redis连接未配置";
                    stopwatch.Stop();
                    result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                    return result;
                }

                var database = _redis.GetDatabase();
                var testKey = "health_check_test";
                var testValue = DateTime.UtcNow.ToString();

                // 测试写入和读取
                await database.StringSetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
                var retrievedValue = await database.StringGetAsync(testKey);
                await database.KeyDeleteAsync(testKey);

                stopwatch.Stop();
                result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                
                if (retrievedValue == testValue)
                {
                    result.Status = HealthStatus.Healthy;
                    result.AdditionalInfo = new Dictionary<string, object>
                    {
                        { "ConnectionStatus", _redis.IsConnected ? "Connected" : "Disconnected" },
                        { "TestResult", "Success" }
                    };
                }
                else
                {
                    result.Status = HealthStatus.Unhealthy;
                    result.ErrorMessage = "Redis读写测试失败";
                }

                _logger.LogDebug($"Redis健康检查完成，状态: {result.Status}, 响应时间: {result.ResponseTimeMs}ms");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                result.Status = HealthStatus.Unhealthy;
                result.ErrorMessage = ex.Message;
                _logger.LogWarning(ex, "Redis健康检查失败");
            }

            return result;
        }

        /// <summary>
        /// 检查消息队列健康状态
        /// </summary>
        public async Task<HealthCheckResult> CheckMessageQueueHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new HealthCheckResult
            {
                ServiceName = "MessageQueue",
                CheckTime = DateTime.UtcNow
            };

            try
            {
                // 使用消息队列服务的测试连接方法
                var testResult = await _messageQueueService.TestConnectionAsync();
                
                stopwatch.Stop();
                result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                
                if (testResult)
                {
                    result.Status = HealthStatus.Healthy;
                    result.AdditionalInfo = new Dictionary<string, object>
                    {
                        { "ConnectionTest", "Success" }
                    };
                }
                else
                {
                    result.Status = HealthStatus.Unhealthy;
                    result.ErrorMessage = "消息队列连接测试失败";
                }

                _logger.LogDebug($"消息队列健康检查完成，状态: {result.Status}, 响应时间: {result.ResponseTimeMs}ms");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                result.Status = HealthStatus.Unhealthy;
                result.ErrorMessage = ex.Message;
                _logger.LogWarning(ex, "消息队列健康检查失败");
            }

            return result;
        }

        /// <summary>
        /// 检查外部API健康状态
        /// </summary>
        public async Task<HealthCheckResult> CheckExternalApiHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new HealthCheckResult
            {
                ServiceName = "ExternalAPI",
                CheckTime = DateTime.UtcNow
            };

            try
            {
                // 这里可以添加对外部API的健康检查
                // 目前返回健康状态，因为没有配置外部API
                await Task.Delay(10); // 模拟API调用
                
                stopwatch.Stop();
                result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                result.Status = HealthStatus.Healthy;
                result.AdditionalInfo = new Dictionary<string, object>
                {
                    { "Note", "No external APIs configured" }
                };

                _logger.LogDebug($"外部API健康检查完成，响应时间: {result.ResponseTimeMs}ms");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                result.Status = HealthStatus.Unhealthy;
                result.ErrorMessage = ex.Message;
                _logger.LogWarning(ex, "外部API健康检查失败");
            }

            return result;
        }

        /// <summary>
        /// 获取系统信息
        /// </summary>
        public async Task<SystemInfo> GetSystemInfoAsync()
        {
            return await Task.FromResult(new SystemInfo
            {
                ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown",
                RuntimeVersion = Environment.Version.ToString(),
                OperatingSystem = RuntimeInformation.OSDescription,
                MachineName = Environment.MachineName,
                StartTime = _startTime,
                Uptime = DateTime.UtcNow - _startTime,
                MemoryUsageMB = GC.GetTotalMemory(false) / 1024 / 1024,
                CpuUsagePercent = GetCpuUsage()
            });
        }

        /// <summary>
        /// 检查特定服务的健康状态
        /// </summary>
        public async Task<HealthCheckResult> CheckServiceHealthAsync(string serviceName)
        {
            return serviceName.ToLower() switch
            {
                "database" => await CheckDatabaseHealthAsync(),
                "redis" => await CheckRedisHealthAsync(),
                "messagequeue" => await CheckMessageQueueHealthAsync(),
                "externalapi" => await CheckExternalApiHealthAsync(),
                _ => new HealthCheckResult
                {
                    ServiceName = serviceName,
                    Status = HealthStatus.Unknown,
                    ErrorMessage = $"未知的服务名称: {serviceName}",
                    CheckTime = DateTime.UtcNow
                }
            };
        }

        /// <summary>
        /// 获取健康检查配置
        /// </summary>
        public TangWebApi.Options.HealthCheckConfiguration GetConfiguration()
        {
            return _configuration;
        }

        /// <summary>
        /// 更新健康检查配置
        /// </summary>
        public void UpdateConfiguration(TangWebApi.Options.HealthCheckConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger.LogInformation("健康检查配置已更新");
        }

        /// <summary>
        /// 计算整体健康状态
        /// </summary>
        private HealthStatus CalculateOverallStatus(HealthCheckResult[] results)
        {
            if (results.Any(r => r.Status == HealthStatus.Unhealthy))
                return HealthStatus.Unhealthy;
            
            if (results.Any(r => r.Status == HealthStatus.Warning))
                return HealthStatus.Warning;
            
            if (results.Any(r => r.Status == HealthStatus.Unknown))
                return HealthStatus.Unknown;
            
            return HealthStatus.Healthy;
        }

        /// <summary>
        /// 获取CPU使用率（简化版本）
        /// </summary>
        private double GetCpuUsage()
        {
            try
            {
                // 这是一个简化的CPU使用率获取方法
                // 在生产环境中，可能需要更精确的实现
                using var process = Process.GetCurrentProcess();
                return process.TotalProcessorTime.TotalMilliseconds / Environment.TickCount * 100;
            }
            catch
            {
                return 0.0;
            }
        }
    }
}