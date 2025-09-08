using TangWebApi.Models;

namespace TangWebApi.Services
{
    /// <summary>
    /// 健康检查服务接口
    /// </summary>
    public interface IHealthCheckService
    {
        /// <summary>
        /// 执行完整的健康检查
        /// </summary>
        /// <returns>整体健康检查结果</returns>
        Task<OverallHealthCheckResult> CheckHealthAsync();

        /// <summary>
        /// 检查数据库健康状态
        /// </summary>
        /// <returns>数据库健康检查结果</returns>
        Task<HealthCheckResult> CheckDatabaseHealthAsync();

        /// <summary>
        /// 检查Redis缓存健康状态
        /// </summary>
        /// <returns>Redis健康检查结果</returns>
        Task<HealthCheckResult> CheckRedisHealthAsync();

        /// <summary>
        /// 检查消息队列健康状态
        /// </summary>
        /// <returns>消息队列健康检查结果</returns>
        Task<HealthCheckResult> CheckMessageQueueHealthAsync();

        /// <summary>
        /// 检查外部API健康状态
        /// </summary>
        /// <returns>外部API健康检查结果</returns>
        Task<HealthCheckResult> CheckExternalApiHealthAsync();

        /// <summary>
        /// 获取系统信息
        /// </summary>
        /// <returns>系统信息</returns>
        Task<SystemInfo> GetSystemInfoAsync();

        /// <summary>
        /// 检查特定服务的健康状态
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>服务健康检查结果</returns>
        Task<HealthCheckResult> CheckServiceHealthAsync(string serviceName);

        /// <summary>
        /// 获取健康检查配置
        /// </summary>
        /// <returns>健康检查配置</returns>
        HealthCheckConfiguration GetConfiguration();

        /// <summary>
        /// 更新健康检查配置
        /// </summary>
        /// <param name="configuration">新的配置</param>
        void UpdateConfiguration(HealthCheckConfiguration configuration);
    }
}