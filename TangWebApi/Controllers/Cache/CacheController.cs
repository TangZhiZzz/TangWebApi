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
        public async Task SetCache([FromQuery] string key, [FromQuery] string value, [FromQuery] int? expireMinutes = null)
        {
            try
            {
                TimeSpan? expiration = expireMinutes.HasValue ? TimeSpan.FromMinutes(expireMinutes.Value) : null;
                await _cacheService.SetAsync(key, value, expiration);
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
        public async Task<string> GetCache([FromQuery] string key)
        {
            try
            {
                return await _cacheService.GetAsync<string>(key);

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
        public async Task RemoveCache([FromQuery] string key)
        {
            try
            {
                await _cacheService.RemoveAsync(key);
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
        public async Task<bool> ExistsCache([FromQuery] string key)
        {
            try
            {
                return await _cacheService.ExistsAsync(key);
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
        public async Task ClearCache()
        {
            try
            {
                await _cacheService.ClearAsync();
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
        public string GetCacheType()
        {
            var cacheType = _cacheService.GetType().Name;
            return cacheType;
        }
    }

}