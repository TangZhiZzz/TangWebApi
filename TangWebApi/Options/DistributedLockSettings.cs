namespace TangWebApi.Options
{
    /// <summary>
    /// 分布式锁配置选项
    /// </summary>
    public class DistributedLockSettings
    {
        /// <summary>
        /// 是否启用分布式锁
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 锁的默认过期时间（秒）
        /// </summary>
        public int DefaultExpirationSeconds { get; set; } = 30;

        /// <summary>
        /// 锁的最大过期时间（秒）
        /// </summary>
        public int MaxExpirationSeconds { get; set; } = 300;

        /// <summary>
        /// 获取锁的重试间隔（毫秒）
        /// </summary>
        public int RetryIntervalMs { get; set; } = 100;

        /// <summary>
        /// 获取锁的最大重试次数
        /// </summary>
        public int MaxRetryCount { get; set; } = 10;

        /// <summary>
        /// 锁的键前缀
        /// </summary>
        public string KeyPrefix { get; set; } = "distributed_lock:";

        /// <summary>
        /// 是否启用锁的自动续期
        /// </summary>
        public bool EnableAutoRenewal { get; set; } = true;

        /// <summary>
        /// 自动续期间隔（秒）
        /// </summary>
        public int AutoRenewalIntervalSeconds { get; set; } = 10;

        /// <summary>
        /// 锁持有者标识符生成策略
        /// </summary>
        public string OwnerIdentifierStrategy { get; set; } = "guid"; // guid, machine, custom

        /// <summary>
        /// 自定义锁持有者标识符（当策略为custom时使用）
        /// </summary>
        public string? CustomOwnerIdentifier { get; set; }

        /// <summary>
        /// 是否启用锁的监控和日志
        /// </summary>
        public bool EnableMonitoring { get; set; } = true;

        /// <summary>
        /// 慢锁阈值（毫秒）- 超过此时间的锁操作将被记录为慢操作
        /// </summary>
        public int SlowLockThresholdMs { get; set; } = 1000;
    }
}