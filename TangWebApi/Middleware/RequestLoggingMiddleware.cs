using System.Diagnostics;
using System.Text;
using TangWebApi.Services;

namespace TangWebApi.Middleware
{
    /// <summary>
    /// 请求日志中间件
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggingService _loggingService;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILoggingService loggingService, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _loggingService = loggingService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 跳过静态文件和健康检查
            if (ShouldSkipLogging(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                // 记录请求信息
                await LogRequestAsync(context.Request);

                // 检查是否为流式输出端点，如果是则不缓存响应
                if (IsStreamingEndpoint(context.Request.Path))
                {
                    // 对于流式输出，直接执行下一个中间件，不缓存响应
                    await _next(context);
                    stopwatch.Stop();
                    return;
                }

                // 创建内存流来捕获响应
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // 执行下一个中间件
                await _next(context);

                stopwatch.Stop();

                // 记录响应信息
                LogResponse(context, stopwatch.ElapsedMilliseconds);

                // 将响应写回原始流
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // 记录异常
                _loggingService.LogError("Request processing failed for {Method} {Path}", ex, 
                    context.Request.Method, context.Request.Path);
                
                // 记录API请求（失败）
                _loggingService.LogApiRequest(
                    context.Request.Method,
                    context.Request.Path,
                    500,
                    stopwatch.ElapsedMilliseconds,
                    context.Request.Headers.UserAgent.ToString(),
                    GetClientIpAddress(context));
                
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        /// <summary>
        /// 记录请求信息
        /// </summary>
        private async Task LogRequestAsync(HttpRequest request)
        {
            try
            {
                var requestInfo = new StringBuilder();
                requestInfo.AppendLine($"Request: {request.Method} {request.Path}{request.QueryString}");
                requestInfo.AppendLine($"Headers: {string.Join(", ", request.Headers.Select(h => $"{h.Key}={h.Value}"))}");
                requestInfo.AppendLine($"ContentType: {request.ContentType}");
                requestInfo.AppendLine($"ContentLength: {request.ContentLength}");
                requestInfo.AppendLine($"UserAgent: {request.Headers.UserAgent}");
                requestInfo.AppendLine($"ClientIP: {GetClientIpAddress(request.HttpContext)}");

                // 记录请求体（仅对POST/PUT/PATCH且内容不太大的请求）
                if (ShouldLogRequestBody(request))
                {
                    request.EnableBuffering();
                    var buffer = new byte[Convert.ToInt32(request.ContentLength ?? 0)];
                    await request.Body.ReadExactlyAsync(buffer, 0, buffer.Length);
                    var requestBody = Encoding.UTF8.GetString(buffer);
                    request.Body.Position = 0;
                    
                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        requestInfo.AppendLine($"Body: {requestBody}");
                    }
                }

                _loggingService.LogDebug(requestInfo.ToString());
            }
            catch (Exception ex)
            {
                _loggingService.LogWarning("Failed to log request details: {Error}", ex.Message);
            }
        }

        /// <summary>
        /// 记录响应信息
        /// </summary>
        private void LogResponse(HttpContext context, long elapsedMilliseconds)
        {
            try
            {
                var response = context.Response;
                var request = context.Request;

                // 记录API请求日志
                var userAgent = request.Headers["User-Agent"].FirstOrDefault();
                var clientIp = GetClientIpAddress(context);
                
                _loggingService.LogApiRequest(
                    request.Method,
                    request.Path,
                    response.StatusCode,
                    elapsedMilliseconds,
                    userAgent,
                    clientIp);

                // 详细响应信息（仅在Debug级别）
                var responseInfo = new StringBuilder();
                responseInfo.AppendLine($"Response: {response.StatusCode}");
                responseInfo.AppendLine($"Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={h.Value}"))}");
                responseInfo.AppendLine($"ContentType: {response.ContentType}");
                responseInfo.AppendLine($"Duration: {elapsedMilliseconds}ms");

                _loggingService.LogDebug(responseInfo.ToString());

                // 记录慢请求
                if (elapsedMilliseconds > 5000) // 超过5秒的请求
                {
                    _loggingService.LogWarning("Slow request detected: {Method} {Path} took {Duration}ms", 
                        request.Method, request.Path, elapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogWarning("Failed to log response details: {Error}", ex.Message);
            }
        }

        /// <summary>
        /// 判断是否应该跳过日志记录
        /// </summary>
        private static bool ShouldSkipLogging(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant();
            return pathValue != null && (
                pathValue.StartsWith("/css/") ||
                pathValue.StartsWith("/js/") ||
                pathValue.StartsWith("/images/") ||
                pathValue.StartsWith("/img/") ||
                pathValue.StartsWith("/fonts/") ||
                pathValue.StartsWith("/assets/") ||
                pathValue.StartsWith("/static/") ||
                pathValue.StartsWith("/wwwroot/") ||
                pathValue.StartsWith("/favicon.ico") ||
                pathValue.StartsWith("/robots.txt") ||
                pathValue.StartsWith("/sitemap.xml") ||
                pathValue.StartsWith("/health") ||
                pathValue.StartsWith("/metrics") ||
                pathValue.EndsWith(".css") ||
                pathValue.EndsWith(".js") ||
                pathValue.EndsWith(".png") ||
                pathValue.EndsWith(".jpg") ||
                pathValue.EndsWith(".jpeg") ||
                pathValue.EndsWith(".gif") ||
                pathValue.EndsWith(".svg") ||
                pathValue.EndsWith(".ico") ||
                pathValue.EndsWith(".woff") ||
                pathValue.EndsWith(".woff2") ||
                pathValue.EndsWith(".ttf") ||
                pathValue.EndsWith(".eot"));
        }

        /// <summary>
        /// 判断是否应该记录请求体
        /// </summary>
        private static bool ShouldLogRequestBody(HttpRequest request)
        {
            if (request.ContentLength == null || request.ContentLength > 10240) // 大于10KB不记录
                return false;

            var method = request.Method.ToUpperInvariant();
            if (method != "POST" && method != "PUT" && method != "PATCH")
                return false;

            var contentType = request.ContentType?.ToLowerInvariant();
            return contentType != null && (
                contentType.Contains("application/json") ||
                contentType.Contains("application/xml") ||
                contentType.Contains("text/"));
        }

        /// <summary>
        /// 判断是否为流式输出端点
        /// </summary>
        private static bool IsStreamingEndpoint(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant();
            return pathValue != null && (
                pathValue.StartsWith("/api/streaming/") ||
                pathValue.Contains("/stream") ||
                pathValue.Contains("/sse") ||
                pathValue.Contains("/events"));
        }

        /// <summary>
        /// 记录流式输出响应信息（不包含响应体）
        /// </summary>
        private void LogStreamingResponse(HttpContext context, long elapsedMilliseconds)
        {
            try
            {
                var response = context.Response;
                var request = context.Request;

                // 记录API请求日志
                var userAgent = request.Headers["User-Agent"].FirstOrDefault();
                var clientIp = GetClientIpAddress(context);
                
                _loggingService.LogApiRequest(
                    request.Method,
                    request.Path,
                    response.StatusCode,
                    elapsedMilliseconds,
                    userAgent,
                    clientIp);

                // 详细响应信息（仅在Debug级别）
                var responseInfo = new StringBuilder();
                responseInfo.AppendLine($"Streaming Response: {response.StatusCode}");
                responseInfo.AppendLine($"Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={h.Value}"))}");
                responseInfo.AppendLine($"ContentType: {response.ContentType}");
                responseInfo.AppendLine($"Duration: {elapsedMilliseconds}ms");
                responseInfo.AppendLine("Note: Response body not logged for streaming endpoints");

                _loggingService.LogDebug(responseInfo.ToString());

                // 记录慢请求
                if (elapsedMilliseconds > 5000) // 超过5秒的请求
                {
                    _loggingService.LogWarning("Slow streaming request detected: {Method} {Path} took {Duration}ms", 
                        request.Method, request.Path, elapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogWarning("Failed to log streaming response details: {Error}", ex.Message);
            }
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        private static string GetClientIpAddress(HttpContext context)
        {
            // 尝试从X-Forwarded-For头获取真实IP
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }

            // 尝试从X-Real-IP头获取
            var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            // 返回连接的远程IP
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}