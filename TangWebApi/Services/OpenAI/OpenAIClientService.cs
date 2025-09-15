using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using TangWebApi.Options;

namespace TangWebApi.Services
{
    /// <summary>
    /// OpenAI客户端服务实现类（单例模式）
    /// </summary>
    public class OpenAIClientService : IOpenAIClientService
    {
        private readonly OpenAIClient _openAIClient;
        private readonly ILogger<OpenAIClientService> _logger;
        private readonly OpenAISettings _settings;

        public OpenAIClientService(IOptions<OpenAISettings> settings, ILogger<OpenAIClientService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            try
            {
                // 验证配置
                if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                {
                    throw new InvalidOperationException("OpenAI API Key is not configured");
                }

                if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
                {
                    throw new InvalidOperationException("OpenAI Base URL is not configured");
                }

                // 初始化OpenAI客户端
                _openAIClient = new OpenAIClient(
                    credential: new ApiKeyCredential(_settings.ApiKey),
                    options: new OpenAIClientOptions()
                    {
                        Endpoint = new Uri(_settings.BaseUrl)
                    }
                );

                _logger.LogInformation("OpenAI客户端初始化成功，BaseUrl: {BaseUrl}, Model: {Model}", 
                    _settings.BaseUrl, _settings.DefaultModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI客户端初始化失败");
                throw;
            }
        }

        /// <summary>
        /// 获取OpenAI客户端实例
        /// </summary>
        /// <returns>OpenAI客户端</returns>
        public OpenAIClient GetClient()
        {
            if (_openAIClient == null)
            {
                throw new InvalidOperationException("OpenAI客户端未正确初始化");
            }

            return _openAIClient;
        }

        /// <summary>
        /// 检查客户端是否已初始化
        /// </summary>
        /// <returns>是否已初始化</returns>
        public bool IsInitialized => _openAIClient != null;
    }
}