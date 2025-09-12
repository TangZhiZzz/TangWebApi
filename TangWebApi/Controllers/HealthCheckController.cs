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
        public async Task<IActionResult> GetHealth()
        {
            try
            {
                _logger.LogInformation("开始执行健康检查");
                var result = await _healthCheckService.CheckHealthAsync();
                
                return result.OverallStatus switch
                {
                    HealthStatus.Healthy => Ok(result),
                    HealthStatus.Warning => Ok(result),
                    HealthStatus.Unhealthy => StatusCode(503, result),
                    HealthStatus.Unknown => StatusCode(503, result),
                    _ => StatusCode(503, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "健康检查执行失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取简化的健康状态（用于负载均衡器等）
        /// </summary>
        /// <returns>简化的健康状态</returns>
        [HttpGet("simple")]
        public async Task<IActionResult> GetSimpleHealth()
        {
            try
            {
                var result = await _healthCheckService.CheckHealthAsync();
                
                if (result.OverallStatus == HealthStatus.Healthy)
                {
                    return Ok(new { Status = "Healthy" });
                }
                else
                {
                    return StatusCode(503, new { Status = "Unhealthy" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "简化健康检查执行失败");
                return StatusCode(503, new { Status = "Unhealthy" });
            }
        }

        /// <summary>
        /// 检查特定服务的健康状态
        /// </summary>
        /// <param name="serviceName">服务名称（database, redis, messagequeue, externalapi）</param>
        /// <returns>服务健康检查结果</returns>
        [HttpGet("service/{serviceName}")]
        public async Task<IActionResult> GetServiceHealth(string serviceName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    return BadRequest(new { error = "服务名称不能为空" });
                }

                _logger.LogInformation($"开始检查服务健康状态: {serviceName}");
                var result = await _healthCheckService.CheckServiceHealthAsync(serviceName);
                
                return result.Status switch
                {
                    HealthStatus.Healthy => Ok(result),
                    HealthStatus.Warning => Ok(result),
                    HealthStatus.Unhealthy => StatusCode(503, result),
                    HealthStatus.Unknown => NotFound(result),
                    _ => StatusCode(503, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"检查服务健康状态失败: {serviceName}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取系统信息
        /// </summary>
        /// <returns>系统信息</returns>
        [HttpGet("system-info")]
        public async Task<IActionResult> GetSystemInfo()
        {
            try
            {
                _logger.LogInformation("获取系统信息");
                var systemInfo = await _healthCheckService.GetSystemInfoAsync();
                
                return Ok(systemInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取系统信息失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取健康检查配置
        /// </summary>
        /// <returns>健康检查配置</returns>
        [HttpGet("configuration")]
        public IActionResult GetConfiguration()
        {
            try
            {
                var configuration = _healthCheckService.GetConfiguration();
                
                return Ok(configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取健康检查配置失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 更新健康检查配置
        /// </summary>
        /// <param name="configuration">新的配置</param>
        /// <returns>更新结果</returns>
        [HttpPut("configuration")]
        public IActionResult UpdateConfiguration([FromBody] HealthCheckConfiguration configuration)
        {
            try
            {
                if (configuration == null)
                {
                    return BadRequest(new { error = "配置不能为空" });
                }

                _healthCheckService.UpdateConfiguration(configuration);
                
                return Ok(new { message = "配置更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新健康检查配置失败");
                return StatusCode(500, new { error = ex.Message });
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
}