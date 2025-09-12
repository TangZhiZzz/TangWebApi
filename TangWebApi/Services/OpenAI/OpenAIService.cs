using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Diagnostics;
using TangWebApi.Models;
using TangWebApi.Options;
using ChatMessage = TangWebApi.Models.ChatMessage;

namespace TangWebApi.Services
{
    /// <summary>
    /// OpenAI服务实现类
    /// </summary>
    public class OpenAIService : IOpenAIService
    {
        private readonly OpenAISettings _settings;
        private readonly ILogger<OpenAIService> _logger;
        private readonly ChatClient _chatClient;

        public OpenAIService(IOptions<OpenAISettings> settings, ILogger<OpenAIService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            // 初始化ChatClient
            _chatClient = new ChatClient(_settings.DefaultModel, _settings.ApiKey);
        }

        /// <summary>
        /// 发送聊天消息
        /// </summary>
        public async Task<string> SendChatMessageAsync(string message, string? model = null, int? maxTokens = null, double? temperature = null)
        {
            if (!_settings.Enabled)
            {
                throw new InvalidOperationException("OpenAI服务未启用");
            }

            try
            {
                var stopwatch = Stopwatch.StartNew();

                // 创建聊天选项
                var options = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = maxTokens ?? _settings.MaxTokens,
                    Temperature = (float)(temperature ?? _settings.Temperature)
                };

                // 使用指定模型或默认模型
                var clientToUse = string.IsNullOrEmpty(model) || model == _settings.DefaultModel
                    ? _chatClient
                    : new ChatClient(model, _settings.ApiKey);

                var userMessage = new UserChatMessage(message);
                var completion = await clientToUse.CompleteChatAsync([userMessage], options);

                stopwatch.Stop();

                _logger.LogInformation("OpenAI聊天完成，耗时: {ElapsedMs}ms, 模型: {Model}, 令牌数: {Tokens}",
                    stopwatch.ElapsedMilliseconds, model ?? _settings.DefaultModel, completion.Value.Usage?.TotalTokenCount ?? 0);

                return completion.Value.Content.FirstOrDefault()?.Text ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI聊天请求失败: {Message}", ex.Message);
                throw new InvalidOperationException($"OpenAI请求失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 发送聊天消息（带历史记录）
        /// </summary>
        public async Task<string> SendChatMessagesAsync(List<ChatMessage> messages, string? model = null, int? maxTokens = null, double? temperature = null)
        {
            if (!_settings.Enabled)
            {
                throw new InvalidOperationException("OpenAI服务未启用");
            }

            try
            {
                var stopwatch = Stopwatch.StartNew();

                // 转换消息格式
                var chatMessages = messages.Select(m => m.Role.ToLower() switch
                {
                    "system" => (OpenAI.Chat.ChatMessage)new SystemChatMessage(m.Content),
                    "user" => (OpenAI.Chat.ChatMessage)new UserChatMessage(m.Content),
                    "assistant" => (OpenAI.Chat.ChatMessage)new AssistantChatMessage(m.Content),
                    _ => (OpenAI.Chat.ChatMessage)new UserChatMessage(m.Content)
                }).ToList();

                // 创建聊天选项
                var options = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = maxTokens ?? _settings.MaxTokens,
                    Temperature = (float)(temperature ?? _settings.Temperature)
                };

                // 使用指定模型或默认模型
                var clientToUse = string.IsNullOrEmpty(model) || model == _settings.DefaultModel
                    ? _chatClient
                    : new ChatClient(model, _settings.ApiKey);

                var completion = await clientToUse.CompleteChatAsync(chatMessages, options);

                stopwatch.Stop();

                _logger.LogInformation("OpenAI多轮聊天完成，耗时: {ElapsedMs}ms, 模型: {Model}, 消息数: {MessageCount}, 令牌数: {Tokens}",
                    stopwatch.ElapsedMilliseconds, model ?? _settings.DefaultModel, messages.Count, completion.Value.Usage?.TotalTokenCount ?? 0);

                return completion.Value.Content.FirstOrDefault()?.Text ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI多轮聊天请求失败: {Message}", ex.Message);
                throw new InvalidOperationException($"OpenAI请求失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 生成文本补全
        /// </summary>
        public async Task<string> GenerateCompletionAsync(string prompt, string? model = null, int? maxTokens = null, double? temperature = null)
        {
            if (!_settings.Enabled)
            {
                throw new InvalidOperationException("OpenAI服务未启用");
            }

            try
            {
                var stopwatch = Stopwatch.StartNew();

                // 创建聊天选项
                var options = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = maxTokens ?? _settings.MaxTokens,
                    Temperature = (float)(temperature ?? _settings.Temperature)
                };

                // 使用指定模型或默认模型
                var clientToUse = string.IsNullOrEmpty(model) || model == _settings.DefaultModel
                    ? _chatClient
                    : new ChatClient(model, _settings.ApiKey);

                var userMessage = new UserChatMessage(prompt);
                var completion = await clientToUse.CompleteChatAsync([userMessage], options);

                stopwatch.Stop();

                _logger.LogInformation("OpenAI文本补全完成，耗时: {ElapsedMs}ms, 模型: {Model}, 令牌数: {Tokens}",
                    stopwatch.ElapsedMilliseconds, model ?? _settings.DefaultModel, completion.Value.Usage?.TotalTokenCount ?? 0);

                return completion.Value.Content.FirstOrDefault()?.Text ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI文本补全请求失败: {Message}", ex.Message);
                throw new InvalidOperationException($"OpenAI请求失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 生成图像描述
        /// </summary>
        public async Task<string> DescribeImageAsync(string imageUrl, string prompt = "请描述这张图片", string? model = null)
        {
            if (!_settings.Enabled)
            {
                throw new InvalidOperationException("OpenAI服务未启用");
            }

            try
            {
                var stopwatch = Stopwatch.StartNew();

                // 使用支持视觉的模型
                var visionModel = model ?? "gpt-4o";
                var clientToUse = new ChatClient(visionModel, _settings.ApiKey);

                // 创建包含图像的消息
                var userMessage = new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart(prompt),
                    ChatMessageContentPart.CreateImagePart(new Uri(imageUrl))
                );

                var completion = await clientToUse.CompleteChatAsync([userMessage]);

                stopwatch.Stop();

                _logger.LogInformation("OpenAI图像描述完成，耗时: {ElapsedMs}ms, 模型: {Model}, 令牌数: {Tokens}",
                    stopwatch.ElapsedMilliseconds, visionModel, completion.Value.Usage?.TotalTokenCount ?? 0);

                return completion.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI图像描述请求失败: {Message}", ex.Message);
                throw new InvalidOperationException($"OpenAI请求失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查服务是否可用
        /// </summary>
        public async Task<bool> IsServiceAvailableAsync()
        {
            if (!_settings.Enabled)
            {
                return false;
            }

            try
            {
                // 发送一个简单的测试请求
                var testMessage = new UserChatMessage("Hello");
                var completion = await _chatClient.CompleteChatAsync([testMessage], new ChatCompletionOptions
                {
                    MaxOutputTokenCount = 10
                });

                return !string.IsNullOrEmpty(completion.Value.Content.FirstOrDefault()?.Text);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OpenAI服务可用性检查失败: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 获取可用模型列表
        /// </summary>
        public async Task<List<string>> GetAvailableModelsAsync()
        {
            if (!_settings.Enabled)
            {
                return new List<string>();
            }

            try
            {
                // 返回常用的模型列表（实际项目中可以通过API获取）
                var models = new List<string>
                {
                    "gpt-4o",
                    "gpt-4o-mini",
                    "gpt-4-turbo",
                    "gpt-4",
                    "gpt-3.5-turbo",
                    "gpt-3.5-turbo-16k"
                };

                _logger.LogInformation("获取可用模型列表成功，共 {Count} 个模型", models.Count);
                return models;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取可用模型列表失败: {Message}", ex.Message);
                return new List<string> { _settings.DefaultModel };
            }
        }

        /// <summary>
        /// 发送流式聊天消息
        /// </summary>
        public async IAsyncEnumerable<OpenAIStreamChunk> SendStreamChatMessageAsync(string message, string? model = null, int? maxTokens = null, double? temperature = null, List<ChatMessage>? history = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var chunk in SendStreamChatMessageInternalAsync(message, model, maxTokens, temperature, history, cancellationToken))
            {
                yield return chunk;
            }
        }

        /// <summary>
        /// 内部流式聊天消息实现
        /// </summary>
        private async IAsyncEnumerable<OpenAIStreamChunk> SendStreamChatMessageInternalAsync(string message, string? model, int? maxTokens, double? temperature, List<ChatMessage>? history, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (!_settings.Enabled)
            {
                yield return new OpenAIStreamChunk
                {
                    Error = "OpenAI服务未启用",
                    IsLast = true
                };
                yield break;
            }

            var requestId = Guid.NewGuid().ToString();
            var index = 0;
            var stopwatch = Stopwatch.StartNew();

            // 构建消息列表
            var chatMessages = new List<OpenAI.Chat.ChatMessage>();
            
            // 添加历史消息
            if (history != null)
            {
                chatMessages.AddRange(history.Select(m => m.Role.ToLower() switch
                {
                    "system" => (OpenAI.Chat.ChatMessage)new SystemChatMessage(m.Content),
                    "user" => (OpenAI.Chat.ChatMessage)new UserChatMessage(m.Content),
                    "assistant" => (OpenAI.Chat.ChatMessage)new AssistantChatMessage(m.Content),
                    _ => (OpenAI.Chat.ChatMessage)new UserChatMessage(m.Content)
                }));
            }
            
            // 添加当前用户消息
            chatMessages.Add(new UserChatMessage(message));

            // 创建聊天选项
            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = maxTokens ?? _settings.MaxTokens,
                Temperature = (float)(temperature ?? _settings.Temperature)
            };

            // 使用指定模型或默认模型
            var clientToUse = string.IsNullOrEmpty(model) || model == _settings.DefaultModel
                ? _chatClient
                : new ChatClient(model, _settings.ApiKey);

            var streamingCompletion = clientToUse.CompleteChatStreamingAsync(chatMessages, options, cancellationToken);

            await foreach (var update in streamingCompletion.WithCancellation(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var content = update.ContentUpdate.FirstOrDefault()?.Text;
                if (!string.IsNullOrEmpty(content))
                {
                    yield return new OpenAIStreamChunk
                    {
                        Content = content,
                        IsLast = false,
                        Index = index++,
                        Model = model ?? _settings.DefaultModel,
                        RequestId = requestId
                    };
                }
            }

            stopwatch.Stop();
            _logger.LogInformation("OpenAI流式聊天完成，耗时: {ElapsedMs}ms, 模型: {Model}, 数据块数: {ChunkCount}",
                stopwatch.ElapsedMilliseconds, model ?? _settings.DefaultModel, index);

            // 发送结束标记
            yield return new OpenAIStreamChunk
            {
                Content = null,
                IsLast = true,
                Index = index,
                Model = model ?? _settings.DefaultModel,
                RequestId = requestId
            };
        }

        /// <summary>
        /// 发送流式多轮聊天消息
        /// </summary>
        public async IAsyncEnumerable<OpenAIStreamChunk> SendStreamChatMessagesAsync(List<ChatMessage> messages, string? model = null, int? maxTokens = null, double? temperature = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!_settings.Enabled)
            {
                yield return new OpenAIStreamChunk
                {
                    Error = "OpenAI服务未启用",
                    IsLast = true
                };
                yield break;
            }

            var requestId = Guid.NewGuid().ToString();
            var index = 0;
            Exception? caughtException = null;

            var enumerable = SendStreamChatMessagesInternalAsync(messages, model, maxTokens, temperature, requestId, cancellationToken);
            
            await foreach (var chunk in enumerable.ConfigureAwait(false))
            {
                if (caughtException != null) break;
                
                index = chunk.Index;
                yield return chunk;
            }

            if (caughtException != null)
            {
                _logger.LogError(caughtException, "OpenAI流式多轮聊天请求失败: {Message}", caughtException.Message);
                yield return new OpenAIStreamChunk
                {
                    Error = $"OpenAI请求失败: {caughtException.Message}",
                    IsLast = true,
                    Index = index,
                    RequestId = requestId
                };
            }
        }

        private async IAsyncEnumerable<OpenAIStreamChunk> SendStreamChatMessagesInternalAsync(List<ChatMessage> messages, string? model, int? maxTokens, double? temperature, string requestId, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var index = 0;
            var stopwatch = Stopwatch.StartNew();

            // 转换消息格式
            var chatMessages = messages.Select(m => m.Role.ToLower() switch
            {
                "system" => (OpenAI.Chat.ChatMessage)new SystemChatMessage(m.Content),
                "user" => (OpenAI.Chat.ChatMessage)new UserChatMessage(m.Content),
                "assistant" => (OpenAI.Chat.ChatMessage)new AssistantChatMessage(m.Content),
                _ => (OpenAI.Chat.ChatMessage)new UserChatMessage(m.Content)
            }).ToList();

            // 创建聊天选项
            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = maxTokens ?? _settings.MaxTokens,
                Temperature = (float)(temperature ?? _settings.Temperature)
            };

            // 使用指定模型或默认模型
            var clientToUse = string.IsNullOrEmpty(model) || model == _settings.DefaultModel
                ? _chatClient
                : new ChatClient(model, _settings.ApiKey);

            var streamingCompletion = clientToUse.CompleteChatStreamingAsync(chatMessages, options, cancellationToken);

            await foreach (var update in streamingCompletion.WithCancellation(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var content = update.ContentUpdate.FirstOrDefault()?.Text;
                if (!string.IsNullOrEmpty(content))
                {
                    yield return new OpenAIStreamChunk
                    {
                        Content = content,
                        IsLast = false,
                        Index = index++,
                        Model = model ?? _settings.DefaultModel,
                        RequestId = requestId
                    };
                }
            }

            stopwatch.Stop();
            _logger.LogInformation("OpenAI流式多轮聊天完成，耗时: {ElapsedMs}ms, 模型: {Model}, 消息数: {MessageCount}, 数据块数: {ChunkCount}",
                stopwatch.ElapsedMilliseconds, model ?? _settings.DefaultModel, messages.Count, index);

            // 发送结束标记
            yield return new OpenAIStreamChunk
            {
                Content = null,
                IsLast = true,
                Index = index,
                Model = model ?? _settings.DefaultModel,
                RequestId = requestId
            };
        }

        /// <summary>
        /// 生成流式文本补全
        /// </summary>
        public async IAsyncEnumerable<OpenAIStreamChunk> GenerateStreamCompletionAsync(string prompt, string? model = null, int? maxTokens = null, double? temperature = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!_settings.Enabled)
            {
                yield return new OpenAIStreamChunk
                {
                    Error = "OpenAI服务未启用",
                    IsLast = true
                };
                yield break;
            }

            var requestId = Guid.NewGuid().ToString();
            var index = 0;
            
            var enumerable = GenerateStreamCompletionInternalAsync(prompt, model, maxTokens, temperature, requestId, cancellationToken);
            
            await foreach (var chunk in enumerable)
            {
                index = chunk.Index;
                yield return chunk;
            }
        }

        private async IAsyncEnumerable<OpenAIStreamChunk> GenerateStreamCompletionInternalAsync(string prompt, string? model, int? maxTokens, double? temperature, string requestId, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var index = 0;
            var stopwatch = Stopwatch.StartNew();

            // 创建聊天选项
            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = maxTokens ?? _settings.MaxTokens,
                Temperature = (float)(temperature ?? _settings.Temperature)
            };

            // 使用指定模型或默认模型
            var clientToUse = string.IsNullOrEmpty(model) || model == _settings.DefaultModel
                ? _chatClient
                : new ChatClient(model, _settings.ApiKey);

            var userMessage = new UserChatMessage(prompt);
            var streamingCompletion = clientToUse.CompleteChatStreamingAsync([userMessage], options, cancellationToken);

            await foreach (var update in streamingCompletion.WithCancellation(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var content = update.ContentUpdate.FirstOrDefault()?.Text;
                if (!string.IsNullOrEmpty(content))
                {
                    yield return new OpenAIStreamChunk
                    {
                        Content = content,
                        IsLast = false,
                        Index = index++,
                        Model = model ?? _settings.DefaultModel,
                        RequestId = requestId
                    };
                }
            }

            stopwatch.Stop();
            _logger.LogInformation("OpenAI流式文本补全完成，耗时: {ElapsedMs}ms, 模型: {Model}, 数据块数: {ChunkCount}",
                stopwatch.ElapsedMilliseconds, model ?? _settings.DefaultModel, index);

            // 发送结束标记
            yield return new OpenAIStreamChunk
            {
                Content = null,
                IsLast = true,
                Index = index,
                Model = model ?? _settings.DefaultModel,
                RequestId = requestId
            };
        }
    }
}