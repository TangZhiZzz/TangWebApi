using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TangWebApi.Models;
using TangWebApi.Services;

namespace TangWebApi.Hubs
{
    /// <summary>
    /// 聊天Hub - 处理实时聊天功能
    /// </summary>
    public class ChatHub : Hub
    {
        // 群组信息（在生产环境中应使用Redis等分布式缓存）
        private static readonly ConcurrentDictionary<string, ChatGroup> _chatGroups = new();

        private readonly ILogger<ChatHub> _logger;
        private readonly IConnectionManager _connectionManager;

        public ChatHub(ILogger<ChatHub> logger, IConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }

        #region 连接管理

        /// <summary>
        /// 客户端连接时调用
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var userName = Context.User?.Identity?.Name ?? $"User_{connectionId[..8]}";
            var userId = Context.User?.FindFirst("sub")?.Value ?? userName;

            _logger.LogInformation("用户 {UserName} 连接到ChatHub，连接ID: {ConnectionId}", userName, connectionId);

            // 使用连接管理服务记录连接
            await _connectionManager.AddConnectionAsync(connectionId, userId, userName);

            // 通知所有客户端有新用户加入
            await Clients.All.SendAsync("UserJoined", userName);

            // 发送当前在线用户列表给新连接的客户端
            var onlineUsers = await _connectionManager.GetOnlineUsersAsync();
            await Clients.Caller.SendAsync("OnlineUsersList", onlineUsers);

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 客户端断开连接时调用
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            
            // 获取用户信息
            var user = await _connectionManager.GetConnectionUserAsync(connectionId);
            if (user != null)
            {
                _logger.LogInformation("用户 {UserName} 断开连接，连接ID: {ConnectionId}", user.Name, connectionId);

                // 使用连接管理服务移除连接
                await _connectionManager.RemoveConnectionAsync(connectionId);

                // 通知所有客户端用户离开
                await Clients.All.SendAsync("UserLeft", user.Name);
            }

            if (exception != null)
            {
                _logger.LogError(exception, "用户断开连接时发生异常");
            }

            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region 消息发送

        /// <summary>
        /// 发送公共消息
        /// </summary>
        /// <param name="user">发送用户</param>
        /// <param name="message">消息内容</param>
        public async Task SendMessage(string user, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(message))
                {
                    await Clients.Caller.SendAsync("Error", "用户名和消息内容不能为空");
                    return;
                }

                var chatMessage = new ChatMessage
                {
                    User = user,
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    MessageType = ChatMessageType.Text
                };

                _logger.LogInformation("用户 {User} 发送公共消息: {Message}", user, message);

                // 广播消息给所有连接的客户端
                await Clients.All.SendAsync("ReceiveMessage", user, message, chatMessage.Timestamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送公共消息时发生错误");
                await Clients.Caller.SendAsync("Error", "发送消息失败");
            }
        }

        /// <summary>
        /// 发送私聊消息
        /// </summary>
        /// <param name="toUser">目标用户</param>
        /// <param name="message">消息内容</param>
        public async Task SendPrivateMessage(string toUser, string message)
        {
            try
            {
                var fromUser = GetCurrentUserName();
                if (string.IsNullOrWhiteSpace(fromUser) || string.IsNullOrWhiteSpace(toUser) || string.IsNullOrWhiteSpace(message))
                {
                    await Clients.Caller.SendAsync("Error", "用户名和消息内容不能为空");
                    return;
                }

                // 使用连接管理服务获取目标用户连接
                var targetConnectionId = await _connectionManager.GetUserConnectionIdAsync(toUser);
                if (!string.IsNullOrEmpty(targetConnectionId))
                {
                    var chatMessage = new ChatMessage
                    {
                        User = fromUser,
                        Message = message,
                        Timestamp = DateTime.UtcNow,
                        MessageType = ChatMessageType.Text,
                        ToUser = toUser
                    };

                    _logger.LogInformation("用户 {FromUser} 向 {ToUser} 发送私聊消息: {Message}", fromUser, toUser, message);

                    // 发送给目标用户
                    await Clients.Client(targetConnectionId).SendAsync("ReceivePrivateMessage", fromUser, message, chatMessage.Timestamp);
                    
                    // 发送给发送者确认
                    await Clients.Caller.SendAsync("PrivateMessageSent", toUser, message, chatMessage.Timestamp);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", $"用户 {toUser} 不在线");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送私聊消息时发生错误");
                await Clients.Caller.SendAsync("Error", "发送私聊消息失败");
            }
        }

