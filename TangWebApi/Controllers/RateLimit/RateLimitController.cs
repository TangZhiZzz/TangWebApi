using Microsoft.AspNetCore.Mvc;
using TangWebApi.Models;

namespace TangWebApi.Controllers
{
    /// <summary>
    /// 限流测试控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RateLimitController : ControllerBase
    {
        /// <summary>
        /// 测试限流接口
        /// </summary>
        [HttpGet("test")]
        public async Task<object> TestRateLimit()
        {
            return new
            {
                Timestamp = DateTime.Now,
                ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                RequestId = Guid.NewGuid().ToString()
            };
        }

        /// <summary>
        /// 获取限流状态
        /// </summary>
        [HttpGet("status")]
        public async Task<object> GetRateLimitStatus()
        {
            var headers = Response.Headers;
            var rateLimitHeaders = new Dictionary<string, string>();

            // 收集限流相关的响应头
            foreach (var header in Request.Headers)
            {
                if (header.Key.StartsWith("X-RateLimit", StringComparison.OrdinalIgnoreCase))
                {
                    rateLimitHeaders[header.Key] = header.Value.ToString();
                }
            }

            return new
            {
                ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.Now,
                RateLimitHeaders = rateLimitHeaders,
                RequestPath = Request.Path.Value,
                Method = Request.Method
            };
        }

        /// <summary>
        /// 高频测试接口（用于快速触发限流）
        /// </summary>
        [HttpPost("burst")]
        public async Task<object> BurstTest()
        {
            return new
            {
                Timestamp = DateTime.Now,
                RequestCount = 1,
                Warning = "此接口用于测试限流功能，请勿频繁调用"
            };
        }

        /// <summary>
        /// 获取限流配置信息
        /// </summary>
        [HttpGet("config")]
        public async Task<object> GetRateLimitConfig()
        {
            var config = new
            {
                IpRateLimit = new
                {
                    GeneralRules = new[]
                    {
                        new { Endpoint = "*", Period = "1m", Limit = 100 },
                        new { Endpoint = "*", Period = "1h", Limit = 1000 }
                    },
                    HttpStatusCode = 429,
                    Message = "IP限流：每分钟最多100次请求，每小时最多1000次请求"
                },
                ClientRateLimit = new
                {
                    GeneralRules = new[]
                    {
                        new { Endpoint = "*", Period = "1m", Limit = 200 },
                        new { Endpoint = "*", Period = "1h", Limit = 2000 }
                    },
                    HttpStatusCode = 429,
                    Message = "客户端限流：每分钟最多200次请求，每小时最多2000次请求"
                }
            };

            return config;
        }
    }
}