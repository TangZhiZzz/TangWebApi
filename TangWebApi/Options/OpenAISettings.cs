namespace TangWebApi.Options
{
    /// <summary>
    /// OpenAI配置模型
    /// </summary>
    public class OpenAISettings
    {
        /// <summary>
        /// OpenAI API密钥
        /// </summary>
        public required string ApiKey { get; set; }

        /// <summary>
        /// OpenAI API基础URL
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";

        /// <summary>
        /// 默认模型名称
        /// </summary>
        public string DefaultModel { get; set; } = "gpt-3.5-turbo";

        /// <summary>
        /// 最大令牌数
        /// </summary>
        public int MaxTokens { get; set; } = 1000;

        /// <summary>
        /// 温度参数（0-2之间，控制输出的随机性）
        /// </summary>
        public double Temperature { get; set; } = 0.7;

        /// <summary>
        /// 请求超时时间（秒）
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 是否启用OpenAI服务
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}