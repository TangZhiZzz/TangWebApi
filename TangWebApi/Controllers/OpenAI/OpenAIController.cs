using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TangWebApi.Models;
using TangWebApi.Services;

namespace TangWebApi.Controllers
{
    /// <summary>
    /// OpenAI服务控制器
    /// </summary>
    [ApiController]
    [Route("api/OpenAI")]
    [Authorize]
    public class OpenAIController : ControllerBase
    {
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<OpenAIController> _logger;

        public OpenAIController(IOpenAIService openAIService, ILogger<OpenAIController> logger)
        {
            _openAIService = openAIService;
            _logger = logger;
        }

        /// <summary>
        /// 发送聊天消息
        /// </summary>
        /// <param name="request">聊天请求</param>
        /// <returns>聊天响应</returns>
        [HttpPost("chat")]
        public async Task<OpenAIChatResponse> SendChatMessage([FromBody] OpenAIChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                throw new ArgumentException("聊天消息不能为空");
            }

            var response = await _openAIService.SendChatMessageAsync(
                request.Message,
                request.Model,
                request.MaxTokens,
                request.Temperature
            );

            var result = new OpenAIChatResponse
            {
                Content = response,
                Model = request.Model ?? "gpt-3.5-turbo",
                Usage = new OpenAIUsage
                {
                    TotalTokens = 0 // 实际项目中应该从API响应中获取
                }
            };

            _logger.LogInformation("聊天消息发送成功，用户: {UserId}", User.Identity?.Name);
            return result;
        }

        /// <summary>
        /// 发送多轮聊天消息
        /// </summary>
        /// <param name="request">多轮聊天请求</param>
        /// <returns>聊天响应</returns>
        [HttpPost("chat/messages")]
        public async Task<OpenAIChatResponse> SendChatMessages([FromBody] OpenAIMultiChatRequest request)
        {
            if (request?.Messages == null || !request.Messages.Any())
            {
                throw new ArgumentException("消息列表不能为空");
            }

            var response = await _openAIService.SendChatMessagesAsync(
                request.Messages,
                request.Model,
                request.MaxTokens,
                request.Temperature
            );

            var result = new OpenAIChatResponse
            {
                Content = response,
                Model = request.Model ?? "gpt-3.5-turbo",
                Usage = new OpenAIUsage
                {
                    TotalTokens = 0 // 实际项目中应该从API响应中获取
                }
            };

            _logger.LogInformation("多轮聊天消息发送成功，消息数: {Count}，用户: {UserId}", 
                request.Messages.Count, User.Identity?.Name);
            return result;
        }

