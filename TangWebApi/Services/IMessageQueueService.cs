using System.Threading.Tasks;

namespace TangWebApi.Services
{
    /// <summary>
    /// 消息队列服务接口
    /// </summary>
    public interface IMessageQueueService
    {
        /// <summary>
        /// 发送消息到指定队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        Task SendMessageAsync(string queueName, object message);

        /// <summary>
        /// 发送消息到指定队列（字符串格式）
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        Task SendMessageAsync(string queueName, string message);

        /// <summary>
        /// 从指定队列接收消息
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="handler">消息处理器</param>
        /// <returns></returns>
        Task ReceiveMessageAsync(string queueName, Func<string, Task> handler);

        /// <summary>
        /// 从指定队列接收消息（泛型版本）
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="queueName">队列名称</param>
        /// <param name="handler">消息处理器</param>
        /// <returns></returns>
        Task ReceiveMessageAsync<T>(string queueName, Func<T, Task> handler) where T : class;

        /// <summary>
        /// 创建队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="durable">是否持久化</param>
        /// <returns></returns>
        Task CreateQueueAsync(string queueName, bool durable = true);

        /// <summary>
        /// 删除队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        Task DeleteQueueAsync(string queueName);

        /// <summary>
        /// 获取队列消息数量
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        Task<uint> GetMessageCountAsync(string queueName);

        /// <summary>
        /// 清空队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        Task PurgeQueueAsync(string queueName);

        /// <summary>
        /// 测试连接
        /// </summary>
        /// <returns>连接是否成功</returns>
        Task<bool> TestConnectionAsync();
    }
}