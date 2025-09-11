namespace TangWebApi.Models
{
    /// <summary>
    /// 限流规则模型
    /// </summary>
    public class RateLimitRule
    {
        /// <summary>
        /// 端点路径（支持通配符*）
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// 时间周期（如：1s, 1m, 1h, 1d）
        /// </summary>
        public string Period { get; set; } = string.Empty;

        /// <summary>
        /// 限制次数
        /// </summary>
        public long Limit { get; set; }
    }

    /// <summary>
    /// IP限流配置模型
    /// </summary>
    public class IpRateLimitSettings
    {
        /// <summary>
        /// 是否启用端点限流
        /// </summary>
        public bool EnableEndpointRateLimiting { get; set; } = true;

        /// <summary>
        /// 是否堆叠被阻止的请求
        /// </summary>
        public bool StackBlockedRequests { get; set; } = false;

        /// <summary>
        /// 真实IP头部名称
        /// </summary>
        public string RealIpHeader { get; set; } = "X-Real-IP";

        /// <summary>
        /// 客户端ID头部名称
        /// </summary>
        public string ClientIdHeader { get; set; } = "X-ClientId";

        /// <summary>
        /// 限流时返回的HTTP状态码
        /// </summary>
        public int HttpStatusCode { get; set; } = 429;

        /// <summary>
        /// 通用限流规则
        /// </summary>
        public List<RateLimitRule> GeneralRules { get; set; } = new List<RateLimitRule>();

        /// <summary>
        /// IP白名单
        /// </summary>
        public List<string> IpWhitelist { get; set; } = new List<string>();

        /// <summary>
        /// 端点白名单
        /// </summary>
        public List<string> EndpointWhitelist { get; set; } = new List<string>();
    }

    /// <summary>
    /// 客户端限流配置模型
    /// </summary>
    public class ClientRateLimitSettings
    {
        /// <summary>
        /// 是否启用端点限流
        /// </summary>
        public bool EnableEndpointRateLimiting { get; set; } = true;

        /// <summary>
        /// 是否堆叠被阻止的请求
        /// </summary>
        public bool StackBlockedRequests { get; set; } = false;

        /// <summary>
        /// 客户端ID头部名称
        /// </summary>
        public string ClientIdHeader { get; set; } = "X-ClientId";

        /// <summary>
        /// 限流时返回的HTTP状态码
        /// </summary>
        public int HttpStatusCode { get; set; } = 429;

        /// <summary>
        /// 通用限流规则
        /// </summary>
        public List<RateLimitRule> GeneralRules { get; set; } = new List<RateLimitRule>();

        /// <summary>
        /// 客户端白名单
        /// </summary>
        public List<string> ClientWhitelist { get; set; } = new List<string>();

        /// <summary>
        /// 端点白名单
        /// </summary>
        public List<string> EndpointWhitelist { get; set; } = new List<string>();
    }

    /// <summary>
    /// 限流响应模型
    /// </summary>
    public class RateLimitResponse
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public int Code { get; set; } = 429;

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; } = "请求过于频繁，请稍后再试";

        /// <summary>
        /// 重试时间（秒）
        /// </summary>
        public int RetryAfter { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}