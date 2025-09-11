using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TangWebApi.Models;

namespace TangWebApi.Hubs;

/// <summary>
/// SignalR 聊天中心
/// </summary>
public class ChatHub : Hub
{
    // 存储连接用户信息
    private static readonly ConcurrentDictionary<string, ConnectedUser> _connectedUsers = new();
    
    // 存储群组信息
    private static readonly ConcurrentDictionary<string, GroupInfo> _groups = new();

    /// <summary>
    /// 用户连接时触发
    /// </summary>
    /// <returns></returns>
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var username = Context.User?.Identity?.Name ?? $"User_{connectionId[..8]}";
        
        var user = new ConnectedUser
        {
            ConnectionId = connectionId,
            Username = username,
            ConnectedAt = DateTime.UtcNow
        };
        
        _connectedUsers.TryAdd(connectionId, user);
        
        // 通知所有客户端有新用户加入
        var joinMessage = new SignalRMessage
        {
            Username = "System",
            Content = $"{username} 加入了聊天室",
            Type = MessageType.UserJoined
        };
        
        await Clients.All.SendAsync("ReceiveMessage", joinMessage);
        await Clients.All.SendAsync("UserJoined", user);
        await UpdateOnlineUserCount();
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// 用户断开连接时触发
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        
        if (_connectedUsers.TryRemove(connectionId, out var user))
        {
            // 从所有群组中移除用户
            foreach (var groupName in user.Groups.ToList())
            {
                await Groups.RemoveFromGroupAsync(connectionId, groupName);
                if (_groups.TryGetValue(groupName, out var group))
                {
                    group.Members.Remove(user.Username);
                    group.MemberCount = group.Members.Count;
                    await Clients.Group(groupName).SendAsync("UserLeftGroup", user.Username, groupName);
                }
            }
            
            // 通知所有客户端用户离开
            var leaveMessage = new SignalRMessage
            {
                Username = "System",
                Content = $"{user.Username} 离开了聊天室",
                Type = MessageType.UserLeft
            };
            
            await Clients.All.SendAsync("ReceiveMessage", leaveMessage);
            await Clients.All.SendAsync("UserLeft", user.Username);
            await UpdateOnlineUserCount();
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// 发送消息到所有客户端
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendMessage(SignalRMessage message)
    {
        if (_connectedUsers.TryGetValue(Context.ConnectionId, out var user))
        {
            message.Username = user.Username;
            message.Timestamp = DateTime.UtcNow;
            
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }

    /// <summary>
    /// 发送私聊消息
    /// </summary>
    /// <param name="targetUsername"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task SendPrivateMessage(string targetUsername, string content)
    {
        if (_connectedUsers.TryGetValue(Context.ConnectionId, out var sender))
        {
            var targetUser = _connectedUsers.Values.FirstOrDefault(u => u.Username == targetUsername);
            if (targetUser != null)
            {
                var message = new SignalRMessage
                {
                    Username = sender.Username,
                    Content = content,
                    Type = MessageType.Private,
                    TargetUser = targetUsername,
                    Timestamp = DateTime.UtcNow
                };
                
                // 发送给目标用户和发送者
                await Clients.Client(targetUser.ConnectionId).SendAsync("ReceivePrivateMessage", message);
                await Clients.Caller.SendAsync("ReceivePrivateMessage", message);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", $"用户 {targetUsername} 不在线");
            }
        }
    }

    /// <summary>
    /// 加入群组
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public async Task JoinGroup(string groupName)
    {
        if (_connectedUsers.TryGetValue(Context.ConnectionId, out var user))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            if (!_groups.ContainsKey(groupName))
            {
                _groups.TryAdd(groupName, new GroupInfo
                {
                    Name = groupName,
                    Description = $"群组 {groupName}",
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            if (_groups.TryGetValue(groupName, out var group))
            {
                if (!group.Members.Contains(user.Username))
                {
                    group.Members.Add(user.Username);
                    group.MemberCount = group.Members.Count;
                }
                
                if (!user.Groups.Contains(groupName))
                {
                    user.Groups.Add(groupName);
                }
            }
            
            await Clients.Group(groupName).SendAsync("UserJoinedGroup", user.Username, groupName);
            await Clients.Caller.SendAsync("JoinedGroup", groupName);
        }
    }

    /// <summary>
    /// 离开群组
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public async Task LeaveGroup(string groupName)
    {
        if (_connectedUsers.TryGetValue(Context.ConnectionId, out var user))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            if (_groups.TryGetValue(groupName, out var group))
            {
                group.Members.Remove(user.Username);
                group.MemberCount = group.Members.Count;
            }
            
            user.Groups.Remove(groupName);
            
            await Clients.Group(groupName).SendAsync("UserLeftGroup", user.Username, groupName);
            await Clients.Caller.SendAsync("LeftGroup", groupName);
        }
    }

    /// <summary>
    /// 发送群组消息
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task SendGroupMessage(string groupName, string content)
    {
        if (_connectedUsers.TryGetValue(Context.ConnectionId, out var user))
        {
            var message = new SignalRMessage
            {
                Username = user.Username,
                Content = content,
                Type = MessageType.Text,
                GroupName = groupName,
                Timestamp = DateTime.UtcNow
            };
            
            await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", message);
        }
    }

    /// <summary>
    /// 获取在线用户列表
    /// </summary>
    /// <returns></returns>
    public async Task GetOnlineUsers()
    {
        var stats = new OnlineUserStats
        {
            TotalUsers = _connectedUsers.Count,
            Users = _connectedUsers.Values.ToList(),
            ActiveGroups = _groups.Count,
            Timestamp = DateTime.UtcNow
        };
        
        await Clients.Caller.SendAsync("OnlineUserStats", stats);
    }

    /// <summary>
    /// 获取群组列表
    /// </summary>
    /// <returns></returns>
    public async Task GetGroups()
    {
        var groups = _groups.Values.ToList();
        await Clients.Caller.SendAsync("GroupList", groups);
    }

    /// <summary>
    /// 发送通知
    /// </summary>
    /// <param name="notification"></param>
    /// <returns></returns>
    public async Task SendNotification(SignalRNotification notification)
    {
        if (_connectedUsers.TryGetValue(Context.ConnectionId, out var user))
        {
            notification.Timestamp = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(notification.TargetUser))
            {
                // 发送给特定用户
                var targetUser = _connectedUsers.Values.FirstOrDefault(u => u.Username == notification.TargetUser);
                if (targetUser != null)
                {
                    await Clients.Client(targetUser.ConnectionId).SendAsync("ReceiveNotification", notification);
                }
            }
            else if (!string.IsNullOrEmpty(notification.TargetGroup))
            {
                // 发送给特定群组
                await Clients.Group(notification.TargetGroup).SendAsync("ReceiveNotification", notification);
            }
            else
            {
                // 广播给所有用户
                await Clients.All.SendAsync("ReceiveNotification", notification);
            }
        }
    }

    /// <summary>
    /// 更新在线用户数量
    /// </summary>
    /// <returns></returns>
    private async Task UpdateOnlineUserCount()
    {
        var count = _connectedUsers.Count;
        await Clients.All.SendAsync("OnlineUserCountUpdated", count);
    }

    /// <summary>
    /// 获取连接的用户信息
    /// </summary>
    /// <returns></returns>
    public static List<ConnectedUser> GetConnectedUsers()
    {
        return _connectedUsers.Values.ToList();
    }

    /// <summary>
    /// 获取群组信息（静态方法）
    /// </summary>
    /// <returns></returns>
    public static List<GroupInfo> GetAllGroups()
    {
        return _groups.Values.ToList();
    }
}