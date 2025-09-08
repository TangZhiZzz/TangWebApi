using StackExchange.Redis;
using System.Text.Json;

namespace TangWebApi.Services
{
    /// <summary>
    /// Redis缓存服务实现
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }

        /// <summary>
        /// 获取缓存值
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                if (!value.HasValue)
                    return default;

                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis获取缓存失败，Key: {Key}", key);
                return default;
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
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var jsonValue = JsonSerializer.Serialize(value);
                await _database.StringSetAsync(key, jsonValue, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis设置缓存失败，Key: {Key}", key);
            }
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis删除缓存失败，Key: {Key}", key);
            }
        }

        /// <summary>
        /// 检查缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis检查缓存存在性失败，Key: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        /// <returns></returns>
        public async Task ClearAsync()
        {
            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                await server.FlushDatabaseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis清空缓存失败");
            }
        }
    }
}