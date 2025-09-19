using TangWebApi.Models;

namespace TangWebApi.Services
{
    /// <summary>
    /// SignalR连接管理接口
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        /// 添加连接
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户名</param>
        Task AddConnectionAsync(string connectionId, string userId, string userName);

        /// <summary>
        /// 移除连接
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        Task RemoveConnectionAsync(string connectionId);

        /// <summary>
        /// 获取用户的所有连接
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>连接ID列表</returns>
        Task<List<string>> GetUserConnectionsAsync(string userId);

        /// <summary>
        /// 获取连接的用户信息
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>用户信息</returns>
        Task<ChatUser?> GetConnectionUserAsync(string connectionId);

        /// <summary>
        /// 获取所有在线用户
        /// </summary>
        /// <returns>在线用户列表</returns>
        Task<List<ChatUser>> GetOnlineUsersAsync();

        /// <summary>
        /// 用户加入群组
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="groupName">群组名称</param>
        Task JoinGroupAsync(string connectionId, string groupName);

        /// <summary>
        /// 用户离开群组
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="groupName">群组名称</param>
        Task LeaveGroupAsync(string connectionId, string groupName);

        /// <summary>
        /// 获取群组中的所有连接
        /// </summary>
        /// <param name="groupName">群组名称</param>
        /// <returns>连接ID列表</returns>
        Task<List<string>> GetGroupConnectionsAsync(string groupName);

        /// <summary>
        /// 设置用户正在输入状态
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="isTyping">是否正在输入</param>
        Task SetUserTypingAsync(string userId, bool isTyping);

        /// <summary>
        /// 获取正在输入的用户列表
        /// </summary>
        /// <returns>正在输入的用户ID列表</returns>
        Task<List<string>> GetTypingUsersAsync();

        /// <summary>
        /// 根据用户名获取用户的连接ID
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <returns>连接ID，如果用户不在线则返回null</returns>
        Task<string?> GetUserConnectionIdAsync(string userName);

        /// <summary>
        /// 将用户添加到群组
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="groupName">群组名称</param>
        Task AddUserToGroupAsync(string userName, string groupName);

        /// <summary>
        /// 从群组中移除用户
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="groupName">群组名称</param>
        Task RemoveUserFromGroupAsync(string userName, string groupName);
    }
}