using System.ComponentModel.DataAnnotations;

namespace TangWebApi.Models;

/// <summary>
/// SignalR 消息模型
/// </summary>
public class SignalRMessage
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 发送者用户名
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 消息内容
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType Type { get; set; } = MessageType.Text;

    /// <summary>
    /// 目标用户（私聊时使用）
    /// </summary>
    public string? TargetUser { get; set; }

    /// <summary>
    /// 群组名称（群聊时使用）
    /// </summary>
    public string? GroupName { get; set; }
}

/// <summary>
/// 消息类型枚举
/// </summary>
public enum MessageType
{
    /// <summary>
    /// 文本消息
    /// </summary>
    Text = 0,

    /// <summary>
    /// 系统通知
    /// </summary>
    System = 1,

    /// <summary>
    /// 用户加入
    /// </summary>
    UserJoined = 2,

    /// <summary>
    /// 用户离开
    /// </summary>
    UserLeft = 3,

    /// <summary>
    /// 私聊消息
    /// </summary>
    Private = 4
}

/// <summary>
/// 连接用户信息
/// </summary>
public class ConnectedUser
{
    /// <summary>
    /// 连接ID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 用户所在的群组列表
    /// </summary>
    public List<string> Groups { get; set; } = new();
}

/// <summary>
/// 群组信息
/// </summary>
public class GroupInfo
{
    /// <summary>
    /// 群组名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 群组描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 群组成员数量
    /// </summary>
    public int MemberCount { get; set; }

    /// <summary>
    /// 群组成员列表
    /// </summary>
    public List<string> Members { get; set; } = new();
}

/// <summary>
/// SignalR 通知模型
/// </summary>
public class SignalRNotification
{
    /// <summary>
    /// 通知ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 通知标题
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 通知内容
    /// </summary>
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.Info;

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 目标用户（为空则广播给所有用户）
    /// </summary>
    public string? TargetUser { get; set; }

    /// <summary>
    /// 目标群组（为空则不发送给群组）
    /// </summary>
    public string? TargetGroup { get; set; }
}

/// <summary>
/// 通知类型枚举
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// 信息通知
    /// </summary>
    Info = 0,

    /// <summary>
    /// 成功通知
    /// </summary>
    Success = 1,

    /// <summary>
    /// 警告通知
    /// </summary>
    Warning = 2,

    /// <summary>
    /// 错误通知
    /// </summary>
    Error = 3
}

/// <summary>
/// 在线用户统计
/// </summary>
public class OnlineUserStats
{
    /// <summary>
    /// 在线用户总数
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// 在线用户列表
    /// </summary>
    public List<ConnectedUser> Users { get; set; } = new();

    /// <summary>
    /// 活跃群组数量
    /// </summary>
    public int ActiveGroups { get; set; }

    /// <summary>
    /// 统计时间
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}