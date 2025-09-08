using Microsoft.AspNetCore.Mvc;
using TangWebApi.Services;
using TangWebApi.Models;
using LogLevel = TangWebApi.Services.LogLevel;

namespace TangWebApi.Controllers
{
    /// <summary>
    /// 日志测试控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("日志测试")]
    public class LoggingController : ControllerBase
    {
        private readonly ILoggingService _loggingService;

        public LoggingController(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        /// <summary>
        /// 测试调试日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <returns></returns>
        [HttpPost("debug")]
        public LogResponse TestDebugLog([FromBody] string message)
        {
            _loggingService.LogDebug(message);
            return new LogResponse
            {
                Message = "调试日志已记录",
                LogLevel = "Debug",
                Content = message,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 测试调试日志 (GET方法)
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <returns></returns>
        [HttpGet("test-debug")]
        public LogResponse TestDebugLogGet([FromQuery] string message = "测试调试日志")
        {
            _loggingService.LogDebug(message);
            return new LogResponse
            {
                Message = "调试日志已记录",
                LogLevel = "Debug",
                Content = message,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 测试信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <returns></returns>
        [HttpPost("info")]
        public LogResponse TestInfoLog([FromBody] string message)
        {
            _loggingService.LogInformation(message);
            return new LogResponse
            {
                Message = "信息日志已记录",
                LogLevel = "Information",
                Content = message,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 测试警告日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <returns></returns>
        [HttpPost("warning")]
        public LogResponse TestWarningLog([FromBody] string message)
        {
            _loggingService.LogWarning(message);
            return new LogResponse
            {
                Message = "警告日志已记录",
                LogLevel = "Warning",
                Content = message,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 测试错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <returns></returns>
        [HttpPost("error")]
        public LogResponse TestErrorLog([FromBody] string message)
        {
            var exception = new Exception("测试异常: " + message);
            _loggingService.LogError(message, exception);
            return new LogResponse
            {
                Message = "错误日志已记录",
                LogLevel = "Error",
                Content = message,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 测试严重错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <returns></returns>
        [HttpPost("critical")]
        public LogResponse TestCriticalLog([FromBody] string message)
        {
            var exception = new Exception("测试严重异常: " + message);
            _loggingService.LogCritical(message, exception);
            return new LogResponse
            {
                Message = "严重错误日志已记录",
                LogLevel = "Critical",
                Content = message,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 测试结构化日志
        /// </summary>
        /// <param name="request">结构化日志请求</param>
        /// <returns></returns>
        [HttpPost("structured")]
        public StructuredLogResponse TestStructuredLog([FromBody] StructuredLogRequest request)
        {
            var properties = new Dictionary<string, object>
            {
                { "UserId", request.UserId },
                { "Action", request.Action },
                { "Resource", request.Resource },
                { "Timestamp", DateTime.UtcNow },
                { "ClientIP", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown" },
                { "UserAgent", Request.Headers["User-Agent"].ToString() }
            };

            _loggingService.LogStructured(request.LogLevel, request.Message, properties);
            return new StructuredLogResponse
            {
                Message = "结构化日志已记录",
                Properties = properties,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 测试业务操作日志
        /// </summary>
        /// <param name="request">业务操作请求</param>
        /// <returns></returns>
        [HttpPost("business")]
        public BusinessLogResponse TestBusinessLog([FromBody] BusinessLogRequest request)
        {
            _loggingService.LogBusinessOperation(
                request.Operation,
                request.UserId,
                request.Resource
            );
            return new BusinessLogResponse
            {
                Message = "业务操作日志已记录",
                Operation = request.Operation,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 测试性能日志（模拟慢操作）
        /// </summary>
        /// <param name="delayMs">延迟毫秒数</param>
        /// <returns></returns>
        [HttpPost("performance/{delayMs}")]
        public async Task<PerformanceLogResponse> TestPerformanceLog(int delayMs)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // 模拟耗时操作
            await Task.Delay(delayMs);
            
            stopwatch.Stop();
            
            var properties = new Dictionary<string, object>
            {
                { "Operation", "TestPerformance" },
                { "Duration", stopwatch.ElapsedMilliseconds },
                { "DelayRequested", delayMs },
                { "Endpoint", Request.Path }
            };

            _loggingService.LogStructured(
                stopwatch.ElapsedMilliseconds > 1000 ? LogLevel.Warning : LogLevel.Information,
                "性能测试操作完成，耗时 {Duration}ms",
                properties
            );

            return new PerformanceLogResponse
            {
                Message = "性能测试完成",
                Duration = stopwatch.ElapsedMilliseconds,
                DelayRequested = delayMs,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 获取日志配置信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("config")]
        public LoggingConfigResponse GetLoggingConfig()
        {
            _loggingService.LogInformation("日志配置信息被查询");
            return new LoggingConfigResponse
            {
                MinimumLevel = "Information",
                ConsoleEnabled = true,
                FileEnabled = true,
                DatabaseEnabled = false,
                LogFilePath = "logs/app.log",
                EnabledProviders = new[] { "Console", "File", "Serilog" },
                Timestamp = DateTime.Now
            };
        }
    }

    /// <summary>
    /// 结构化日志请求模型
    /// </summary>
    public class StructuredLogRequest
    {
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }

    /// <summary>
    /// 业务操作日志请求模型
    /// </summary>
    public class BusinessLogRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public Dictionary<string, object>? Details { get; set; }
    }

    // 日志相关响应模型
    public class LogResponse
    {
        public required string Message { get; set; }
        public required string LogLevel { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class StructuredLogResponse
    {
        public required string Message { get; set; }
        public required Dictionary<string, object> Properties { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class BusinessLogResponse
    {
        public required string Message { get; set; }
        public required string Operation { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PerformanceLogResponse
    {
        public required string Message { get; set; }
        public long Duration { get; set; }
        public int DelayRequested { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class LoggingConfigResponse
    {
        public required string MinimumLevel { get; set; }
        public bool ConsoleEnabled { get; set; }
        public bool FileEnabled { get; set; }
        public bool DatabaseEnabled { get; set; }
        public required string LogFilePath { get; set; }
        public required string[] EnabledProviders { get; set; }
        public DateTime Timestamp { get; set; }
    }
}