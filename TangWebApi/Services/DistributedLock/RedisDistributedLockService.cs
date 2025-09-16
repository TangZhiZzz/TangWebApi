using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Diagnostics;
using TangWebApi.Options;

namespace TangWebApi.Services
{
    /// <summary>
    /// 基于Redis的分布式锁服务实现
    /// </summary>
    public class RedisDistributedLockService : IDistributedLockService
    {
        private readonly IDatabase _database;
        private readonly DistributedLockSettings _settings;
        private readonly ILogger<RedisDistributedLockService> _logger;
        private readonly string _ownerIdentifier;

        public RedisDistributedLockService(
            IConnectionMultiplexer redis,
            IOptions<DistributedLockSettings> settings,
            ILogger<RedisDistributedLockService> logger)
        {
            _database = redis.GetDatabase();
            _settings = settings.Value;
            _logger = logger;
            _ownerIdentifier = GenerateOwnerIdentifier();
        }

        public async Task<IDistributedLock?> TryAcquireLockAsync(string lockKey, TimeSpan expirationTime, CancellationToken cancellationToken = default)
        {
            if (!_settings.Enabled)
            {
                _logger.LogWarning("分布式锁服务已禁用");
                return null;
            }

            var fullKey = GetFullKey(lockKey);
            var lockValue = GenerateLockValue();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // 使用SET命令的NX选项实现原子性获取锁
                var acquired = await _database.StringSetAsync(fullKey, lockValue, expirationTime, When.NotExists);
                
                stopwatch.Stop();
                
                if (_settings.EnableMonitoring && stopwatch.ElapsedMilliseconds > _settings.SlowLockThresholdMs)
                {
                    _logger.LogWarning("慢锁操作: 获取锁 {LockKey} 耗时 {ElapsedMs}ms", lockKey, stopwatch.ElapsedMilliseconds);
                }

                if (acquired)
                {
                    _logger.LogDebug("成功获取锁: {LockKey}, 值: {LockValue}, 过期时间: {ExpirationTime}", 
                        lockKey, lockValue, expirationTime);
                    
                    return new RedisDistributedLock(this, fullKey, lockValue, DateTime.UtcNow.Add(expirationTime));
                }
                else
                {
                    _logger.LogDebug("获取锁失败: {LockKey} 已被其他进程持有", lockKey);
                    return null;
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "获取锁时发生异常: {LockKey}", lockKey);
                return null;
            }
        }

        public async Task<IDistributedLock?> TryAcquireLockAsync(string lockKey, CancellationToken cancellationToken = default)
        {
            return await TryAcquireLockAsync(lockKey, TimeSpan.FromSeconds(_settings.DefaultExpirationSeconds), cancellationToken);
        }

        public async Task<IDistributedLock> AcquireLockAsync(string lockKey, TimeSpan expirationTime, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var retryCount = 0;

            while (stopwatch.Elapsed < timeout && !cancellationToken.IsCancellationRequested)
            {
                var lockResult = await TryAcquireLockAsync(lockKey, expirationTime, cancellationToken);
                if (lockResult != null)
                {
                    _logger.LogDebug("获取锁成功: {LockKey}, 重试次数: {RetryCount}, 总耗时: {ElapsedMs}ms", 
                        lockKey, retryCount, stopwatch.ElapsedMilliseconds);
                    return lockResult;
                }

                retryCount++;
                if (retryCount > _settings.MaxRetryCount)
                {
                    break;
                }

                await Task.Delay(_settings.RetryIntervalMs, cancellationToken);
            }

            stopwatch.Stop();
            var errorMessage = $"获取锁超时: {lockKey}, 超时时间: {timeout}, 重试次数: {retryCount}";
            _logger.LogWarning(errorMessage);
            throw new TimeoutException(errorMessage);
        }

        public async Task<IDistributedLock> AcquireLockAsync(string lockKey, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            return await AcquireLockAsync(lockKey, TimeSpan.FromSeconds(_settings.DefaultExpirationSeconds), timeout, cancellationToken);
        }

