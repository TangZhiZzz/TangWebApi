using TangWebApi.Models;

namespace TangWebApi.Services
{
    /// <summary>
    /// SignalR服务接口
    /// </summary>
    public interface ISignalRService
    {
        /// <summary>
        /// 发送消息给所有用户
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        Task SendMessageToAllAsync(SignalRMessage message);

        /// <summary>
        /// 发送消息给指定用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        Task SendMessageToUserAsync(string userId, SignalRMessage message);

        /// <summary>
        /// 发送消息给指定群组
        /// </summary>
        /// <param name="groupName">群组名称</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        Task SendMessageToGroupAsync(string groupName, SignalRMessage message);

        /// <summary>
        /// 发送通知给所有用户
        /// </summary>
        /// <param name="notification">通知内容</param>
        /// <returns></returns>
        Task SendNotificationToAllAsync(SignalRNotification notification);

        /// <summary>
        /// 发送通知给指定用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="notification">通知内容</param>
        /// <returns></returns>
        Task SendNotificationToUserAsync(string userId, SignalRNotification notification);

        /// <summary>
        /// 发送通知给指定群组
        /// </summary>
        /// <param name="groupName">群组名称</param>
        /// <param name="notification">通知内容</param>
        /// <returns></returns>
        Task SendNotificationToGroupAsync(string groupName, SignalRNotification notification);

        /// <summary>
        /// 获取在线用户列表
        /// </summary>
        /// <returns></returns>
        Task<List<ConnectedUser>> GetOnlineUsersAsync();

        /// <summary>
        /// 添加用户到群组
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="groupName">群组名称</param>
        /// <returns></returns>
        Task AddToGroupAsync(string connectionId, string groupName);

        /// <summary>
        /// 从群组移除用户
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="groupName">群组名称</param>
        /// <returns></returns>
        Task RemoveFromGroupAsync(string connectionId, string groupName);
    }
}