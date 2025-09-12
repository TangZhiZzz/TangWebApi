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
        public IActionResult TestDebugLog([FromBody] string message)
        {
            try
            {
                _loggingService.LogDebug(message);
                return Ok(new { message = "调试日志已记录", logLevel = "Debug", content = message, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 测试调试日志 (GET方法)
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <returns></returns>
        [HttpGet("test-debug")]
        public IActionResult TestDebugLogGet([FromQuery] string message = "测试调试日志")
        {
            try
            {
                _loggingService.LogDebug(message);
                return Ok(new { message = "调试日志已记录", logLevel = "Debug", content = message, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 测试信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <returns></returns>
        [HttpPost("info")]
        public IActionResult TestInfoLog([FromBody] string message)
        {
            try
            {
                _loggingService.LogInformation(message);
                return Ok(new { message = "信息日志已记录", logLevel = "Information", content = message, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 测试警告日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <returns></returns>
        [HttpPost("warning")]
        public IActionResult TestWarningLog([FromBody] string message)
        {
            try
            {
                _loggingService.LogWarning(message);
                return Ok(new { message = "警告日志已记录", logLevel = "Warning", content = message, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 测试错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <returns></returns>
        [HttpPost("error")]
        public IActionResult TestErrorLog([FromBody] string message)
        {
            try
            {
                var exception = new Exception("测试异常: " + message);
                _loggingService.LogError(message, exception);
                return Ok(new { message = "错误日志已记录", logLevel = "Error", content = message, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 测试严重错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <returns></returns>
        [HttpPost("critical")]
        public IActionResult TestCriticalLog([FromBody] string message)
        {
            try
            {
                var exception = new Exception("测试严重异常: " + message);
                _loggingService.LogCritical(message, exception);
                return Ok(new { message = "严重错误日志已记录", logLevel = "Critical", content = message, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 测试结构化日志
        /// </summary>
        /// <param name="request">结构化日志请求</param>
        /// <returns></returns>
        [HttpPost("structured")]
        public IActionResult TestStructuredLog([FromBody] StructuredLogRequest request)
        {
            try
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
                return Ok(new { message = "结构化日志已记录", properties, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 测试业务操作日志
        /// </summary>
        /// <param name="request">业务操作请求</param>
        /// <returns></returns>
        [HttpPost("business")]
        public IActionResult TestBusinessLog([FromBody] BusinessLogRequest request)
        {
            try
            {
                _loggingService.LogBusinessOperation(
                    request.Operation,
                    request.UserId,
                    request.Resource
                );
                return Ok(new { message = "业务操作日志已记录", operation = request.Operation, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 测试性能日志（模拟慢操作）
        /// </summary>
        /// <param name="delayMs">延迟毫秒数</param>
        /// <returns></returns>
        [HttpPost("performance/{delayMs}")]
        public async Task<IActionResult> TestPerformanceLog(int delayMs)
        {
            try
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

                return Ok(new { message = "性能测试完成", duration = stopwatch.ElapsedMilliseconds, delayRequested = delayMs, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取日志配置信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("config")]
        public IActionResult GetLoggingConfig()
        {
            try
            {
                _loggingService.LogInformation("日志配置信息被查询");
                return Ok(new { minimumLevel = "Information", consoleEnabled = true, fileEnabled = true, databaseEnabled = false, logFilePath = "logs/app.log", enabledProviders = new[] { "Console", "File", "Serilog" }, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
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


}