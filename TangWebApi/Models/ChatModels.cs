using System.ComponentModel.DataAnnotations;

namespace TangWebApi.Models
{
    /// <summary>
    /// 聊天消息模型
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 发送用户
        /// </summary>
        [Required]
        public string User { get; set; } = string.Empty;

        /// <summary>
        /// 消息内容
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 消息类型
        /// </summary>
        public ChatMessageType MessageType { get; set; } = ChatMessageType.Text;

        /// <summary>
        /// 目标群组（可选）
        /// </summary>
        public string? GroupName { get; set; }

        /// <summary>
        /// 目标用户（私聊时使用）
        /// </summary>
        public string? ToUser { get; set; }
    }

    /// <summary>
    /// 聊天用户模型
    /// </summary>
    public class ChatUser
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 连接ID
        /// </summary>
        public string ConnectionId { get; set; } = string.Empty;

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsOnline { get; set; } = true;

        /// <summary>
        /// 最后活跃时间
        /// </summary>
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 加入的群组列表
        /// </summary>
        public List<string> Groups { get; set; } = new();
    }

    /// <summary>
    /// 聊天群组模型
    /// </summary>
    public class ChatGroup
    {
        /// <summary>
        /// 群组ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 群组名称
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 群组描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 群组成员
        /// </summary>
        public List<string> Members { get; set; } = new();

        /// <summary>
        /// 群组管理员
        /// </summary>
        public List<string> Admins { get; set; } = new();

        /// <summary>
        /// 最大成员数
        /// </summary>
        public int MaxMembers { get; set; } = 100;
    }

    /// <summary>
    /// 消息类型枚举
    /// </summary>
    public enum ChatMessageType
    {
        /// <summary>
        /// 文本消息
        /// </summary>
        Text = 0,

        /// <summary>
        /// 图片消息
        /// </summary>
        Image = 1,

        /// <summary>
        /// 文件消息
        /// </summary>
        File = 2,

        /// <summary>
        /// 系统消息
        /// </summary>
        System = 3,

        /// <summary>
        /// 通知消息
        /// </summary>
        Notification = 4
    }

    /// <summary>
    /// 发送消息请求模型
    /// </summary>
    public class SendMessageRequest
    {
        /// <summary>
        /// 发送用户
        /// </summary>
        [Required]
        public string User { get; set; } = string.Empty;

        /// <summary>
        /// 消息内容
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 消息类型
        /// </summary>
        public ChatMessageType MessageType { get; set; } = ChatMessageType.Text;
    }

    /// <summary>
    /// 发送私聊消息请求模型
    /// </summary>
    public class SendPrivateMessageRequest : SendMessageRequest
    {
        /// <summary>
        /// 目标用户
        /// </summary>
        [Required]
        public string ToUser { get; set; } = string.Empty;
    }

    /// <summary>
    /// 发送群组消息请求模型
    /// </summary>
    public class SendGroupMessageRequest : SendMessageRequest
    {
        /// <summary>
        /// 目标群组
        /// </summary>
        [Required]
        public string GroupName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 加入群组请求模型
    /// </summary>
    public class JoinGroupRequest
    {
        /// <summary>
        /// 群组名称
        /// </summary>
        [Required]
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }
    }

    /// <summary>
    /// 用户状态更新模型
    /// </summary>
    public class UserStatusUpdate
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 是否正在输入
        /// </summary>
        public bool IsTyping { get; set; }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string? StatusMessage { get; set; }
    }

    /// <summary>
    /// 系统通知模型
    /// </summary>
    public class SystemNotification
    {
        /// <summary>
        /// 通知ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 通知消息
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 通知类型
        /// </summary>
        public NotificationType Type { get; set; } = NotificationType.Info;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 目标用户（为空则广播给所有用户）
        /// </summary>
        public string? TargetUser { get; set; }

        /// <summary>
        /// 目标群组（为空则不限制群组）
        /// </summary>
        public string? TargetGroup { get; set; }
    }

    /// <summary>
    /// 通知类型枚举
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// 信息
        /// </summary>
        Info = 0,

        /// <summary>
        /// 警告
        /// </summary>
        Warning = 1,

        /// <summary>
        /// 错误
        /// </summary>
        Error = 2,

        /// <summary>
        /// 成功
        /// </summary>
        Success = 3
    }
}