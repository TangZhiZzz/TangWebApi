using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace TangWebApi.Controllers.Demo
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        [HttpGet("test")]
        public async IAsyncEnumerable<int> Test([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < 10; i++)
            {
                // 模拟异步操作
                await Task.Delay(1000, cancellationToken);
                yield return i;
            }
        }

        [HttpGet("test-stream")]
        public async Task TestStream(CancellationToken cancellationToken = default)
        {
            Response.ContentType = "text/plain; charset=utf-8";
            
            await foreach (var item in Test(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                    
                await Response.WriteAsync($"数据: {item}\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }

        [HttpGet("sse-stream")]
        public async Task StreamSSE(CancellationToken cancellationToken = default)
        {
            // 设置SSE响应头
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            
            try
            {
                for (int i = 0; i < 20; i++)
                {
                    // 检查客户端是否断开连接
                    if (cancellationToken.IsCancellationRequested)
                        break;
                    
                    // 构造SSE格式的数据
                    var data = new 
                    {
                        id = i,
                        message = $"这是第 {i + 1} 条流式消息",
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    
                    var jsonData = System.Text.Json.JsonSerializer.Serialize(data);
                    
                    // 发送SSE格式的数据
                    await Response.WriteAsync($"id: {i}\n", cancellationToken);
                    await Response.WriteAsync($"event: message\n", cancellationToken);
                    await Response.WriteAsync($"data: {jsonData}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                    
                    // 模拟处理时间
                    await Task.Delay(1000, cancellationToken);
                }
                
                // 发送结束事件
                await Response.WriteAsync("event: end\n", cancellationToken);
                await Response.WriteAsync("data: {\"message\": \"流式传输完成\"}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // 客户端断开连接，正常结束
            }
        }

        [HttpGet("json-stream")]
        public async Task<IActionResult> JsonStream(CancellationToken cancellationToken = default)
        {
            Response.ContentType = "application/json";
            
            await Response.WriteAsync("[", cancellationToken);
            
            for (int i = 0; i < 10; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                    
                var item = new 
                {
                    index = i,
                    value = $"Item {i}",
                    timestamp = DateTime.Now
                };
                
                var json = System.Text.Json.JsonSerializer.Serialize(item);
                
                if (i > 0)
                    await Response.WriteAsync(",", cancellationToken);
                    
                await Response.WriteAsync(json, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
                
                await Task.Delay(500, cancellationToken);
            }
            
            await Response.WriteAsync("]", cancellationToken);
            return new EmptyResult();
        }
    }
}
