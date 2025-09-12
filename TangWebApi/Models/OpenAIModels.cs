namespace TangWebApi.Models
{
    /// <summary>
    /// 聊天消息模型
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// 消息角色（system, user, assistant）
        /// </summary>
        public required string Role { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public required string Content { get; set; }

        /// <summary>
        /// 消息时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// OpenAI聊天请求模型
    /// </summary>
    public class OpenAIChatRequest
    {
        /// <summary>
        /// 用户消息
        /// </summary>
        public required string Message { get; set; }

        /// <summary>
        /// 使用的模型
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// 最大令牌数
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// 温度参数
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// 消息历史记录
        /// </summary>
        public List<ChatMessage>? History { get; set; }
    }

    /// <summary>
    /// OpenAI使用情况统计
    /// </summary>
    public class OpenAIUsage
    {
        /// <summary>
        /// 总令牌数
        /// </summary>
        public int TotalTokens { get; set; }

        /// <summary>
        /// 提示令牌数
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// 完成令牌数
        /// </summary>
        public int CompletionTokens { get; set; }
    }

    /// <summary>
    /// OpenAI聊天响应模型
    /// </summary>
    public class OpenAIChatResponse
    {
        /// <summary>
        /// AI回复内容
        /// </summary>
        public required string Content { get; set; }

        /// <summary>
        /// 使用的模型
        /// </summary>
        public required string Model { get; set; }

        /// <summary>
        /// 使用情况统计
        /// </summary>
        public OpenAIUsage? Usage { get; set; }

        /// <summary>
        /// 消耗的令牌数
        /// </summary>
        public int TokensUsed { get; set; }

        /// <summary>
        /// 响应时间（毫秒）
        /// </summary>
        public long ResponseTimeMs { get; set; }

        /// <summary>
        /// 请求ID
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 图像描述请求模型
    /// </summary>
    public class ImageDescriptionRequest
    {
        /// <summary>
        /// 图像URL
        /// </summary>
        public required string ImageUrl { get; set; }

        /// <summary>
        /// 描述提示
        /// </summary>
        public string Prompt { get; set; } = "请描述这张图片";

        /// <summary>
        /// 使用的模型
        /// </summary>
        public string? Model { get; set; }
    }

    /// <summary>
    /// 多轮聊天请求模型
    /// </summary>
    public class OpenAIMultiChatRequest
    {
        /// <summary>
        /// 消息历史记录
        /// </summary>
        public required List<ChatMessage> Messages { get; set; }

        /// <summary>
        /// 使用的模型
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// 最大令牌数
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// 温度参数
        /// </summary>
        public double? Temperature { get; set; }
    }

    /// <summary>
    /// 文本补全请求模型
    /// </summary>
    public class OpenAICompletionRequest
    {
        /// <summary>
        /// 提示文本
        /// </summary>
        public required string Prompt { get; set; }

        /// <summary>
        /// 使用的模型
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// 最大令牌数
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// 温度参数
        /// </summary>
        public double? Temperature { get; set; }
    }

    /// <summary>
    /// 图像视觉分析请求模型
    /// </summary>
    public class OpenAIVisionRequest
    {
        /// <summary>
        /// 图像URL
        /// </summary>
        public required string ImageUrl { get; set; }

        /// <summary>
        /// 描述提示
        /// </summary>
        public string Prompt { get; set; } = "请描述这张图片";

        /// <summary>
        /// 使用的模型
        /// </summary>
        public string? Model { get; set; }
    }

    /// <summary>
    /// 流式聊天请求模型
    /// </summary>
    public class OpenAIStreamChatRequest
    {
        /// <summary>
        /// 用户消息
        /// </summary>
        public required string Message { get; set; }

        /// <summary>
        /// 使用的模型
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// 最大令牌数
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// 温度参数
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// 消息历史记录
        /// </summary>
        public List<ChatMessage>? History { get; set; }
    }

    /// <summary>
    /// 流式多轮聊天请求模型
    /// </summary>
    public class OpenAIStreamMultiChatRequest
    {
        /// <summary>
        /// 消息历史记录
        /// </summary>
        public required List<ChatMessage> Messages { get; set; }

        /// <summary>
        /// 使用的模型
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// 最大令牌数
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// 温度参数
        /// </summary>
        public double? Temperature { get; set; }
    }

    /// <summary>
    /// 流式响应数据块
    /// </summary>
    public class OpenAIStreamChunk
    {
        /// <summary>
        /// 数据块内容
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// 是否为最后一个数据块
        /// </summary>
        public bool IsLast { get; set; }

        /// <summary>
        /// 数据块索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 使用的模型
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// 请求ID
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? Error { get; set; }
    }

    /// <summary>
    /// 文本补全请求模型（保留兼容性）
    /// </summary>
    public class CompletionRequest
    {
        /// <summary>
        /// 提示文本
        /// </summary>
        public required string Prompt { get; set; }

        /// <summary>
        /// 使用的模型
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// 最大令牌数
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// 温度参数
        /// </summary>
        public double? Temperature { get; set; }
    }

    /// <summary>
    /// OpenAI服务状态模型
    /// </summary>
    public class OpenAIServiceStatus
    {
        /// <summary>
        /// 服务是否可用
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 检查时间
        /// </summary>
        public DateTime CheckTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 可用模型列表
        /// </summary>
        public List<string>? AvailableModels { get; set; }
    }
}