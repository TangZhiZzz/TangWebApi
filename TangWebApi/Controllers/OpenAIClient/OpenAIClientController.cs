using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAI.Chat;
using SqlSugar;
using System.ClientModel;
using System.Text;
using TangWebApi.Filter;
using TangWebApi.Models;
using TangWebApi.Services;

namespace TangWebApi.Controllers;

/// <summary>
/// OpenAI客户端测试控制器
/// </summary>
[ApiController]
[Route("[controller]")]
[SkipApiResponseFilter]
public class OpenAIClientController : ControllerBase
{
    private readonly IOpenAIClientService _openAIClientService;
    private readonly ILogger<OpenAIClientController> _logger;

    public OpenAIClientController(
        IOpenAIClientService openAIClientService,
        ILogger<OpenAIClientController> logger)
    {
        _openAIClientService = openAIClientService;
        _logger = logger;
    }

    /// <summary>
    /// 测试OpenAI客户端状态
    /// </summary>
    /// <returns>客户端状态信息</returns>
    [HttpGet("status")]
    public object GetClientStatus()
    {
        var client = _openAIClientService.GetClient();
        var isInitialized = _openAIClientService.IsInitialized;

        var status = new
        {
            IsInitialized = isInitialized,
            ClientType = client?.GetType().Name,
            ClientHashCode = client?.GetHashCode(),
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("OpenAI客户端状态检查: {Status}", status);
        return status;

    }
    [HttpPost("complete")]
    public async Task<string> CompleteChat([FromBody] string message = "你是什么模型")
    {
        ChatCompletion completion = await _openAIClientService.GetClient().GetChatClient("deepseek-ai/DeepSeek-R1-Distill-Qwen-7B").CompleteChatAsync(message);

        return completion.Content[0].Text;
    }
    [HttpPost("completeStreamAsync")]
    public async Task CompleteChatStreamAsync([FromBody] string message = "你是什么模型")
    {
        Response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("X-Accel-Buffering", "no");
        AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = _openAIClientService.GetClient().GetChatClient("deepseek-ai/DeepSeek-R1-Distill-Qwen-7B").CompleteChatStreamingAsync(message);

        Console.Write($"[ASSISTANT]: ");
        await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
        {
            if (completionUpdate.ContentUpdate.Count > 0)
            {
                Console.Write(completionUpdate.ContentUpdate[0].Text);
                byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(completionUpdate) + "\n\n");
                await Response.Body.WriteAsync(data);
                await Response.Body.FlushAsync();

            }
        }
    }
}