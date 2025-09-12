using TangWebApi.Models;

namespace TangWebApi.Services
{
    /// <summary>
    /// OpenAI服务接口
    /// </summary>
    public interface IOpenAIService
    {
        /// <summary>
        /// 发送聊天消息
        /// </summary>
        /// <param name="message">用户消息</param>
        /// <param name="model">使用的模型，为空时使用默认模型</param>
        /// <param name="maxTokens">最大令牌数，为空时使用配置默认值</param>
        /// <param name="temperature">温度参数，为空时使用配置默认值</param>
        /// <returns>AI回复</returns>
        Task<string> SendChatMessageAsync(string message, string? model = null, int? maxTokens = null, double? temperature = null);

        /// <summary>
        /// 发送聊天消息（带历史记录）
        /// </summary>
        /// <param name="messages">消息历史记录</param>
        /// <param name="model">使用的模型，为空时使用默认模型</param>
        /// <param name="maxTokens">最大令牌数，为空时使用配置默认值</param>
        /// <param name="temperature">温度参数，为空时使用配置默认值</param>
        /// <returns>AI回复</returns>
        Task<string> SendChatMessagesAsync(List<ChatMessage> messages, string? model = null, int? maxTokens = null, double? temperature = null);

        /// <summary>
        /// 生成文本补全
        /// </summary>
        /// <param name="prompt">提示文本</param>
        /// <param name="model">使用的模型，为空时使用默认模型</param>
        /// <param name="maxTokens">最大令牌数，为空时使用配置默认值</param>
        /// <param name="temperature">温度参数，为空时使用配置默认值</param>
        /// <returns>生成的文本</returns>
        Task<string> GenerateCompletionAsync(string prompt, string? model = null, int? maxTokens = null, double? temperature = null);

        /// <summary>
        /// 生成图像描述
        /// </summary>
        /// <param name="imageUrl">图像URL</param>
        /// <param name="prompt">描述提示</param>
        /// <param name="model">使用的模型，为空时使用默认模型</param>
        /// <returns>图像描述</returns>
        Task<string> DescribeImageAsync(string imageUrl, string prompt = "请描述这张图片", string? model = null);

        /// <summary>
        /// 检查服务是否可用
        /// </summary>
        /// <returns>服务状态</returns>
        Task<bool> IsServiceAvailableAsync();

        /// <summary>
        /// 获取可用模型列表
        /// </summary>
        /// <returns>模型列表</returns>
        Task<List<string>> GetAvailableModelsAsync();

        /// <summary>
        /// 发送流式聊天消息
        /// </summary>
        /// <param name="message">用户消息</param>
        /// <param name="model">使用的模型，为空时使用默认模型</param>
        /// <param name="maxTokens">最大令牌数，为空时使用配置默认值</param>
        /// <param name="temperature">温度参数，为空时使用配置默认值</param>
        /// <param name="history">消息历史记录</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>流式响应数据块的异步枚举</returns>
        IAsyncEnumerable<OpenAIStreamChunk> SendStreamChatMessageAsync(string message, string? model = null, int? maxTokens = null, double? temperature = null, List<ChatMessage>? history = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 发送流式多轮聊天消息
        /// </summary>
        /// <param name="messages">消息历史记录</param>
        /// <param name="model">使用的模型，为空时使用默认模型</param>
        /// <param name="maxTokens">最大令牌数，为空时使用配置默认值</param>
        /// <param name="temperature">温度参数，为空时使用配置默认值</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>流式响应数据块的异步枚举</returns>
        IAsyncEnumerable<OpenAIStreamChunk> SendStreamChatMessagesAsync(List<ChatMessage> messages, string? model = null, int? maxTokens = null, double? temperature = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 生成流式文本补全
        /// </summary>
        /// <param name="prompt">提示文本</param>
        /// <param name="model">使用的模型，为空时使用默认模型</param>
        /// <param name="maxTokens">最大令牌数，为空时使用配置默认值</param>
        /// <param name="temperature">温度参数，为空时使用配置默认值</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>流式响应数据块的异步枚举</returns>
        IAsyncEnumerable<OpenAIStreamChunk> GenerateStreamCompletionAsync(string prompt, string? model = null, int? maxTokens = null, double? temperature = null, CancellationToken cancellationToken = default);
    }
}