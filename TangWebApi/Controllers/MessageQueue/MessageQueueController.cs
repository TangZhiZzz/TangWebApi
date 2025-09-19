using Microsoft.AspNetCore.Mvc;
using TangWebApi.Services;
using TangWebApi.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using TangWebApi.Options;
using Azure.Core;

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
        public async Task SendMessage([FromBody] QueueMessageRequest request)
        {
            var message = new QueueMessage
            {
                MessageType = request.MessageType,
                Data = request.Message,
                Priority = request.Priority,
                ExpirationMs = request.ExpirationMs
            };

            await _messageQueueService.SendMessageAsync(request.QueueName, message);

            _logger.LogInformation("消息已发送到队列 {QueueName}: {Message}", request.QueueName, message);


        }

        /// <summary>
        /// 发送简单文本消息
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        [HttpPost("send-text")]
        public async Task SendTextMessage([Required] string queueName, [Required] string message)
        {
            await _messageQueueService.SendMessageAsync(queueName, message);

            _logger.LogInformation("文本消息已发送到队列 {QueueName}: {Message}", queueName, message);


        }

        /// <summary>
        /// 创建队列
        /// </summary>
        /// <param name="request">创建队列请求</param>
        /// <returns></returns>
        [HttpPost("create-queue")]
        public async Task CreateQueue([FromBody] CreateQueueRequest request)
        {
            await _messageQueueService.CreateQueueAsync(request.QueueName, request.Durable);

            _logger.LogInformation("队列 {QueueName} 创建成功", request.QueueName);



        }

        /// <summary>
        /// 删除队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        [HttpDelete("delete-queue/{queueName}")]
        public async Task DeleteQueue(string queueName)
        {
            await _messageQueueService.DeleteQueueAsync(queueName);
            _logger.LogInformation("删除队列 {queueName} 成功", queueName);


        }

        /// <summary>
        /// 获取队列信息
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        [HttpGet("queue-info/{queueName}")]
        public async Task<QueueInfo> GetQueueInfo(string queueName)
        {
            var messageCount = await _messageQueueService.GetMessageCountAsync(queueName);

            var queueInfo = new QueueInfo
            {
                Name = queueName,
                MessageCount = messageCount,
                Durable = true // RabbitMQ默认创建持久化队列
            };

            return queueInfo;

        }

        /// <summary>
        /// 清空队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        [HttpPost("purge/{queueName}")]
        public async Task PurgeQueue(string queueName)
        {
            await _messageQueueService.PurgeQueueAsync(queueName);
        }

        /// <summary>
        /// 测试消息队列连接
        /// </summary>
        /// <returns></returns>
        [HttpPost("test-connection")]
        public async Task<TestResult> TestConnection()
        {
            var testResult = new TestResult();
            var errors = new List<string>();

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

            return testResult;

        }

        /// <summary>
        /// 测试消息发送和接收功能
        /// </summary>
        /// <param name="message">测试消息内容</param>
        /// <returns></returns>
        [HttpPost("test-send-receive")]
        public async Task TestSendReceive([FromQuery] string message = "Hello from API test!")
        {
            var queueName = "test-queue";

            // 确保队列存在
            await _messageQueueService.CreateQueueAsync(queueName);

            // 发送测试消息
            await _messageQueueService.SendMessageAsync(queueName, message);

        }


        /// <summary>
        /// 获取队列配置信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("queue-config")]
        public List<QueueConfig> GetQueueConfig()
        {
            return _messageQueueConfig.Queues;
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



}