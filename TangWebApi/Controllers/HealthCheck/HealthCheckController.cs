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
        public async Task<OverallHealthCheckResult> GetHealth()
        {
            _logger.LogInformation("开始执行健康检查");
            var result = await _healthCheckService.CheckHealthAsync();
            
            if (result.OverallStatus == HealthStatus.Unhealthy || result.OverallStatus == HealthStatus.Unknown)
            {
                throw new InvalidOperationException($"系统健康检查失败，状态: {result.OverallStatus}");
            }
            
            return result;
        }

        /// <summary>
        /// 获取简化的健康状态（用于负载均衡器等）
        /// </summary>
        /// <returns>简化的健康状态</returns>
        [HttpGet("simple")]
        public async Task<object> GetSimpleHealth()
        {
            var result = await _healthCheckService.CheckHealthAsync();
            
            if (result.OverallStatus != HealthStatus.Healthy)
            {
                throw new InvalidOperationException("系统健康检查失败");
            }
            
            return new { Status = "Healthy" };
        }

        /// <summary>
        /// 检查特定服务的健康状态
        /// </summary>
        /// <param name="serviceName">服务名称（database, redis, messagequeue, externalapi）</param>
        /// <returns>服务健康检查结果</returns>
        [HttpGet("service/{serviceName}")]
        public async Task<HealthCheckResult> GetServiceHealth(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentException("服务名称不能为空");
            }

            _logger.LogInformation($"开始检查服务健康状态: {serviceName}");
            var result = await _healthCheckService.CheckServiceHealthAsync(serviceName);
            
            if (result.Status == HealthStatus.Unknown)
            {
                throw new KeyNotFoundException($"未知的服务名称: {serviceName}");
            }
            
            if (result.Status == HealthStatus.Unhealthy)
            {
                throw new InvalidOperationException($"服务 {serviceName} 健康检查失败: {result.ErrorMessage}");
            }
            
            return result;
        }

        /// <summary>
        /// 获取系统信息
        /// </summary>
        /// <returns>系统信息</returns>
        [HttpGet("system-info")]
        public async Task<SystemInfo> GetSystemInfo()
        {
            _logger.LogInformation("获取系统信息");
            var systemInfo = await _healthCheckService.GetSystemInfoAsync();
            
            return systemInfo;
        }

        /// <summary>
        /// 获取健康检查配置
        /// </summary>
        /// <returns>健康检查配置</returns>
        [HttpGet("configuration")]
        public TangWebApi.Options.HealthCheckConfiguration GetConfiguration()
        {
            var configuration = _healthCheckService.GetConfiguration();
            
            return configuration;
        }

        /// <summary>
        /// 更新健康检查配置
        /// </summary>
        /// <param name="configuration">新的配置</param>
        [HttpPut("configuration")]
        public async Task UpdateConfiguration([FromBody] TangWebApi.Options.HealthCheckConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "配置不能为空");
            }

            _healthCheckService.UpdateConfiguration(configuration);
            
            await Task.CompletedTask;
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