using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TangWebApi.Hubs;
using TangWebApi.Models;

namespace TangWebApi.Controllers;

/// <summary>
/// SignalR 测试控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SignalRController : ControllerBase
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<SignalRController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="hubContext">SignalR Hub上下文</param>
    /// <param name="logger">日志记录器</param>
    public SignalRController(IHubContext<ChatHub> hubContext, ILogger<SignalRController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// 发送广播消息
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <returns></returns>
    [HttpPost("broadcast")]
    public async Task<ApiResponse<object>> BroadcastMessage([FromBody] SignalRMessage message)
    {
        try
        {
            message.Timestamp = DateTime.UtcNow;
            message.Type = MessageType.System;
            
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
            
            _logger.LogInformation("广播消息发送成功: {Content}", message.Content);
            
            return ApiResponse<object>.CreateSuccess(new { messageId = message.Id }, "广播消息发送成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送广播消息失败");
            return ApiResponse<object>.CreateError("发送广播消息失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 发送通知
    /// </summary>
    /// <param name="notification">通知内容</param>
    /// <returns></returns>
    [HttpPost("notification")]
    public async Task<ApiResponse<object>> SendNotification([FromBody] SignalRNotification notification)
    {
        try
        {
            notification.Timestamp = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(notification.TargetUser))
            {
                // 发送给特定用户（这里需要根据用户名找到连接ID）
                var connectedUsers = ChatHub.GetConnectedUsers();
                var targetUser = connectedUsers.FirstOrDefault(u => u.Username == notification.TargetUser);
                
                if (targetUser != null)
                {
                    await _hubContext.Clients.Client(targetUser.ConnectionId).SendAsync("ReceiveNotification", notification);
                    _logger.LogInformation("通知发送给用户 {Username}: {Title}", notification.TargetUser, notification.Title);
                }
                else
                {
                    return ApiResponse<object>.CreateError($"用户 {notification.TargetUser} 不在线");
                }
            }
            else if (!string.IsNullOrEmpty(notification.TargetGroup))
            {
                // 发送给特定群组
                await _hubContext.Clients.Group(notification.TargetGroup).SendAsync("ReceiveNotification", notification);
                _logger.LogInformation("通知发送给群组 {GroupName}: {Title}", notification.TargetGroup, notification.Title);
            }
            else
            {
                // 广播给所有用户
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
                _logger.LogInformation("通知广播给所有用户: {Title}", notification.Title);
            }
            
            return ApiResponse<object>.CreateSuccess(new { notificationId = notification.Id }, "通知发送成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送通知失败");
            return ApiResponse<object>.CreateError("发送通知失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 获取在线用户列表
    /// </summary>
    /// <returns></returns>
    [HttpGet("online-users")]
    public ApiResponse<OnlineUserStats> GetOnlineUsers()
    {
        try
        {
            var connectedUsers = ChatHub.GetConnectedUsers();
            var groups = ChatHub.GetAllGroups();
            
            var stats = new OnlineUserStats
            {
                TotalUsers = connectedUsers.Count,
                Users = connectedUsers,
                ActiveGroups = groups.Count,
                Timestamp = DateTime.UtcNow
            };
            
            return ApiResponse<OnlineUserStats>.CreateSuccess(stats, "获取在线用户成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取在线用户失败");
            return ApiResponse<OnlineUserStats>.CreateError("获取在线用户失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 获取群组列表
    /// </summary>
    /// <returns></returns>
    [HttpGet("groups")]
    public ApiResponse<List<GroupInfo>> GetGroups()
    {
        try
        {
            var groups = ChatHub.GetAllGroups();
            return ApiResponse<List<GroupInfo>>.CreateSuccess(groups, "获取群组列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取群组列表失败");
            return ApiResponse<List<GroupInfo>>.CreateError("获取群组列表失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 强制用户离线（管理功能）
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns></returns>
    [HttpPost("disconnect/{username}")]
    public async Task DisconnectUser(string username)
    {
        try
        {
            var connectedUsers = ChatHub.GetConnectedUsers();
            var targetUser = connectedUsers.FirstOrDefault(u => u.Username == username);
            
            if (targetUser != null)
            {
                // 发送断开连接通知
                await _hubContext.Clients.Client(targetUser.ConnectionId).SendAsync("ForceDisconnect", "管理员强制断开连接");
                
                _logger.LogInformation("用户 {Username} 被强制断开连接", username);
            }
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "强制断开用户连接失败");
        }
    }

    /// <summary>
    /// 获取SignalR连接统计信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("stats")]
    public object GetConnectionStats()
    {
        try
        {
            var connectedUsers = ChatHub.GetConnectedUsers();
            var groups = ChatHub.GetAllGroups();
            
            var stats = new
            {
                TotalConnections = connectedUsers.Count,
                TotalGroups = groups.Count,
                AverageConnectionTime = connectedUsers.Any() 
                    ? connectedUsers.Average(u => (DateTime.UtcNow - u.ConnectedAt).TotalMinutes)
                    : 0,
                GroupStats = groups.Select(g => new
                {
                    g.Name,
                    g.MemberCount,
                    g.CreatedAt
                }).ToList(),
                RecentConnections = connectedUsers
                    .OrderByDescending(u => u.ConnectedAt)
                    .Take(10)
                    .Select(u => new
                    {
                        u.Username,
                        u.ConnectedAt,
                        GroupCount = u.Groups.Count
                    })
                    .ToList(),
                Timestamp = DateTime.UtcNow
            };
            
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取连接统计失败");
            return ApiResponse<object>.CreateError("获取连接统计失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 测试SignalR连接
    /// </summary>
    /// <returns></returns>
    [HttpGet("test")]
    public async Task<ApiResponse<object>> TestSignalR()
    {
        try
        {
            var testMessage = new SignalRMessage
            {
                Username = "System",
                Content = $"SignalR 测试消息 - {DateTime.Now:HH:mm:ss}",
                Type = MessageType.System,
                Timestamp = DateTime.UtcNow
            };
            
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", testMessage);
            
            var testNotification = new SignalRNotification
            {
                Title = "系统测试",
                Message = "SignalR 功能测试通知",
                Type = NotificationType.Info,
                Timestamp = DateTime.UtcNow
            };
            
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", testNotification);
            
            _logger.LogInformation("SignalR 测试消息发送成功");
            
            return ApiResponse<object>.CreateSuccess(new
            {
                message = "测试消息和通知已发送给所有连接的客户端",
                timestamp = DateTime.UtcNow,
                hubEndpoint = "/chathub"
            }, "SignalR 测试成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR 测试失败");
            return ApiResponse<object>.CreateError("SignalR 测试失败: " + ex.Message);
        }
    }
}