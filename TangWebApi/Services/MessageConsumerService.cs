using TangWebApi.Services;

namespace TangWebApi.Services
{
    /// <summary>
    /// 消息队列消费者后台服务
    /// </summary>
    public class MessageConsumerService : BackgroundService
    {
        private readonly IMessageQueueService _messageQueueService;
        private readonly ILogger<MessageConsumerService> _logger;
        private readonly IConfiguration _configuration;

        public MessageConsumerService(
            IMessageQueueService messageQueueService,
            ILogger<MessageConsumerService> logger,
            IConfiguration configuration)
        {
            _messageQueueService = messageQueueService;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("消息队列消费者服务已启动，开始监听队列");

            try
            {
                // 监听默认队列
                await _messageQueueService.ReceiveMessageAsync("test-queue", async (message) =>
                {
                    try
                    {
                        // 在这里处理消息逻辑
                        await ProcessMessageAsync(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "处理消息失败: {Message}", message);
                        throw; // 重新抛出异常，让消息重新入队
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "消息队列消费者服务发生错误");
            }
        }

        /// <summary>
        /// 处理接收到的消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        private async Task ProcessMessageAsync(string message)
        {
            // 这里可以根据消息内容进行不同的处理
            // 例如：解析JSON、调用其他服务、存储到数据库等
            
            _logger.LogInformation("处理消息: {Message}", message);
            
            // 模拟处理时间
            await Task.Delay(100);
            
            // 在这里添加你的业务逻辑
            // 例如：
            // - 解析消息内容
            // - 调用业务服务
            // - 存储到数据库
            // - 发送通知等
            
            // 只有在需要时才记录详细信息
            // _logger.LogInformation("消息处理完成: {Message}", message);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("消息队列消费者服务停止");
            await base.StopAsync(stoppingToken);
        }
    }
}