        public async Task<bool> ReleaseLockAsync(string lockKey, string lockValue, CancellationToken cancellationToken = default)
        {
            var fullKey = GetFullKey(lockKey);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // 使用Lua脚本确保原子性释放锁（只有锁的持有者才能释放）
                const string script = @"
                    if redis.call('GET', KEYS[1]) == ARGV[1] then
                        return redis.call('DEL', KEYS[1])
                    else
                        return 0
                    end";

                var result = await _database.ScriptEvaluateAsync(script, new RedisKey[] { fullKey }, new RedisValue[] { lockValue });
                
                stopwatch.Stop();
                
                var released = result.ToString() == "1";
                
                if (_settings.EnableMonitoring && stopwatch.ElapsedMilliseconds > _settings.SlowLockThresholdMs)
                {
                    _logger.LogWarning("慢锁操作: 释放锁 {LockKey} 耗时 {ElapsedMs}ms", lockKey, stopwatch.ElapsedMilliseconds);
                }

                if (released)
                {
                    _logger.LogDebug("成功释放锁: {LockKey}, 值: {LockValue}", lockKey, lockValue);
                }
                else
                {
                    _logger.LogWarning("释放锁失败: {LockKey}, 锁可能已过期或被其他进程持有", lockKey);
                }

                return released;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "释放锁时发生异常: {LockKey}", lockKey);
                return false;
            }
        }

        public async Task<bool> RenewLockAsync(string lockKey, string lockValue, TimeSpan expirationTime, CancellationToken cancellationToken = default)
        {
            var fullKey = GetFullKey(lockKey);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // 使用Lua脚本确保原子性续期锁
                const string script = @"
                    if redis.call('GET', KEYS[1]) == ARGV[1] then
                        return redis.call('EXPIRE', KEYS[1], ARGV[2])
                    else
                        return 0
                    end";

                var result = await _database.ScriptEvaluateAsync(script, 
                    new RedisKey[] { fullKey }, 
                    new RedisValue[] { lockValue, (int)expirationTime.TotalSeconds });
                
                stopwatch.Stop();
                
                var renewed = result.ToString() == "1";
                
                if (_settings.EnableMonitoring && stopwatch.ElapsedMilliseconds > _settings.SlowLockThresholdMs)
                {
                    _logger.LogWarning("慢锁操作: 续期锁 {LockKey} 耗时 {ElapsedMs}ms", lockKey, stopwatch.ElapsedMilliseconds);
                }

                if (renewed)
                {
                    _logger.LogDebug("成功续期锁: {LockKey}, 值: {LockValue}, 新过期时间: {ExpirationTime}", 
                        lockKey, lockValue, expirationTime);
                }
                else
                {
                    _logger.LogWarning("续期锁失败: {LockKey}, 锁可能已过期或被其他进程持有", lockKey);
                }

                return renewed;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "续期锁时发生异常: {LockKey}", lockKey);
                return false;
            }
        }

        public async Task<bool> IsLockExistsAsync(string lockKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = GetFullKey(lockKey);
                return await _database.KeyExistsAsync(fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查锁存在性时发生异常: {LockKey}", lockKey);
                return false;
            }
        }

        public async Task<TimeSpan?> GetLockRemainingTimeAsync(string lockKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = GetFullKey(lockKey);
                var ttl = await _database.KeyTimeToLiveAsync(fullKey);
                return ttl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取锁剩余时间时发生异常: {LockKey}", lockKey);
                return null;
            }
        }

        public async Task<T> ExecuteWithLockAsync<T>(string lockKey, Func<CancellationToken, Task<T>> action, TimeSpan expirationTime, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            await using var distributedLock = await AcquireLockAsync(lockKey, expirationTime, timeout, cancellationToken);
            return await action(cancellationToken);
        }

        public async Task ExecuteWithLockAsync(string lockKey, Func<CancellationToken, Task> action, TimeSpan expirationTime, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            await using var distributedLock = await AcquireLockAsync(lockKey, expirationTime, timeout, cancellationToken);
            await action(cancellationToken);
        }

        private string GetFullKey(string lockKey)
        {
            return $"{_settings.KeyPrefix}{lockKey}";
        }

        private string GenerateOwnerIdentifier()
        {
            return _settings.OwnerIdentifierStrategy.ToLower() switch
            {
                "guid" => Guid.NewGuid().ToString(),
                "machine" => Environment.MachineName,
                "custom" => _settings.CustomOwnerIdentifier ?? Guid.NewGuid().ToString(),
                _ => Guid.NewGuid().ToString()
            };
        }

        private string GenerateLockValue()
        {
            return $"{_ownerIdentifier}:{Guid.NewGuid()}:{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }
    }

    /// <summary>
    /// Redis分布式锁实现
    /// </summary>
    internal class RedisDistributedLock : IDistributedLock
    {
        private readonly RedisDistributedLockService _lockService;
        private bool _disposed = false;

        public string LockKey { get; }
        public string LockValue { get; }
        public DateTime ExpirationTime { get; private set; }
        public bool IsValid => !_disposed && DateTime.UtcNow < ExpirationTime;

        public RedisDistributedLock(RedisDistributedLockService lockService, string lockKey, string lockValue, DateTime expirationTime)
        {
            _lockService = lockService;
            LockKey = lockKey;
            LockValue = lockValue;
            ExpirationTime = expirationTime;
        }

        public async Task<bool> RenewAsync(TimeSpan expirationTime, CancellationToken cancellationToken = default)
        {
            if (_disposed)
                return false;

            var renewed = await _lockService.RenewLockAsync(LockKey, LockValue, expirationTime, cancellationToken);
            if (renewed)
            {
                ExpirationTime = DateTime.UtcNow.Add(expirationTime);
            }
            return renewed;
        }

        public async Task<bool> ReleaseAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
                return false;

            var released = await _lockService.ReleaseLockAsync(LockKey, LockValue, cancellationToken);
            if (released)
            {
                _disposed = true;
            }
            return released;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await ReleaseAsync();
            }
        }
    }
}