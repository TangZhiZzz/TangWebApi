using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TangWebApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace TangWebApi.Services
{
    /// <summary>
    /// RabbitMQ消息队列服务实现
    /// </summary>
    public class RabbitMQService : IMessageQueueService, IDisposable
    {
        private readonly MessageQueueSettings _settings;
        private readonly ILogger<RabbitMQService> _logger;
        private IConnection? _connection;
        private IModel? _channel;
        private readonly object _lock = new object();
        private bool _disposed = false;

        public RabbitMQService(IOptions<MessageQueueSettings> settings, ILogger<RabbitMQService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            // 不在构造函数中立即初始化连接，而是在需要时延迟初始化
        }

        /// <summary>
        /// 初始化连接
        /// </summary>
        private void InitializeConnection()
        {
            if (_connection != null && _connection.IsOpen)
                return;

            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = ExtractHostFromConnectionString(_settings.ConnectionString),
                    Port = ExtractPortFromConnectionString(_settings.ConnectionString),
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    VirtualHost = _settings.VirtualHost,
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(_settings.ConnectionTimeout),
                    RequestedHeartbeat = TimeSpan.FromSeconds(_settings.HeartbeatInterval)
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _logger.LogInformation("RabbitMQ连接已建立");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化RabbitMQ连接失败: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 从连接字符串提取主机名
        /// </summary>
        private string ExtractHostFromConnectionString(string connectionString)
        {
            if (connectionString.StartsWith("amqp://"))
            {
                var uri = new Uri(connectionString);
                return uri.Host;
            }
            return "localhost";
        }

        /// <summary>
        /// 从连接字符串提取端口
        /// </summary>
        private int ExtractPortFromConnectionString(string connectionString)
        {
            if (connectionString.StartsWith("amqp://"))
            {
                var uri = new Uri(connectionString);
                return uri.Port == -1 ? 5672 : uri.Port;
            }
            return 5672;
        }

        /// <summary>
        /// 确保连接可用
        /// </summary>
        private void EnsureConnection()
        {
            lock (_lock)
            {
                if (_connection == null || !_connection.IsOpen || _channel == null || !_channel.IsOpen)
                {
                    _logger.LogWarning("RabbitMQ连接已断开，正在重新连接...");
                    InitializeConnection();
                }
            }
        }

        /// <summary>
        /// 发送消息到指定队列
        /// </summary>
        public async Task SendMessageAsync(string queueName, object message)
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            await SendMessageAsync(queueName, jsonMessage);
        }

        /// <summary>
        /// 发送消息到指定队列（字符串格式）
        /// </summary>
        public async Task SendMessageAsync(string queueName, string message)
        {
            await Task.Run(() =>
            {
                EnsureConnection();

                try
                {
                    // 声明队列
                    _channel!.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                    var body = Encoding.UTF8.GetBytes(message);
                    var properties = _channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.MessageId = Guid.NewGuid().ToString();
                    properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                    _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);

                    _logger.LogInformation("消息已发送到队列 {QueueName}: {Message}", queueName, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "发送消息到队列 {QueueName} 失败", queueName);
                    throw;
                }
            });
        }

        /// <summary>
        /// 从指定队列接收消息
        /// </summary>
        public async Task ReceiveMessageAsync(string queueName, Func<string, Task> handler)
        {
            await Task.Run(() =>
            {
                EnsureConnection();

                try
                {
                    // 声明队列
                    _channel!.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += async (model, ea) =>
                    {
                        try
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);

                            _logger.LogInformation("从队列 {QueueName} 接收到消息: {Message}", queueName, message);

                            await handler(message);

                            // 确认消息处理完成
                            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "处理队列 {QueueName} 消息失败", queueName);
                            // 拒绝消息并重新入队
                            _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                        }
                    };

                    _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                    _logger.LogInformation("开始监听队列 {QueueName}", queueName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "监听队列 {QueueName} 失败", queueName);
                    throw;
                }
            });
        }

        /// <summary>
        /// 从指定队列接收消息（泛型版本）
        /// </summary>
        public async Task ReceiveMessageAsync<T>(string queueName, Func<T, Task> handler) where T : class
        {
            await ReceiveMessageAsync(queueName, async (message) =>
            {
                try
                {
                    var obj = JsonSerializer.Deserialize<T>(message);
                    if (obj != null)
                    {
                        await handler(obj);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "反序列化消息失败: {Message}", message);
                    throw;
                }
            });
        }

        /// <summary>
        /// 创建队列
        /// </summary>
        public async Task CreateQueueAsync(string queueName, bool durable = true)
        {
            await Task.Run(() =>
            {
                EnsureConnection();

                try
                {
                    _channel!.QueueDeclare(queue: queueName, durable: durable, exclusive: false, autoDelete: false, arguments: null);
                    _logger.LogInformation("队列 {QueueName} 创建成功", queueName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "创建队列 {QueueName} 失败", queueName);
                    throw;
                }
            });
        }

        /// <summary>
        /// 删除队列
        /// </summary>
        public async Task DeleteQueueAsync(string queueName)
        {
            await Task.Run(() =>
            {
                EnsureConnection();

                try
                {
                    _channel!.QueueDelete(queue: queueName);
                    _logger.LogInformation("队列 {QueueName} 删除成功", queueName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "删除队列 {QueueName} 失败", queueName);
                    throw;
                }
            });
        }

        /// <summary>
        /// 获取队列消息数量
        /// </summary>
        public async Task<uint> GetMessageCountAsync(string queueName)
        {
            return await Task.Run(() =>
            {
                EnsureConnection();

                try
                {
                    var result = _channel!.QueueDeclarePassive(queueName);
                    return result.MessageCount;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "获取队列 {QueueName} 消息数量失败", queueName);
                    throw;
                }
            });
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public async Task PurgeQueueAsync(string queueName)
        {
            await Task.Run(() =>
            {
                EnsureConnection();

                try
                {
                    _channel!.QueuePurge(queueName);
                    _logger.LogInformation("队列 {QueueName} 已清空", queueName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "清空队列 {QueueName} 失败", queueName);
                    throw;
                }
            });
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    InitializeConnection();
                    
                    if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
                    {
                        _logger.LogInformation("RabbitMQ连接测试成功");
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("RabbitMQ连接测试失败：连接未建立");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RabbitMQ连接测试失败: {Message}", ex.Message);
                    return false;
                }
            });
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _channel?.Close();
                    _channel?.Dispose();
                    _connection?.Close();
                    _connection?.Dispose();
                    _logger.LogInformation("RabbitMQ连接已关闭");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "关闭RabbitMQ连接时发生错误");
                }
                finally
                {
                    _disposed = true;
                }
            }
        }
    }
}