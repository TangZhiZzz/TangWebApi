namespace TangWebApi.Services
{
    /// <summary>
    /// 缓存服务接口
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// 获取缓存值
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// 设置缓存值
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expiration">过期时间</param>
        /// <returns></returns>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        Task RemoveAsync(string key);

        /// <summary>
        /// 检查缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        /// <returns></returns>
        Task ClearAsync();
    }
}