        #endregion

        #region 群组管理

        /// <summary>
        /// 加入群组
        /// </summary>
        /// <param name="groupName">群组名称</param>
        public async Task JoinGroup(string groupName)
        {
            try
            {
                var userName = GetCurrentUserName();
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(groupName))
                {
                    await Clients.Caller.SendAsync("Error", "用户名和群组名不能为空");
                    return;
                }

                // 创建或获取群组
                var group = _chatGroups.GetOrAdd(groupName, _ => new ChatGroup
                {
                    Name = groupName,
                    CreatedAt = DateTime.UtcNow
                });

                // 检查群组成员数量限制
                if (group.Members.Count >= group.MaxMembers)
                {
                    await Clients.Caller.SendAsync("Error", $"群组 {groupName} 已达到最大成员数限制");
                    return;
                }

                // 将用户添加到群组
                if (!group.Members.Contains(userName))
                {
                    group.Members.Add(userName);
                }

                // 使用连接管理服务添加用户到群组
                await _connectionManager.AddUserToGroupAsync(userName, groupName);

                // 将连接添加到SignalR群组
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                _logger.LogInformation("用户 {UserName} 加入群组 {GroupName}", userName, groupName);

                // 通知群组内所有成员
                await Clients.Group(groupName).SendAsync("UserJoinedGroup", userName, groupName);
                
                // 发送群组成员列表给新加入的用户
                await Clients.Caller.SendAsync("GroupMembersList", groupName, group.Members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加入群组时发生错误");
                await Clients.Caller.SendAsync("Error", "加入群组失败");
            }
        }

