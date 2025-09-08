using Microsoft.AspNetCore.Mvc;
using TangWebApi.Services;

namespace TangWebApi.Controllers
{
    /// <summary>
    /// 缓存测试控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheController> _logger;

        public CacheController(ICacheService cacheService, ILogger<CacheController> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expireMinutes">过期时间（分钟）</param>
        /// <returns></returns>
        [HttpPost("set")]
        public async Task<CacheSetResponse> SetCache([FromQuery] string key, [FromQuery] string value, [FromQuery] int? expireMinutes = null)
        {
            try
            {
                TimeSpan? expiration = expireMinutes.HasValue ? TimeSpan.FromMinutes(expireMinutes.Value) : null;
                await _cacheService.SetAsync(key, value, expiration);
                return new CacheSetResponse { Message = "缓存设置成功", Key = key, Value = value, ExpireMinutes = expireMinutes };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置缓存失败");
                throw new InvalidOperationException($"设置缓存失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        [HttpGet("get")]
        public async Task<CacheGetResponse> GetCache([FromQuery] string key)
        {
            try
            {
                var value = await _cacheService.GetAsync<string>(key);
                if (value == null)
                {
                    throw new KeyNotFoundException($"缓存不存在: {key}");
                }
                return new CacheGetResponse { Message = "获取缓存成功", Key = key, Value = value };
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取缓存失败");
                throw new InvalidOperationException($"获取缓存失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        [HttpDelete("remove")]
        public async Task<CacheRemoveResponse> RemoveCache([FromQuery] string key)
        {
            try
            {
                await _cacheService.RemoveAsync(key);
                return new CacheRemoveResponse { Message = "删除缓存成功", Key = key };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除缓存失败");
                throw new InvalidOperationException($"删除缓存失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        [HttpGet("exists")]
        public async Task<CacheExistsResponse> ExistsCache([FromQuery] string key)
        {
            try
            {
                var exists = await _cacheService.ExistsAsync(key);
                return new CacheExistsResponse { Message = "检查缓存完成", Key = key, Exists = exists };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查缓存失败");
                throw new InvalidOperationException($"检查缓存失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        /// <returns></returns>
        [HttpPost("clear")]
        public async Task<CacheClearResponse> ClearCache()
        {
            try
            {
                await _cacheService.ClearAsync();
                return new CacheClearResponse { Message = "清空缓存成功" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空缓存失败");
                throw new InvalidOperationException($"清空缓存失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取缓存服务类型
        /// </summary>
        /// <returns></returns>
        [HttpGet("type")]
        public CacheTypeResponse GetCacheType()
        {
            var cacheType = _cacheService.GetType().Name;
            return new CacheTypeResponse { Message = "获取缓存类型成功", CacheType = cacheType };
        }
    }

    // 缓存相关响应模型
    public class CacheSetResponse
    {
        public required string Message { get; set; }
        public required string Key { get; set; }
        public required string Value { get; set; }
        public int? ExpireMinutes { get; set; }
    }

    public class CacheGetResponse
    {
        public required string Message { get; set; }
        public required string Key { get; set; }
        public required string Value { get; set; }
    }

    public class CacheRemoveResponse
    {
        public required string Message { get; set; }
        public required string Key { get; set; }
    }

    public class CacheExistsResponse
    {
        public required string Message { get; set; }
        public required string Key { get; set; }
        public bool Exists { get; set; }
    }

    public class CacheClearResponse
    {
        public required string Message { get; set; }
    }

    public class CacheTypeResponse
    {
        public required string Message { get; set; }
        public required string CacheType { get; set; }
    }
}