        /// <summary>
        /// 生成文本补全
        /// </summary>
        /// <param name="request">文本补全请求</param>
        /// <returns>补全响应</returns>
        [HttpPost("completion")]
        public async Task<OpenAIChatResponse> GenerateCompletion([FromBody] OpenAICompletionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Prompt))
            {
                throw new ArgumentException("提示文本不能为空");
            }

            var response = await _openAIService.GenerateCompletionAsync(
                request.Prompt,
                request.Model,
                request.MaxTokens,
                request.Temperature
            );

            var result = new OpenAIChatResponse
            {
                Content = response,
                Model = request.Model ?? "gpt-3.5-turbo",
                Usage = new OpenAIUsage
                {
                    TotalTokens = 0 // 实际项目中应该从API响应中获取
                }
            };

            _logger.LogInformation("文本补全生成成功，用户: {UserId}", User.Identity?.Name);
            return result;
        }

        /// <summary>
        /// 生成图像描述
        /// </summary>
        /// <param name="request">图像描述请求</param>
        /// <returns>描述响应</returns>
        [HttpPost("vision/describe")]
        public async Task<OpenAIChatResponse> DescribeImage([FromBody] OpenAIVisionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ImageUrl))
            {
                throw new ArgumentException("图像URL不能为空");
            }

            var response = await _openAIService.DescribeImageAsync(
                request.ImageUrl,
                request.Prompt ?? "请描述这张图片",
                request.Model
            );

            var result = new OpenAIChatResponse
            {
                Content = response,
                Model = request.Model ?? "gpt-4o",
                Usage = new OpenAIUsage
                {
                    TotalTokens = 0 // 实际项目中应该从API响应中获取
                }
            };

            _logger.LogInformation("图像描述生成成功，用户: {UserId}", User.Identity?.Name);
            return result;
        }

        /// <summary>
        /// 检查OpenAI服务状态
        /// </summary>
        /// <returns>服务状态</returns>
        [HttpGet("status")]
        public async Task<object> GetServiceStatus()
        {
            var isAvailable = await _openAIService.IsServiceAvailableAsync();
            var models = await _openAIService.GetAvailableModelsAsync();

            var status = new
            {
                IsAvailable = isAvailable,
                AvailableModels = models,
                CheckTime = DateTime.UtcNow
            };

            _logger.LogInformation("OpenAI服务状态检查完成，可用: {IsAvailable}", isAvailable);
            return status;
        }

        /// <summary>
        /// 获取可用模型列表
        /// </summary>
        /// <returns>模型列表</returns>
        [HttpGet("models")]
        public async Task<List<string>> GetAvailableModels()
        {
            var models = await _openAIService.GetAvailableModelsAsync();

            _logger.LogInformation("获取可用模型列表成功，共 {Count} 个模型", models.Count);
            return models;
        }

        /// <summary>
        /// 发送流式聊天消息
        /// </summary>
        /// <param name="request">流式聊天请求</param>
        /// <returns>流式响应</returns>
        [HttpPost("stream/chat")]
        public async Task StreamChatMessage([FromBody] OpenAIStreamChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                throw new ArgumentException("消息内容不能为空");

            Response.ContentType = "text/event-stream";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            Response.Headers.Add("Access-Control-Allow-Origin", "*");

            try
            {
                await foreach (var chunk in _openAIService.SendStreamChatMessageAsync(
                    request.Message,
                    request.Model,
                    request.MaxTokens,
                    request.Temperature,
                    request.History,
                    HttpContext.RequestAborted))
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(chunk);
                    await Response.WriteAsync($"data: {json}\n\n");
                    await Response.Body.FlushAsync();

                    if (chunk.IsLast)
                        break;
                }

                _logger.LogInformation("流式聊天消息发送成功，用户: {UserId}", User.Identity?.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "流式聊天请求失败: {Message}", ex.Message);
                var errorChunk = new OpenAIStreamChunk
                {
                    Error = $"请求失败: {ex.Message}",
                    IsLast = true
                };
                var errorJson = System.Text.Json.JsonSerializer.Serialize(errorChunk);
                await Response.WriteAsync($"data: {errorJson}\n\n");
                await Response.Body.FlushAsync();
            }
        }

        /// <summary>
        /// 发送流式多轮聊天消息
        /// </summary>
        /// <param name="request">流式多轮聊天请求</param>
        /// <returns>流式响应</returns>
        [HttpPost("stream/chat/messages")]
        public async Task StreamChatMessages([FromBody] OpenAIStreamMultiChatRequest request)
        {
            if (request?.Messages == null || !request.Messages.Any())
                throw new ArgumentException("消息列表不能为空");

            Response.ContentType = "text/event-stream";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            Response.Headers.Add("Access-Control-Allow-Origin", "*");

            try
            {
                await foreach (var chunk in _openAIService.SendStreamChatMessagesAsync(
                    request.Messages,
                    request.Model,
                    request.MaxTokens,
                    request.Temperature,
                    HttpContext.RequestAborted))
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(chunk);
                    await Response.WriteAsync($"data: {json}\n\n");
                    await Response.Body.FlushAsync();

                    if (chunk.IsLast)
                        break;
                }

                _logger.LogInformation("流式多轮聊天消息发送成功，消息数: {Count}，用户: {UserId}", 
                    request.Messages.Count, User.Identity?.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "流式多轮聊天请求失败: {Message}", ex.Message);
                var errorChunk = new OpenAIStreamChunk
                {
                    Error = $"请求失败: {ex.Message}",
                    IsLast = true
                };
                var errorJson = System.Text.Json.JsonSerializer.Serialize(errorChunk);
                await Response.WriteAsync($"data: {errorJson}\n\n");
                await Response.Body.FlushAsync();
            }
        }

        /// <summary>
        /// 生成流式文本补全
        /// </summary>
        /// <param name="prompt">提示文本</param>
        /// <param name="model">使用的模型</param>
        /// <param name="maxTokens">最大令牌数</param>
        /// <param name="temperature">温度参数</param>
        /// <returns>流式响应</returns>
        [HttpPost("stream/completion")]
        public async Task StreamCompletion(
            [FromQuery] string prompt,
            [FromQuery] string? model = null,
            [FromQuery] int? maxTokens = null,
            [FromQuery] double? temperature = null)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("提示文本不能为空");

            Response.ContentType = "text/event-stream";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            Response.Headers.Add("Access-Control-Allow-Origin", "*");

            try
            {
                await foreach (var chunk in _openAIService.GenerateStreamCompletionAsync(
                    prompt,
                    model,
                    maxTokens,
                    temperature,
                    HttpContext.RequestAborted))
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(chunk);
                    await Response.WriteAsync($"data: {json}\n\n");
                    await Response.Body.FlushAsync();

                    if (chunk.IsLast)
                        break;
                }

                _logger.LogInformation("流式文本补全生成成功，用户: {UserId}", User.Identity?.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "流式文本补全请求失败: {Message}", ex.Message);
                var errorChunk = new OpenAIStreamChunk
                {
                    Error = $"请求失败: {ex.Message}",
                    IsLast = true
                };
                var errorJson = System.Text.Json.JsonSerializer.Serialize(errorChunk);
                await Response.WriteAsync($"data: {errorJson}\n\n");
                await Response.Body.FlushAsync();
            }
        }
    }
}