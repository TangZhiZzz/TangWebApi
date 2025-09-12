using System.ComponentModel.DataAnnotations;

namespace TangWebApi.Models
{
    /// <summary>
    /// 健康检查结果
    /// </summary>
    public class HealthCheckResult
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// 健康状态
        /// </summary>
        public HealthStatus Status { get; set; }

        /// <summary>
        /// 响应时间（毫秒）
        /// </summary>
        public long ResponseTimeMs { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 检查时间
        /// </summary>
        public DateTime CheckTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 附加信息
        /// </summary>
        public Dictionary<string, object>? AdditionalInfo { get; set; }
    }

    /// <summary>
    /// 整体健康检查结果
    /// </summary>
    public class OverallHealthCheckResult
    {
        /// <summary>
        /// 整体健康状态
        /// </summary>
        public HealthStatus OverallStatus { get; set; }

        /// <summary>
        /// 总响应时间（毫秒）
        /// </summary>
        public long TotalResponseTimeMs { get; set; }

        /// <summary>
        /// 检查时间
        /// </summary>
        public DateTime CheckTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 各服务检查结果
        /// </summary>
        public List<HealthCheckResult> ServiceResults { get; set; } = new List<HealthCheckResult>();

        /// <summary>
        /// 系统信息
        /// </summary>
        public SystemInfo? SystemInfo { get; set; }
    }

    /// <summary>
    /// 健康状态枚举
    /// </summary>
    public enum HealthStatus
    {
        /// <summary>
        /// 健康
        /// </summary>
        Healthy = 0,

        /// <summary>
        /// 警告
        /// </summary>
        Warning = 1,

        /// <summary>
        /// 不健康
        /// </summary>
        Unhealthy = 2,

        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 3
    }

    /// <summary>
    /// 系统信息
    /// </summary>
    public class SystemInfo
    {
        /// <summary>
        /// 应用程序版本
        /// </summary>
        public string ApplicationVersion { get; set; } = string.Empty;

        /// <summary>
        /// 运行时版本
        /// </summary>
        public string RuntimeVersion { get; set; } = string.Empty;

        /// <summary>
        /// 操作系统
        /// </summary>
        public string OperatingSystem { get; set; } = string.Empty;

        /// <summary>
        /// 机器名称
        /// </summary>
        public string MachineName { get; set; } = string.Empty;

        /// <summary>
        /// 启动时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 运行时间
        /// </summary>
        public TimeSpan Uptime { get; set; }

        /// <summary>
        /// 内存使用情况（MB）
        /// </summary>
        public long MemoryUsageMB { get; set; }

        /// <summary>
        /// CPU使用率（百分比）
        /// </summary>
        public double CpuUsagePercent { get; set; }
    }


}