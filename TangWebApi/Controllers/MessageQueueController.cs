using Microsoft.AspNetCore.Mvc;
using TangWebApi.Services;
using TangWebApi.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

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
        private readonly TangWebApi.Options.MessageQueueConfig _messageQueueConfig;

        public MessageQueueController(
            IMessageQueueService messageQueueService, 
            ILogger<MessageQueueController> logger,
            IOptions<TangWebApi.Options.MessageQueueConfig> messageQueueConfig)
        {
            _messageQueueService = messageQueueService;
            _logger = logger;
            _messageQueueConfig = messageQueueConfig.Value;
        }

        /// <summary>
        /// 发送消息到指定队列
        /// </summary>
        /// <param name="request">发送消息请求</param>
        /// <returns></returns>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
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

                // 移除冗余的发送成功日志

                return Ok(new
                {
                    Success = true,
                    Message = "消息发送成功",
                    QueueName = request.QueueName,
                    MessageId = message.MessageId,
                    Timestamp = message.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送消息到队列 {QueueName} 失败", request.QueueName);
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// 发送简单文本消息
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        [HttpPost("send-text")]
        public async Task<IActionResult> SendTextMessage([Required] string queueName, [Required] string message)
        {
            try
            {
                await _messageQueueService.SendMessageAsync(queueName, message);

                _logger.LogInformation("文本消息已发送到队列 {QueueName}: {Message}", queueName, message);

                return Ok(new
                {
                    Success = true,
                    Message = "文本消息发送成功",
                    QueueName = queueName,
                    Content = message,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送文本消息到队列 {QueueName} 失败", queueName);
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// 创建队列
        /// </summary>
        /// <param name="request">创建队列请求</param>
        /// <returns></returns>
        [HttpPost("create-queue")]
        public async Task<IActionResult> CreateQueue([FromBody] CreateQueueRequest request)
        {
            try
            {
                await _messageQueueService.CreateQueueAsync(request.QueueName, request.Durable);

                _logger.LogInformation("队列 {QueueName} 创建成功", request.QueueName);

                return Ok(new
                {
                    Success = true,
                    Message = "队列创建成功",
                    QueueName = request.QueueName,
                    Durable = request.Durable,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建队列 {QueueName} 失败", request.QueueName);
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// 删除队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        [HttpDelete("delete-queue/{queueName}")]
        public async Task<IActionResult> DeleteQueue(string queueName)
        {
            try
            {
                await _messageQueueService.DeleteQueueAsync(queueName);

                // 移除冗余的删除成功日志

                return Ok(new
                {
                    Success = true,
                    Message = "队列删除成功",
                    QueueName = queueName,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除队列 {QueueName} 失败", queueName);
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// 获取队列信息
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        [HttpGet("queue-info/{queueName}")]
        public async Task<IActionResult> GetQueueInfo(string queueName)
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

                return Ok(new
                {
                    Success = true,
                    Data = queueInfo,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取队列 {QueueName} 信息失败", queueName);
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        [HttpPost("purge/{queueName}")]
        public async Task<IActionResult> PurgeQueue(string queueName)
        {
            try
            {
                await _messageQueueService.PurgeQueueAsync(queueName);
                return Ok(new
                {
                    Success = true,
                    Message = "队列已清空",
                    QueueName = queueName,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// 测试消息队列连接
        /// </summary>
        /// <returns></returns>
        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            var testResult = new TestResult();
            var errors = new List<string>();

            try
            {
                // 测试连接
                var isConnected = await _messageQueueService.TestConnectionAsync();
                testResult.ConnectionTest = isConnected;
                if (!isConnected)
                {
                    errors.Add("连接测试失败");
                }

                // 创建测试队列
                var testQueueName = $"test-queue-{DateTime.Now:yyyyMMddHHmmss}";
                await _messageQueueService.CreateQueueAsync(testQueueName);
                testResult.QueueCreated = true;

                // 发送测试消息
                var testMessage = $"Test message at {DateTime.Now}";
                await _messageQueueService.SendMessageAsync(testQueueName, testMessage);
                testResult.MessageSent = true;

                // 检查消息数量
                var messageCount = await _messageQueueService.GetMessageCountAsync(testQueueName);
                testResult.MessageCount = messageCount;

                // 清理测试队列
                await _messageQueueService.DeleteQueueAsync(testQueueName);
                testResult.QueueDeleted = true;

                return Ok(new
                {
                    Success = true,
                    Message = "消息队列连接测试成功",
                    TestResult = testResult,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试消息队列连接失败");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "消息队列连接测试失败",
                    Error = ex.Message,
                    TestResult = testResult,
                    Timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// 测试消息发送和接收功能
        /// </summary>
        /// <param name="message">测试消息内容</param>
        /// <returns></returns>
        [HttpPost("test-send-receive")]
        public async Task<IActionResult> TestSendReceive([FromQuery] string message = "Hello from API test!")
        {
            try
            {
                var queueName = "test-queue";
                
                // 确保队列存在
                await _messageQueueService.CreateQueueAsync(queueName);
                
                // 发送测试消息
                await _messageQueueService.SendMessageAsync(queueName, message);
                
                // 移除冗余的测试消息发送日志
                
                return Ok(new
                {
                    Success = true,
                    Message = "测试消息已发送，请查看控制台日志以确认消费者是否接收到消息",
                    QueueName = queueName,
                    SentMessage = message,
                    Timestamp = DateTime.Now,
                    Note = "如果消费者服务正在运行，您应该在控制台看到 '收到消息: ' 的日志输出"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试消息发送失败");
                return BadRequest(new
                {
                    Success = false,
                    Message = "测试消息发送失败",
                    Error = ex.Message,
                    Timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// 测试多队列功能 - 向所有启用的队列发送测试消息
        /// </summary>
        /// <param name="message">测试消息</param>
        /// <returns></returns>
        [HttpPost("test-multi-queues")]
        public async Task<IActionResult> TestMultiQueues(string message = "Multi-Queue Test Message")
        {
            try
            {
                var enabledQueues = _messageQueueConfig.Queues.Where(q => q.Enabled).ToList();
                var results = new List<QueueTestResult>();

                _logger.LogInformation("开始测试多队列功能，向 {Count} 个队列发送消息", enabledQueues.Count);

                foreach (var queueConfig in enabledQueues)
                {
                    try
                    {
                        var testMessage = $"[{queueConfig.Name}] {message} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                        await _messageQueueService.SendMessageAsync(queueConfig.Name, testMessage);

                        results.Add(new QueueTestResult
                        {
                            QueueName = queueConfig.Name,
                            Description = queueConfig.Description,
                            Success = true,
                            Message = "消息发送成功",
                            SentMessage = testMessage
                        });

                        _logger.LogInformation("向队列 {QueueName} 发送测试消息成功", queueConfig.Name);
                    }
                    catch (Exception ex)
                    {
                        results.Add(new QueueTestResult
                        {
                            QueueName = queueConfig.Name,
                            Description = queueConfig.Description,
                            Success = false,
                            Message = $"发送失败: {ex.Message}",
                            SentMessage = null
                        });

                        _logger.LogError(ex, "向队列 {QueueName} 发送测试消息失败", queueConfig.Name);
                    }
                }

                var successCount = results.Count(r => r.Success);
                var totalCount = results.Count;

                return Ok(new
                {
                    Success = successCount > 0,
                    Message = $"多队列测试完成，成功 {successCount}/{totalCount} 个队列",
                    TotalQueues = totalCount,
                    SuccessfulQueues = successCount,
                    FailedQueues = totalCount - successCount,
                    Results = results,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "多队列测试失败");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// 获取队列配置信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("queue-config")]
        public IActionResult GetQueueConfig()
        {
            try
            {
                return Ok(new
                {
                    Success = true,
                    Message = "获取队列配置成功",
                    TotalQueues = _messageQueueConfig.Queues.Count,
                    EnabledQueues = _messageQueueConfig.Queues.Count(q => q.Enabled),
                    DisabledQueues = _messageQueueConfig.Queues.Count(q => !q.Enabled),
                    Queues = _messageQueueConfig.Queues.Select(q => new QueueConfigInfo
                    {
                        Name = q.Name,
                        Description = q.Description,
                        Enabled = q.Enabled
                    }).ToList(),
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取队列配置失败");
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }

    // 保留必要的数据模型
    public class TestResult
    {
        public bool ConnectionTest { get; set; }
        public bool QueueCreated { get; set; }
        public bool MessageSent { get; set; }
        public uint MessageCount { get; set; }
        public bool QueueDeleted { get; set; }
    }

    public class QueueTestResult
    {
        public required string QueueName { get; set; }
        public string? Description { get; set; }
        public bool Success { get; set; }
        public required string Message { get; set; }
        public string? SentMessage { get; set; }
    }

    public class QueueConfigInfo
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool Enabled { get; set; }
    }
}