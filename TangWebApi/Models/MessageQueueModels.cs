using System;
using System.ComponentModel.DataAnnotations;

namespace TangWebApi.Models
{
    /// <summary>
    /// 消息队列配置
    /// </summary>
    public class MessageQueueSettings
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; } = "amqp://localhost:5672";

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = "guest";

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// 虚拟主机
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// 连接超时时间（秒）
        /// </summary>
        public int ConnectionTimeout { get; set; } = 30;

        /// <summary>
        /// 心跳间隔（秒）
        /// </summary>
        public ushort HeartbeatInterval { get; set; } = 60;
    }

    /// <summary>
    /// 队列消息基类
    /// </summary>
    public class QueueMessage
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 消息类型
        /// </summary>
        public string MessageType { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 消息内容
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// 消息优先级（0-255）
        /// </summary>
        public byte Priority { get; set; } = 0;

        /// <summary>
        /// 消息过期时间（毫秒）
        /// </summary>
        public int? ExpirationMs { get; set; }
    }

    /// <summary>
    /// 队列消息请求
    /// </summary>
    public class QueueMessageRequest
    {
        /// <summary>
        /// 队列名称
        /// </summary>
        [Required(ErrorMessage = "队列名称不能为空")]
        public string QueueName { get; set; } = string.Empty;

        /// <summary>
        /// 消息内容
        /// </summary>
        [Required(ErrorMessage = "消息内容不能为空")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 消息类型
        /// </summary>
        public string MessageType { get; set; } = "text";

        /// <summary>
        /// 消息优先级
        /// </summary>
        [Range(0, 255, ErrorMessage = "优先级必须在0-255之间")]
        public byte Priority { get; set; } = 0;

        /// <summary>
        /// 消息过期时间（毫秒）
        /// </summary>
        public int? ExpirationMs { get; set; }
    }

    /// <summary>
    /// 队列信息
    /// </summary>
    public class QueueInfo
    {
        /// <summary>
        /// 队列名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 消息数量
        /// </summary>
        public uint MessageCount { get; set; }

        /// <summary>
        /// 消费者数量
        /// </summary>
        public uint ConsumerCount { get; set; }

        /// <summary>
        /// 是否持久化
        /// </summary>
        public bool Durable { get; set; }

        /// <summary>
        /// 是否自动删除
        /// </summary>
        public bool AutoDelete { get; set; }
    }

    /// <summary>
    /// 创建队列请求
    /// </summary>
    public class CreateQueueRequest
    {
        /// <summary>
        /// 队列名称
        /// </summary>
        [Required(ErrorMessage = "队列名称不能为空")]
        public string QueueName { get; set; } = string.Empty;

        /// <summary>
        /// 是否持久化
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// 是否排他性
        /// </summary>
        public bool Exclusive { get; set; } = false;

        /// <summary>
        /// 是否自动删除
        /// </summary>
        public bool AutoDelete { get; set; } = false;
    }
}