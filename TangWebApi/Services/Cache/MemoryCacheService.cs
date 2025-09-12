using Microsoft.Extensions.Caching.Memory;

namespace TangWebApi.Services
{
    /// <summary>
    /// 内存缓存服务实现
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly HashSet<string> _keys = new();
        private readonly object _lockObject = new();

        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        /// <summary>
        /// 获取缓存值
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        public Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = _memoryCache.Get<T>(key);
                return Task.FromResult(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "内存缓存获取失败，Key: {Key}", key);
                return Task.FromResult(default(T));
            }
        }

        /// <summary>
        /// 设置缓存值
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expiration">过期时间</param>
        /// <returns></returns>
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new MemoryCacheEntryOptions();
                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration;
                }

                // 设置过期回调，用于从键集合中移除
                options.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
                {
                    EvictionCallback = (k, v, reason, state) =>
                    {
                        lock (_lockObject)
                        {
                            _keys.Remove(k.ToString()!);
                        }
                    }
                });

                _memoryCache.Set(key, value, options);

                // 添加到键集合
                lock (_lockObject)
                {
                    _keys.Add(key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "内存缓存设置失败，Key: {Key}", key);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                lock (_lockObject)
                {
                    _keys.Remove(key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "内存缓存删除失败，Key: {Key}", key);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 检查缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public Task<bool> ExistsAsync(string key)
        {
            try
            {
                return Task.FromResult(_memoryCache.TryGetValue(key, out _));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "内存缓存检查存在性失败，Key: {Key}", key);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        /// <returns></returns>
        public Task ClearAsync()
        {
            try
            {
                lock (_lockObject)
                {
                    foreach (var key in _keys.ToList())
                    {
                        _memoryCache.Remove(key);
                    }
                    _keys.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "内存缓存清空失败");
            }

            return Task.CompletedTask;
        }
    }
}