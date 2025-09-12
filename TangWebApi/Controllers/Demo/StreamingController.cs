using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using TangWebApi.Filter;

namespace TangWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [SkipApiResponseFilter]
    public class StreamingController : ControllerBase
    {
        private readonly ILogger<StreamingController> _logger;

        public StreamingController(ILogger<StreamingController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 流式文本输出 - 使用分块传输编码
        /// </summary>
        [HttpGet("text")]
        public async Task GetTextStream()
        {
            Response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("X-Accel-Buffering", "no"); // 禁用代理缓冲

            try
            {
                for (int i = 0; i < 5; i++)
                {
                    string chunk = $"第 {i + 1} 条消息 - 时间: {DateTime.Now:HH:mm:ss}\\n";
                    byte[] data = Encoding.UTF8.GetBytes(chunk);
                    await Response.Body.WriteAsync(data);
                    await Response.Body.FlushAsync();
                    await Task.Delay(1000); // 每秒发送一条消息
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "流式文本输出错误");
            }
        }

        /// <summary>
        /// 流式JSON数据输出
        /// </summary>
        [HttpGet("json")]
        public async IAsyncEnumerable<StreamData> GetJsonStream()
        {
            for (int i = 0; i < 10; i++)
            {
                yield return new StreamData
                {
                    Id = i + 1,
                    Message = $"流式消息 {i + 1}",
                    Timestamp = DateTime.Now,
                    Value = Random.Shared.Next(100)
                };
                await Task.Delay(800); // 每800毫秒发送一条数据
            }
        }

        /// <summary>
        /// 服务器发送事件(SSE) - 实时数据推送
        /// </summary>
        [HttpGet("sse")]
        public async Task GetServerSentEvents()
        {
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            Response.Headers.Add("X-Accel-Buffering", "no");

            var clientId = Guid.NewGuid().ToString();
            
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    var data = new
                    {
                        id = i,
                        message = $"SSE消息 {i + 1} 给客户端 {clientId}",
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        randomNumber = Random.Shared.Next(1000)
                    };

                    string jsonData = JsonSerializer.Serialize(data);
                    string sseData = $"id: {i}\nevent: message\ndata: {jsonData}\n\n";
                    
                    byte[] bytes = Encoding.UTF8.GetBytes(sseData);
                    await Response.Body.WriteAsync(bytes);
                    await Response.Body.FlushAsync();
                    
                    await Task.Delay(1000); // 每1秒发送一次
                }

                // 发送结束消息
                string endMessage = "event: complete\ndata: {\"status\":\"completed\"}\n\n";
                byte[] endBytes = Encoding.UTF8.GetBytes(endMessage);
                await Response.Body.WriteAsync(endBytes);
                await Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSE流式输出错误");
            }
        }

        /// <summary>
        /// 大文件流式下载
        /// </summary>
        [HttpGet("download")]
        public async Task<IActionResult> DownloadLargeFile()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            // 模拟生成大量数据
            for (int i = 0; i < 1000; i++)
            {
                await writer.WriteLineAsync($"数据行 {i + 1}: {Guid.NewGuid()} - {DateTime.Now}");
            }

            await writer.FlushAsync();
            stream.Position = 0;

            return File(stream, "text/plain", "large-file.txt");
        }

        /// <summary>
        /// 实时进度流 - 模拟长时间运行的任务进度
        /// </summary>
        [HttpGet("progress")]
        public async Task GetProgressStream()
        {
            Response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("X-Accel-Buffering", "no");

            try
            {
                for (int progress = 0; progress <= 100; progress += 10)
                {
                    string message = $"data: {{\"progress\":{progress},\"message\":\"任务执行中...\"}}\n\n";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await Response.Body.WriteAsync(data);
                    await Response.Body.FlushAsync();
                    
                    if (progress < 100)
                        await Task.Delay(800);
                }

                string completeMessage = "data: {\"progress\":100,\"message\":\"任务完成！\"}\n\n";
                byte[] completeData = Encoding.UTF8.GetBytes(completeMessage);
                await Response.Body.WriteAsync(completeData);
                await Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "进度流输出错误");
            }
        }

        /// <summary>
        /// 简单的测试流 - 返回固定格式的数据
        /// </summary>
        [HttpGet("test")]
        public async Task GetTestStream()
        {
            Response.Headers.Add("Content-Type", "application/json");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("X-Accel-Buffering", "no");

            try
            {
                for (int i = 1; i <= 3; i++)
                {
                    var data = new { index = i, message = $"测试消息 {i}", timestamp = DateTime.Now };
                    var json = JsonSerializer.Serialize(data) + "\n";
                    var bytes = Encoding.UTF8.GetBytes(json);
                    
                    await Response.Body.WriteAsync(bytes);
                    await Response.Body.FlushAsync();
                    
                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试流输出错误");
            }
        }
    }

    public class StreamData
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int Value { get; set; }
    }
}