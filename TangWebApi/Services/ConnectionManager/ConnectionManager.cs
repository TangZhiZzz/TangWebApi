using System.Collections.Concurrent;
using TangWebApi.Models;

namespace TangWebApi.Services
{
    /// <summary>
    /// SignalR连接管理服务
    /// </summary>
    public class ConnectionManager : IConnectionManager
    {
        // 连接ID -> 用户信息映射
        private readonly ConcurrentDictionary<string, ChatUser> _connections = new();
        
        // 用户ID -> 连接ID列表映射
        private readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
        
        // 群组名 -> 连接ID列表映射
        private readonly ConcurrentDictionary<string, HashSet<string>> _groupConnections = new();
        
        // 正在输入的用户ID集合
        private readonly ConcurrentDictionary<string, DateTime> _typingUsers = new();

        /// <summary>
        /// 添加连接
        /// </summary>
        public async Task AddConnectionAsync(string connectionId, string userId, string userName)
        {
            var user = new ChatUser
            {
                Id = userId,
                Name = userName,
                IsOnline = true,
                LastSeen = DateTime.UtcNow,
                ConnectionId = connectionId
            };

            _connections.TryAdd(connectionId, user);

            // 添加到用户连接映射
            _userConnections.AddOrUpdate(userId,
                new HashSet<string> { connectionId },
                (key, existing) =>
                {
                    existing.Add(connectionId);
                    return existing;
                });

            await Task.CompletedTask;
        }

        /// <summary>
        /// 移除连接
        /// </summary>
        public async Task RemoveConnectionAsync(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out var user))
            {
                // 从用户连接映射中移除
                if (_userConnections.TryGetValue(user.Id, out var connections))
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        _userConnections.TryRemove(user.Id, out _);
                        // 用户完全离线，移除输入状态
                        _typingUsers.TryRemove(user.Id, out _);
                    }
                }

                // 从所有群组中移除
                var groupsToRemove = new List<string>();
                foreach (var group in _groupConnections)
                {
                    if (group.Value.Remove(connectionId) && group.Value.Count == 0)
                    {
                        groupsToRemove.Add(group.Key);
                    }
                }

                // 移除空群组
                foreach (var groupName in groupsToRemove)
                {
                    _groupConnections.TryRemove(groupName, out _);
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取用户的所有连接
        /// </summary>
        public async Task<List<string>> GetUserConnectionsAsync(string userId)
        {
            _userConnections.TryGetValue(userId, out var connections);
            return await Task.FromResult(connections?.ToList() ?? new List<string>());
        }

        /// <summary>
        /// 获取连接的用户信息
        /// </summary>
        public async Task<ChatUser?> GetConnectionUserAsync(string connectionId)
        {
            _connections.TryGetValue(connectionId, out var user);
            return await Task.FromResult(user);
        }

        /// <summary>
        /// 获取所有在线用户
        /// </summary>
        public async Task<List<ChatUser>> GetOnlineUsersAsync()
        {
            var onlineUsers = _userConnections.Keys
                .Select(userId =>
                {
                    // 获取用户的第一个连接来获取用户信息
                    var userConnections = _userConnections[userId];
                    var firstConnection = userConnections.FirstOrDefault();
                    if (firstConnection != null && _connections.TryGetValue(firstConnection, out var user))
                    {
                        return new ChatUser
                        {
                            Id = user.Id,
                            Name = user.Name,
                            IsOnline = true,
                            LastSeen = DateTime.UtcNow,
                            ConnectionId = firstConnection
                        };
                    }
                    return null;
                })
                .Where(user => user != null)
                .Cast<ChatUser>()
                .ToList();

            return await Task.FromResult(onlineUsers);
        }

        /// <summary>
        /// 用户加入群组
        /// </summary>
        public async Task JoinGroupAsync(string connectionId, string groupName)
        {
            _groupConnections.AddOrUpdate(groupName,
                new HashSet<string> { connectionId },
                (key, existing) =>
                {
                    existing.Add(connectionId);
                    return existing;
                });

            await Task.CompletedTask;
        }

        /// <summary>
        /// 用户离开群组
        /// </summary>
        public async Task LeaveGroupAsync(string connectionId, string groupName)
        {
            if (_groupConnections.TryGetValue(groupName, out var connections))
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _groupConnections.TryRemove(groupName, out _);
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取群组中的所有连接
        /// </summary>
        public async Task<List<string>> GetGroupConnectionsAsync(string groupName)
        {
            _groupConnections.TryGetValue(groupName, out var connections);
            return await Task.FromResult(connections?.ToList() ?? new List<string>());
        }

        /// <summary>
        /// 设置用户正在输入状态
        /// </summary>
        public async Task SetUserTypingAsync(string userId, bool isTyping)
        {
            if (isTyping)
            {
                _typingUsers.AddOrUpdate(userId, DateTime.UtcNow, (key, existing) => DateTime.UtcNow);
            }
            else
            {
                _typingUsers.TryRemove(userId, out _);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取正在输入的用户列表
        /// </summary>
        public async Task<List<string>> GetTypingUsersAsync()
        {
            var now = DateTime.UtcNow;
            var expiredUsers = new List<string>();

            // 清理过期的输入状态（超过30秒）
            foreach (var kvp in _typingUsers)
            {
                if (now - kvp.Value > TimeSpan.FromSeconds(30))
                {
                    expiredUsers.Add(kvp.Key);
                }
            }

            foreach (var userId in expiredUsers)
            {
                _typingUsers.TryRemove(userId, out _);
            }

            return await Task.FromResult(_typingUsers.Keys.ToList());
        }

        /// <summary>
        /// 根据用户名获取用户的连接ID
        /// </summary>
        public async Task<string?> GetUserConnectionIdAsync(string userName)
        {
            // 遍历所有连接，查找匹配用户名的连接
            var connection = _connections.FirstOrDefault(kvp => kvp.Value.Name == userName);
            return await Task.FromResult(connection.Key);
        }

        /// <summary>
        /// 将用户添加到群组
        /// </summary>
        public async Task AddUserToGroupAsync(string userName, string groupName)
        {
            // 获取用户的所有连接
            var user = _connections.Values.FirstOrDefault(u => u.Name == userName);
            if (user != null)
            {
                // 获取用户的所有连接ID
                var userConnections = await GetUserConnectionsAsync(user.Id);
                
                // 将所有连接添加到群组
                foreach (var connectionId in userConnections)
                {
                    await JoinGroupAsync(connectionId, groupName);
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 从群组中移除用户
        /// </summary>
        public async Task RemoveUserFromGroupAsync(string userName, string groupName)
        {
            // 获取用户的所有连接
            var user = _connections.Values.FirstOrDefault(u => u.Name == userName);
            if (user != null)
            {
                // 获取用户的所有连接ID
                var userConnections = await GetUserConnectionsAsync(user.Id);
                
                // 从群组中移除所有连接
                foreach (var connectionId in userConnections)
                {
                    await LeaveGroupAsync(connectionId, groupName);
                }
            }

            await Task.CompletedTask;
        }
    }
}