        /// <summary>
        /// 离开群组
        /// </summary>
        /// <param name="groupName">群组名称</param>
        public async Task LeaveGroup(string groupName)
        {
            try
            {
                var userName = GetCurrentUserName();
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(groupName))
                {
                    await Clients.Caller.SendAsync("Error", "用户名和群组名不能为空");
                    return;
                }

                // 从群组中移除用户
                if (_chatGroups.TryGetValue(groupName, out var group))
                {
                    group.Members.Remove(userName);
                }

                // 使用连接管理服务从群组中移除用户
                await _connectionManager.RemoveUserFromGroupAsync(userName, groupName);

                // 从SignalR群组中移除连接
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

                _logger.LogInformation("用户 {UserName} 离开群组 {GroupName}", userName, groupName);

                // 通知群组内其他成员
                await Clients.Group(groupName).SendAsync("UserLeftGroup", userName, groupName);
                
                // 确认离开群组
                await Clients.Caller.SendAsync("LeftGroup", groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "离开群组时发生错误");
                await Clients.Caller.SendAsync("Error", "离开群组失败");
            }
        }

        /// <summary>
        /// 发送群组消息
        /// </summary>
        /// <param name="groupName">群组名称</param>
        /// <param name="message">消息内容</param>
        public async Task SendGroupMessage(string groupName, string message)
        {
            try
            {
                var userName = GetCurrentUserName();
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(groupName) || string.IsNullOrWhiteSpace(message))
                {
                    await Clients.Caller.SendAsync("Error", "用户名、群组名和消息内容不能为空");
                    return;
                }

                // 检查用户是否在群组中
                if (!_chatGroups.TryGetValue(groupName, out var group) || !group.Members.Contains(userName))
                {
                    await Clients.Caller.SendAsync("Error", $"您不在群组 {groupName} 中");
                    return;
                }

                var chatMessage = new ChatMessage
                {
                    User = userName,
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    MessageType = ChatMessageType.Text,
                    GroupName = groupName
                };

                _logger.LogInformation("用户 {UserName} 在群组 {GroupName} 发送消息: {Message}", userName, groupName, message);

                // 发送消息给群组内所有成员
                await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", groupName, userName, message, chatMessage.Timestamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送群组消息时发生错误");
                await Clients.Caller.SendAsync("Error", "发送群组消息失败");
            }
        }

        #endregion

        #region 用户状态

        /// <summary>
        /// 设置用户输入状态
        /// </summary>
        /// <param name="isTyping">是否正在输入</param>
        public async Task SetUserTyping(bool isTyping)
        {
            try
            {
                var userName = GetCurrentUserName();
                if (string.IsNullOrWhiteSpace(userName))
                {
                    return;
                }

                // 使用连接管理服务设置输入状态
                await _connectionManager.SetUserTypingAsync(userName, isTyping);

                _logger.LogDebug("用户 {UserName} 输入状态: {IsTyping}", userName, isTyping);

                // 通知所有其他用户
                await Clients.Others.SendAsync("UserTyping", userName, isTyping);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置用户输入状态时发生错误");
            }
        }

        /// <summary>
        /// 获取在线用户列表
        /// </summary>
        public async Task GetOnlineUsers()
        {
            try
            {
                var onlineUsers = await _connectionManager.GetOnlineUsersAsync();
                await Clients.Caller.SendAsync("OnlineUsersList", onlineUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取在线用户列表时发生错误");
                await Clients.Caller.SendAsync("Error", "获取在线用户列表失败");
            }
        }

        /// <summary>
        /// Ping测试连接
        /// </summary>
        public async Task Ping()
        {
            try
            {
                await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ping测试时发生错误");
            }
        }

        #endregion

        #region 系统通知

        /// <summary>
        /// 发送系统通知（仅管理员可用）
        /// </summary>
        /// <param name="message">通知消息</param>
        /// <param name="type">通知类型</param>
        public async Task SendSystemNotification(string message, string type = "info")
        {
            try
            {
                var userName = GetCurrentUserName();
                _logger.LogInformation("系统通知 - 发送者: {UserName}, 消息: {Message}, 类型: {Type}", userName, message, type);

                var notification = new SystemNotification
                {
                    Message = message,
                    Type = Enum.TryParse<NotificationType>(type, true, out var notificationType) 
                        ? notificationType 
                        : NotificationType.Info
                };

                // 广播系统通知给所有用户
                await Clients.All.SendAsync("SystemNotification", notification.Message, notification.Type.ToString().ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送系统通知时发生错误");
                await Clients.Caller.SendAsync("Error", "发送系统通知失败");
            }
        }

        #endregion

        #region 大厅聊天方法

        /// <summary>
        /// 用户加入大厅
        /// </summary>
        /// <param name="userName">用户名</param>
        public async Task JoinHall(string userName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                {
                    await Clients.Caller.SendAsync("Error", "用户名不能为空");
                    return;
                }

                var connectionId = Context.ConnectionId;
                _logger.LogInformation("用户 {UserName} 加入大厅，连接ID: {ConnectionId}", userName, connectionId);

                // 添加连接到连接管理器中
                await _connectionManager.AddConnectionAsync(connectionId, userName, userName);

                // 通知所有客户端有新用户加入
                await Clients.All.SendAsync("UserJoined", userName);

                // 发送当前在线用户列表给所有客户端
                var onlineUsers = await _connectionManager.GetOnlineUsersAsync();
                await Clients.All.SendAsync("UsersUpdated", onlineUsers.Select(u => u.Name).ToArray());

                // 发送欢迎消息给加入的用户
                await Clients.Caller.SendAsync("ReceiveMessage", "系统", $"欢迎 {userName} 加入大厅！");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户加入大厅时发生错误");
                await Clients.Caller.SendAsync("Error", "加入大厅失败");
            }
        }

        /// <summary>
        /// 发送消息到大厅
        /// </summary>
        /// <param name="userName">发送用户名</param>
        /// <param name="message">消息内容</param>
        public async Task SendToHall(string userName, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(message))
                {
                    await Clients.Caller.SendAsync("Error", "用户名和消息内容不能为空");
                    return;
                }

                var chatMessage = new ChatMessage
                {
                    User = userName,
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    MessageType = ChatMessageType.Text
                };

                _logger.LogInformation("用户 {User} 在大厅发送消息: {Message}", userName, message);

                // 广播消息给所有连接的客户端
                await Clients.All.SendAsync("ReceiveMessage", userName, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送大厅消息时发生错误");
                await Clients.Caller.SendAsync("Error", "发送消息失败");
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 获取当前用户名
        /// </summary>
        private string GetCurrentUserName()
        {
            var user = _connectionManager.GetConnectionUserAsync(Context.ConnectionId).Result;
            return user?.Name ?? Context.User?.Identity?.Name ?? $"User_{Context.ConnectionId[..8]}";
        }

        #endregion
    }
}