namespace TangWebApi.Models
{
    /// <summary>
    /// 队列配置模型
    /// </summary>
    public class QueueConfig
    {
        /// <summary>
        /// 队列名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 队列描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// 消息队列配置
    /// </summary>
    public class MessageQueueConfig
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// 默认交换机
        /// </summary>
        public string DefaultExchange { get; set; } = string.Empty;

        /// <summary>
        /// 默认路由键
        /// </summary>
        public string DefaultRoutingKey { get; set; } = string.Empty;

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// 重试延迟（毫秒）
        /// </summary>
        public int RetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// 连接超时（毫秒）
        /// </summary>
        public int ConnectionTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// 心跳间隔
        /// </summary>
        public int RequestedHeartbeat { get; set; } = 60;

        /// <summary>
        /// 队列配置列表
        /// </summary>
        public List<QueueConfig> Queues { get; set; } = new List<QueueConfig>();
    }
}