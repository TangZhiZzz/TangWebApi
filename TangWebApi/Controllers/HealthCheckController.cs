using Microsoft.AspNetCore.Mvc;
using TangWebApi.Models;
using TangWebApi.Services;
using TangWebApi.Filter;

namespace TangWebApi.Controllers
{
    /// <summary>
    /// 健康检查控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class HealthCheckController : ControllerBase
    {
        private readonly IHealthCheckService _healthCheckService;
        private readonly ILogger<HealthCheckController> _logger;

        public HealthCheckController(
            IHealthCheckService healthCheckService,
            ILogger<HealthCheckController> logger)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        /// <summary>
        /// 获取整体健康状态
        /// </summary>
        /// <returns>整体健康检查结果</returns>
        [HttpGet]
        public async Task<HealthCheckResponse> GetHealth()
        {
            try
            {
                _logger.LogInformation("开始执行健康检查");
                var result = await _healthCheckService.CheckHealthAsync();
                
                var statusCode = result.OverallStatus switch
                {
                    HealthStatus.Healthy => 200,
                    HealthStatus.Warning => 200, // 警告状态仍返回200，但在响应中标明
                    HealthStatus.Unhealthy => 503, // 服务不可用
                    HealthStatus.Unknown => 503,
                    _ => 503
                };

                var response = new ApiResponse<OverallHealthCheckResult>
                {
                    Success = result.OverallStatus == HealthStatus.Healthy,
                    Message = GetStatusMessage(result.OverallStatus),
                    Data = result
                };

                return new HealthCheckResponse
                {
                    Success = result.OverallStatus == HealthStatus.Healthy,
                    Message = GetStatusMessage(result.OverallStatus),
                    Data = result,
                    StatusCode = statusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "健康检查执行失败");
                return new HealthCheckResponse
                {
                    Success = false,
                    Message = "健康检查执行失败",
                    Error = ex.Message,
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// 获取简化的健康状态（用于负载均衡器等）
        /// </summary>
        /// <returns>简化的健康状态</returns>
        [HttpGet("simple")]
        public async Task<SimpleHealthResponse> GetSimpleHealth()
        {
            try
            {
                var result = await _healthCheckService.CheckHealthAsync();
                
                if (result.OverallStatus == HealthStatus.Healthy)
                {
                    return new SimpleHealthResponse
                    {
                        Status = "Healthy",
                        StatusCode = 200
                    };
                }
                else
                {
                    return new SimpleHealthResponse
                    {
                        Status = "Unhealthy",
                        StatusCode = 503
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "简化健康检查执行失败");
                return new SimpleHealthResponse
                {
                    Status = "Unhealthy",
                    StatusCode = 503
                };
            }
        }

        /// <summary>
        /// 检查特定服务的健康状态
        /// </summary>
        /// <param name="serviceName">服务名称（database, redis, messagequeue, externalapi）</param>
        /// <returns>服务健康检查结果</returns>
        [HttpGet("service/{serviceName}")]
        public async Task<ServiceHealthResponse> GetServiceHealth(string serviceName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    return new ServiceHealthResponse
                    {
                        Success = false,
                        Message = "服务名称不能为空",
                        StatusCode = 400
                    };
                }

                _logger.LogInformation($"开始检查服务健康状态: {serviceName}");
                var result = await _healthCheckService.CheckServiceHealthAsync(serviceName);
                
                var response = new ApiResponse<HealthCheckResult>
                {
                    Success = result.Status == HealthStatus.Healthy,
                    Message = GetStatusMessage(result.Status),
                    Data = result
                };

                var statusCode = result.Status switch
                {
                    HealthStatus.Healthy => 200,
                    HealthStatus.Warning => 200,
                    HealthStatus.Unhealthy => 503,
                    HealthStatus.Unknown => 404,
                    _ => 503
                };

                return new ServiceHealthResponse
                {
                    Success = result.Status == HealthStatus.Healthy,
                    Message = GetStatusMessage(result.Status),
                    Data = result,
                    StatusCode = statusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"检查服务健康状态失败: {serviceName}");
                return new ServiceHealthResponse
                {
                    Success = false,
                    Message = "服务健康检查执行失败",
                    Error = ex.Message,
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// 获取系统信息
        /// </summary>
        /// <returns>系统信息</returns>
        [HttpGet("system-info")]
        public async Task<HealthCheckSystemInfoResponse> GetSystemInfo()
        {
            try
            {
                _logger.LogInformation("获取系统信息");
                var systemInfo = await _healthCheckService.GetSystemInfoAsync();
                
                return new HealthCheckSystemInfoResponse
                {
                    Success = true,
                    Message = "系统信息获取成功",
                    Data = systemInfo,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取系统信息失败");
                return new HealthCheckSystemInfoResponse
                {
                    Success = false,
                    Message = "获取系统信息失败",
                    Error = ex.Message,
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// 获取健康检查配置
        /// </summary>
        /// <returns>健康检查配置</returns>
        [HttpGet("configuration")]
        public HealthCheckConfigurationResponse GetConfiguration()
        {
            try
            {
                var configuration = _healthCheckService.GetConfiguration();
                
                return new HealthCheckConfigurationResponse
                {
                    Success = true,
                    Message = "配置获取成功",
                    Data = configuration,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取健康检查配置失败");
                return new HealthCheckConfigurationResponse
                {
                    Success = false,
                    Message = "获取配置失败",
                    Error = ex.Message,
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// 更新健康检查配置
        /// </summary>
        /// <param name="configuration">新的配置</param>
        /// <returns>更新结果</returns>
        [HttpPut("configuration")]
        public UpdateConfigurationResponse UpdateConfiguration([FromBody] HealthCheckConfiguration configuration)
        {
            try
            {
                if (configuration == null)
                {
                    return new UpdateConfigurationResponse
                    {
                        Success = false,
                        Message = "配置不能为空",
                        StatusCode = 400
                    };
                }

                _healthCheckService.UpdateConfiguration(configuration);
                
                return new UpdateConfigurationResponse
                {
                    Success = true,
                    Message = "配置更新成功",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新健康检查配置失败");
                return new UpdateConfigurationResponse
                {
                    Success = false,
                    Message = "配置更新失败",
                    Error = ex.Message,
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// 获取状态消息
        /// </summary>
        /// <param name="status">健康状态</param>
        /// <returns>状态消息</returns>
        private static string GetStatusMessage(HealthStatus status)
        {
            return status switch
            {
                HealthStatus.Healthy => "所有服务运行正常",
                HealthStatus.Warning => "部分服务存在警告",
                HealthStatus.Unhealthy => "部分服务不可用",
                HealthStatus.Unknown => "服务状态未知",
                _ => "未知状态"
            };
        }
    }

    // 健康检查响应数据模型
    public class HealthCheckResponse
    {
        public required bool Success { get; set; }
        public required string Message { get; set; }
        public OverallHealthCheckResult? Data { get; set; }
        public string? Error { get; set; }
        public required int StatusCode { get; set; }
    }

    public class SimpleHealthResponse
    {
        public required string Status { get; set; }
        public required int StatusCode { get; set; }
    }

    public class ServiceHealthResponse
    {
        public required bool Success { get; set; }
        public required string Message { get; set; }
        public HealthCheckResult? Data { get; set; }
        public string? Error { get; set; }
        public required int StatusCode { get; set; }
    }

    public class HealthCheckSystemInfoResponse
    {
        public required bool Success { get; set; }
        public required string Message { get; set; }
        public SystemInfo? Data { get; set; }
        public string? Error { get; set; }
        public required int StatusCode { get; set; }
    }

    public class HealthCheckConfigurationResponse
    {
        public required bool Success { get; set; }
        public required string Message { get; set; }
        public HealthCheckConfiguration? Data { get; set; }
        public string? Error { get; set; }
        public required int StatusCode { get; set; }
    }

    public class UpdateConfigurationResponse
    {
        public required bool Success { get; set; }
        public required string Message { get; set; }
        public string? Error { get; set; }
        public required int StatusCode { get; set; }
    }
}