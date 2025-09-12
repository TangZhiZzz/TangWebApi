namespace TangWebApi.Options
{
    /// <summary>
    /// 健康检查配置
    /// </summary>
    public class HealthCheckConfiguration
    {
        /// <summary>
        /// 数据库连接超时时间（秒）
        /// </summary>
        public int DatabaseTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Redis连接超时时间（秒）
        /// </summary>
        public int RedisTimeoutSeconds { get; set; } = 10;

        /// <summary>
        /// 消息队列连接超时时间（秒）
        /// </summary>
        public int MessageQueueTimeoutSeconds { get; set; } = 15;

        /// <summary>
        /// 是否启用详细检查
        /// </summary>
        public bool EnableDetailedChecks { get; set; } = true;

        /// <summary>
        /// 是否包含系统信息
        /// </summary>
        public bool IncludeSystemInfo { get; set; } = true;
    }
}