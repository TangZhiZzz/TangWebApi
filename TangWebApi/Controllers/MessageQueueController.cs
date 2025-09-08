using Microsoft.AspNetCore.Mvc;
using TangWebApi.Services;
using TangWebApi.Models;
using System.ComponentModel.DataAnnotations;

namespace TangWebApi.Controllers
{
    /// <summary>
    /// 消息队列控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MessageQueueController : ControllerBase
    {
        private readonly IMessageQueueService _messageQueueService;
        private readonly ILogger<MessageQueueController> _logger;

        public MessageQueueController(IMessageQueueService messageQueueService, ILogger<MessageQueueController> logger)
        {
            _messageQueueService = messageQueueService;
            _logger = logger;
        }

        /// <summary>
        /// 发送消息到指定队列
        /// </summary>
        /// <param name="request">发送消息请求</param>
        /// <returns></returns>
        [HttpPost("send")]
        public async Task<SendMessageResponse> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                var message = new QueueMessage
                {
                    MessageType = request.MessageType,
                    Data = request.Message,
                    Priority = request.Priority,
                    ExpirationMs = request.ExpirationMs
                };

                await _messageQueueService.SendMessageAsync(request.QueueName, message);

                _logger.LogInformation("消息已发送到队列 {QueueName}", request.QueueName);

                return new SendMessageResponse
                {
                    Success = true,
                    Message = "消息发送成功",
                    QueueName = request.QueueName,
                    MessageId = message.MessageId,
                    Timestamp = message.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送消息到队列 {QueueName} 失败", request.QueueName);
                throw new InvalidOperationException($"发送消息失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 发送简单文本消息
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        [HttpPost("send-text")]
        public async Task<SendTextMessageResponse> SendTextMessage([Required] string queueName, [Required] string message)
        {
            try
            {
                await _messageQueueService.SendMessageAsync(queueName, message);

                _logger.LogInformation("文本消息已发送到队列 {QueueName}: {Message}", queueName, message);

                return new SendTextMessageResponse
                {
                    Success = true,
                    Message = "文本消息发送成功",
                    QueueName = queueName,
                    Content = message,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送文本消息到队列 {QueueName} 失败", queueName);
                throw new InvalidOperationException($"发送文本消息失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 创建队列
        /// </summary>
        /// <param name="request">创建队列请求</param>
        /// <returns></returns>
        [HttpPost("create-queue")]
        public async Task<CreateQueueResponse> CreateQueue([FromBody] CreateQueueRequest request)
        {
            try
            {
                await _messageQueueService.CreateQueueAsync(request.QueueName, request.Durable);

                _logger.LogInformation("队列 {QueueName} 创建成功", request.QueueName);

                return new CreateQueueResponse
                {
                    Success = true,
                    Message = "队列创建成功",
                    QueueName = request.QueueName,
                    Durable = request.Durable,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建队列 {QueueName} 失败", request.QueueName);
                throw new InvalidOperationException($"创建队列失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        [HttpDelete("delete-queue/{queueName}")]
        public async Task<DeleteQueueResponse> DeleteQueue(string queueName)
        {
            try
            {
                await _messageQueueService.DeleteQueueAsync(queueName);

                _logger.LogInformation("队列 {QueueName} 删除成功", queueName);

                return new DeleteQueueResponse
                {
                    Success = true,
                    Message = "队列删除成功",
                    QueueName = queueName,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除队列 {QueueName} 失败", queueName);
                throw new InvalidOperationException($"删除队列失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取队列信息
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        [HttpGet("queue-info/{queueName}")]
        public async Task<QueueInfoResponse> GetQueueInfo(string queueName)
        {
            try
            {
                var messageCount = await _messageQueueService.GetMessageCountAsync(queueName);

                var queueInfo = new QueueInfo
                {
                    Name = queueName,
                    MessageCount = messageCount,
                    Durable = true // RabbitMQ默认创建持久化队列
                };

                return new QueueInfoResponse
                {
                    Success = true,
                    Data = queueInfo,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取队列 {QueueName} 信息失败", queueName);
                throw new InvalidOperationException($"获取队列信息失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        [HttpPost("purge/{queueName}")]
        public async Task<PurgeQueueResponse> PurgeQueue(string queueName)
        {
            try
            {
                await _messageQueueService.PurgeQueueAsync(queueName);
                return new PurgeQueueResponse
                {
                    Success = true,
                    Message = "队列已清空",
                    QueueName = queueName,
                    Timestamp = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"清空队列失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 测试消息队列连接
        /// </summary>
        /// <returns></returns>
        [HttpPost("test-connection")]
        public async Task<TestConnectionResponse> TestConnection()
        {
            try
            {
                var result = await _messageQueueService.TestConnectionAsync();
                return new TestConnectionResponse
                {
                    Success = true,
                    Message = "连接测试完成",
                    TestResult = new TestResult
                    {
                        ConnectionTest = result,
                        QueueCreated = false,
                        MessageSent = false,
                        MessageCount = 0,
                        QueueDeleted = false
                    },
                    Timestamp = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"连接测试失败: {ex.Message}", ex);
            }
        }
    }

    // 消息队列相关响应模型
    public class SendMessageResponse
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public required string QueueName { get; set; }
        public required string MessageId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class SendTextMessageResponse
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public required string QueueName { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class CreateQueueResponse
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public required string QueueName { get; set; }
        public bool Durable { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class DeleteQueueResponse
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public required string QueueName { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class QueueInfoResponse
    {
        public bool Success { get; set; }
        public required object Data { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PurgeQueueResponse
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public required string QueueName { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class TestConnectionResponse
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public string? Error { get; set; }
        public required TestResult TestResult { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class TestResult
    {
        public bool ConnectionTest { get; set; }
        public bool QueueCreated { get; set; }
        public bool MessageSent { get; set; }
        public int MessageCount { get; set; }
        public bool QueueDeleted { get; set; }
    }
}