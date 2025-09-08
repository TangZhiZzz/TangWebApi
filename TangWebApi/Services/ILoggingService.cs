using System;
using System.Collections.Generic;

namespace TangWebApi.Services
{
    /// <summary>
    /// 日志服务接口
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="args">格式化参数</param>
        void LogDebug(string message, params object[] args);

        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="args">格式化参数</param>
        void LogInformation(string message, params object[] args);

        /// <summary>
        /// 记录警告信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="args">格式化参数</param>
        void LogWarning(string message, params object[] args);

        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        /// <param name="args">格式化参数</param>
        void LogError(string message, Exception? exception = null, params object[] args);

        /// <summary>
        /// 记录严重错误信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        /// <param name="args">格式化参数</param>
        void LogCritical(string message, Exception? exception = null, params object[] args);

        /// <summary>
        /// 记录结构化日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="properties">结构化属性</param>
        /// <param name="exception">异常对象</param>
        void LogStructured(LogLevel level, string message, Dictionary<string, object>? properties = null, Exception? exception = null);

        /// <summary>
        /// 记录API请求日志
        /// </summary>
        /// <param name="method">HTTP方法</param>
        /// <param name="path">请求路径</param>
        /// <param name="statusCode">响应状态码</param>
        /// <param name="duration">请求耗时(毫秒)</param>
        /// <param name="userAgent">用户代理</param>
        /// <param name="clientIp">客户端IP</param>
        void LogApiRequest(string method, string path, int statusCode, long duration, string? userAgent = null, string? clientIp = null);

        /// <summary>
        /// 记录业务操作日志
        /// </summary>
        /// <param name="operation">操作名称</param>
        /// <param name="userId">用户ID</param>
        /// <param name="details">操作详情</param>
        /// <param name="success">是否成功</param>
        void LogBusinessOperation(string operation, string? userId = null, string? details = null, bool success = true);
    }

    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Information = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }
}