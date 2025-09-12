using Microsoft.AspNetCore.SignalR;
using TangWebApi.Models;
using TangWebApi.Hubs;
using System.Collections.Concurrent;

namespace TangWebApi.Services
{
    /// <summary>
    /// SignalR服务实现
    /// </summary>
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<SignalRService> _logger;
        private static readonly ConcurrentDictionary<string, ConnectedUser> _connectedUsers = new();
        private static readonly ConcurrentDictionary<string, HashSet<string>> _groupUsers = new();

        public SignalRService(IHubContext<ChatHub> hubContext, ILogger<SignalRService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// 发送消息给所有用户
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        public async Task SendMessageToAllAsync(SignalRMessage message)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
                _logger.LogInformation("消息已发送给所有用户: {Content}", message.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送消息给所有用户失败");
                throw;
            }
        }

        /// <summary>
        /// 发送消息给指定用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        public async Task SendMessageToUserAsync(string userId, SignalRMessage message)
        {
            try
            {
                await _hubContext.Clients.User(userId).SendAsync("ReceiveMessage", message);
                _logger.LogInformation("消息已发送给用户 {UserId}: {Content}", userId, message.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送消息给用户 {UserId} 失败", userId);
                throw;
            }
        }

        /// <summary>
        /// 发送消息给指定群组
        /// </summary>
        /// <param name="groupName">群组名称</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        public async Task SendMessageToGroupAsync(string groupName, SignalRMessage message)
        {
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", message);
                _logger.LogInformation("消息已发送给群组 {GroupName}: {Content}", groupName, message.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送消息给群组 {GroupName} 失败", groupName);
                throw;
            }
        }

        /// <summary>
        /// 发送通知给所有用户
        /// </summary>
        /// <param name="notification">通知内容</param>
        /// <returns></returns>
        public async Task SendNotificationToAllAsync(SignalRNotification notification)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
                _logger.LogInformation("通知已发送给所有用户: {Title}", notification.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送通知给所有用户失败");
                throw;
            }
        }

        /// <summary>
        /// 发送通知给指定用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="notification">通知内容</param>
        /// <returns></returns>
        public async Task SendNotificationToUserAsync(string userId, SignalRNotification notification)
        {
            try
            {
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
                _logger.LogInformation("通知已发送给用户 {UserId}: {Title}", userId, notification.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送通知给用户 {UserId} 失败", userId);
                throw;
            }
        }

        /// <summary>
        /// 发送通知给指定群组
        /// </summary>
        /// <param name="groupName">群组名称</param>
        /// <param name="notification">通知内容</param>
        /// <returns></returns>
        public async Task SendNotificationToGroupAsync(string groupName, SignalRNotification notification)
        {
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
                _logger.LogInformation("通知已发送给群组 {GroupName}: {Title}", groupName, notification.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送通知给群组 {GroupName} 失败", groupName);
                throw;
            }
        }

        /// <summary>
        /// 获取在线用户列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<ConnectedUser>> GetOnlineUsersAsync()
        {
            try
            {
                return await Task.FromResult(_connectedUsers.Values.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取在线用户列表失败");
                throw;
            }
        }

        /// <summary>
        /// 添加用户到群组
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="groupName">群组名称</param>
        /// <returns></returns>
        public async Task AddToGroupAsync(string connectionId, string groupName)
        {
            try
            {
                await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
                
                // 更新群组用户列表
                _groupUsers.AddOrUpdate(groupName, 
                    new HashSet<string> { connectionId },
                    (key, existingSet) => 
                    {
                        existingSet.Add(connectionId);
                        return existingSet;
                    });

                // 更新用户的群组列表
                if (_connectedUsers.TryGetValue(connectionId, out var user))
                {
                    user.Groups.Add(groupName);
                }

                _logger.LogInformation("用户 {ConnectionId} 已加入群组 {GroupName}", connectionId, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加用户 {ConnectionId} 到群组 {GroupName} 失败", connectionId, groupName);
                throw;
            }
        }

        /// <summary>
        /// 从群组移除用户
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="groupName">群组名称</param>
        /// <returns></returns>
        public async Task RemoveFromGroupAsync(string connectionId, string groupName)
        {
            try
            {
                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
                
                // 更新群组用户列表
                if (_groupUsers.TryGetValue(groupName, out var groupUsers))
                {
                    groupUsers.Remove(connectionId);
                    if (groupUsers.Count == 0)
                    {
                        _groupUsers.TryRemove(groupName, out _);
                    }
                }

                // 更新用户的群组列表
                if (_connectedUsers.TryGetValue(connectionId, out var user))
                {
                    user.Groups.Remove(groupName);
                }

                _logger.LogInformation("用户 {ConnectionId} 已离开群组 {GroupName}", connectionId, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从群组 {GroupName} 移除用户 {ConnectionId} 失败", groupName, connectionId);
                throw;
            }
        }

        /// <summary>
        /// 添加连接用户
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="username">用户名</param>
        public static void AddConnectedUser(string connectionId, string username)
        {
            var user = new ConnectedUser
            {
                ConnectionId = connectionId,
                Username = username,
                ConnectedAt = DateTime.UtcNow
            };
            _connectedUsers.TryAdd(connectionId, user);
        }

        /// <summary>
        /// 移除连接用户
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        public static void RemoveConnectedUser(string connectionId)
        {
            if (_connectedUsers.TryRemove(connectionId, out var user))
            {
                // 从所有群组中移除用户
                foreach (var groupName in user.Groups.ToList())
                {
                    if (_groupUsers.TryGetValue(groupName, out var groupUsers))
                    {
                        groupUsers.Remove(connectionId);
                        if (groupUsers.Count == 0)
                        {
                            _groupUsers.TryRemove(groupName, out _);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取群组列表
        /// </summary>
        /// <returns></returns>
        public static List<string> GetGroups()
        {
            return _groupUsers.Keys.ToList();
        }
    }
}