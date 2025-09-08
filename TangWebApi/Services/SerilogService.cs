using System;
using System.Collections.Generic;
using Serilog;
using Serilog.Events;
using TangWebApi.Services;
using ILogger = Serilog.ILogger;

namespace TangWebApi.Services
{
    /// <summary>
    /// Serilog日志服务实现
    /// </summary>
    public class SerilogService : ILoggingService
    {
        private readonly ILogger _logger;

        public SerilogService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 记录调试信息
        /// </summary>
        public void LogDebug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        public void LogInformation(string message, params object[] args)
        {
            _logger.Information(message, args);
        }

        /// <summary>
        /// 记录警告信息
        /// </summary>
        public void LogWarning(string message, params object[] args)
        {
            _logger.Warning(message, args);
        }

        /// <summary>
        /// 记录错误信息
        /// </summary>
        public void LogError(string message, Exception? exception = null, params object[] args)
        {
            if (exception != null)
            {
                _logger.Error(exception, message, args);
            }
            else
            {
                _logger.Error(message, args);
            }
        }

        /// <summary>
        /// 记录严重错误信息
        /// </summary>
        public void LogCritical(string message, Exception? exception = null, params object[] args)
        {
            if (exception != null)
            {
                _logger.Fatal(exception, message, args);
            }
            else
            {
                _logger.Fatal(message, args);
            }
        }

        /// <summary>
        /// 记录结构化日志
        /// </summary>
        public void LogStructured(Services.LogLevel level, string message, Dictionary<string, object>? properties = null, Exception? exception = null)
        {
            var serilogLevel = ConvertLogLevel(level);
            
            if (properties != null && properties.Count > 0)
            {
                var enrichedLogger = _logger;
                foreach (var prop in properties)
                {
                    enrichedLogger = enrichedLogger.ForContext(prop.Key, prop.Value);
                }
                
                if (exception != null)
                {
                    enrichedLogger.Write(serilogLevel, exception, message);
                }
                else
                {
                    enrichedLogger.Write(serilogLevel, message);
                }
            }
            else
            {
                if (exception != null)
                {
                    _logger.Write(serilogLevel, exception, message);
                }
                else
                {
                    _logger.Write(serilogLevel, message);
                }
            }
        }

        /// <summary>
        /// 记录API请求日志
        /// </summary>
        public void LogApiRequest(string method, string path, int statusCode, long duration, string? userAgent = null, string? clientIp = null)
        {
            _logger.ForContext("RequestMethod", method)
                   .ForContext("RequestPath", path)
                   .ForContext("StatusCode", statusCode)
                   .ForContext("Duration", duration)
                   .ForContext("UserAgent", userAgent ?? "Unknown")
                   .ForContext("ClientIP", clientIp ?? "Unknown")
                   .Information("API Request: {Method} {Path} responded {StatusCode} in {Duration}ms", 
                              method, path, statusCode, duration);
        }

        /// <summary>
        /// 记录业务操作日志
        /// </summary>
        public void LogBusinessOperation(string operation, string? userId = null, string? details = null, bool success = true)
        {
            var level = success ? LogEventLevel.Information : LogEventLevel.Warning;
            var status = success ? "Success" : "Failed";
            
            _logger.ForContext("Operation", operation)
                   .ForContext("UserId", userId ?? "Anonymous")
                   .ForContext("Details", details ?? "No details")
                   .ForContext("Success", success)
                   .Write(level, "Business Operation: {Operation} - {Status} (User: {UserId})", 
                         operation, status, userId ?? "Anonymous");
        }

        /// <summary>
        /// 转换日志级别
        /// </summary>
        private static LogEventLevel ConvertLogLevel(Services.LogLevel level)
        {
            return level switch
            {
                Services.LogLevel.Debug => LogEventLevel.Debug,
                Services.LogLevel.Information => LogEventLevel.Information,
                Services.LogLevel.Warning => LogEventLevel.Warning,
                Services.LogLevel.Error => LogEventLevel.Error,
                Services.LogLevel.Critical => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }
    }
}