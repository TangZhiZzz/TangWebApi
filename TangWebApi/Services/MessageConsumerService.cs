using TangWebApi.Services;
using TangWebApi.Models;
using Microsoft.Extensions.Options;

namespace TangWebApi.Services
{
    /// <summary>
    /// 消息队列消费者后台服务
    /// </summary>
    public class MessageConsumerService : BackgroundService
    {
        private readonly IMessageQueueService _messageQueueService;
        private readonly ILogger<MessageConsumerService> _logger;
        private readonly TangWebApi.Options.MessageQueueConfig _messageQueueConfig;
        private readonly List<Task> _consumerTasks = new List<Task>();

        public MessageConsumerService(
            IMessageQueueService messageQueueService,
            ILogger<MessageConsumerService> logger,
            IOptions<TangWebApi.Options.MessageQueueConfig> messageQueueConfig)
        {
            _messageQueueService = messageQueueService;
            _logger = logger;
            _messageQueueConfig = messageQueueConfig.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var enabledQueues = _messageQueueConfig.Queues.Where(q => q.Enabled).ToList();
            _logger.LogInformation("消息队列消费者服务已启动，开始监听 {Count} 个队列: {Queues}", 
                enabledQueues.Count, string.Join(", ", enabledQueues.Select(q => q.Name)));

            try
            {
                // 为每个启用的队列创建消费者任务
                foreach (var queueConfig in enabledQueues)
                {
                    var consumerTask = StartQueueConsumerAsync(queueConfig, stoppingToken);
                    _consumerTasks.Add(consumerTask);
                }

                // 等待所有消费者任务完成或取消
                await Task.WhenAll(_consumerTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "消息队列消费者服务发生错误");
            }
        }

        /// <summary>
        /// 启动队列消费者
        /// </summary>
        /// <param name="queueConfig">队列配置</param>
        /// <param name="stoppingToken">取消令牌</param>
        /// <returns></returns>
        private async Task StartQueueConsumerAsync(TangWebApi.Options.QueueConfig queueConfig, CancellationToken stoppingToken)
        {
            _logger.LogDebug("开始监听队列: {QueueName} - {Description}", queueConfig.Name, queueConfig.Description);

            try
            {
                await _messageQueueService.ReceiveMessageAsync(queueConfig.Name, async (message) =>
                {
                    try
                    {
                        // 根据队列名称处理不同类型的消息
                        await ProcessMessageAsync(queueConfig.Name, message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "队列 {QueueName} 处理消息失败: {Message}", queueConfig.Name, message);
                        throw; // 重新抛出异常，让消息重新入队
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "队列 {QueueName} 消费者发生错误", queueConfig.Name);
            }
        }

        /// <summary>
        /// 处理接收到的消息
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        private async Task ProcessMessageAsync(string queueName, string message)
        {
            _logger.LogDebug("队列 {QueueName} 开始处理消息: {Message}", queueName, message);
            
            // 根据队列名称进行不同的处理逻辑
            switch (queueName)
            {
                case "test-queue":
                    await ProcessTestMessage(message);
                    break;
                case "email-queue":
                    await ProcessEmailMessage(message);
                    break;
                case "notification-queue":
                    await ProcessNotificationMessage(message);
                    break;
                case "log-queue":
                    await ProcessLogMessage(message);
                    break;
                default:
                    _logger.LogWarning("未知队列类型: {QueueName}，使用默认处理逻辑", queueName);
                    await ProcessDefaultMessage(message);
                    break;
            }
        }

        /// <summary>
        /// 处理测试消息
        /// </summary>
        private async Task ProcessTestMessage(string message)
        {
            _logger.LogInformation("处理测试消息: {Message}", message);
            await Task.Delay(100); // 模拟处理时间
        }

        /// <summary>
        /// 处理邮件消息
        /// </summary>
        private async Task ProcessEmailMessage(string message)
        {
            _logger.LogInformation("处理邮件消息: {Message}", message);
            // 这里可以调用邮件服务发送邮件
            await Task.Delay(200); // 模拟处理时间
        }

        /// <summary>
        /// 处理通知消息
        /// </summary>
        private async Task ProcessNotificationMessage(string message)
        {
            _logger.LogInformation("处理通知消息: {Message}", message);
            // 这里可以发送推送通知
            await Task.Delay(150); // 模拟处理时间
        }

        /// <summary>
        /// 处理日志消息
        /// </summary>
        private async Task ProcessLogMessage(string message)
        {
            _logger.LogInformation("处理日志消息: {Message}", message);
            // 这里可以进行日志分析和存储
            await Task.Delay(50); // 模拟处理时间
        }

        /// <summary>
        /// 默认消息处理
        /// </summary>
        private async Task ProcessDefaultMessage(string message)
        {
            _logger.LogInformation("处理默认消息: {Message}", message);
            await Task.Delay(100); // 模拟处理时间
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("消息队列消费者服务停止");
            await base.StopAsync(stoppingToken);
        }
    }
}