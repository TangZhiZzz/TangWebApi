using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
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
        private readonly OpenAIClient _openAIClient;

        public OpenAIService(IOptions<OpenAISettings> settings, ILogger<OpenAIService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            // 初始化ChatClient
            _openAIClient = new(
                credential: new ApiKeyCredential(_settings.ApiKey),
                options: new OpenAIClientOptions()
                {
                    Endpoint = new Uri(_settings.BaseUrl)
                }
            );

        }
    }
}