namespace TangWebApi.Services
{
    /// <summary>
    /// 分布式锁服务接口
    /// </summary>
    public interface IDistributedLockService
    {
        /// <summary>
        /// 尝试获取锁
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="expirationTime">锁的过期时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>锁对象，如果获取失败返回null</returns>
        Task<IDistributedLock?> TryAcquireLockAsync(string lockKey, TimeSpan expirationTime, CancellationToken cancellationToken = default);

        /// <summary>
        /// 尝试获取锁（使用默认过期时间）
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>锁对象，如果获取失败返回null</returns>
        Task<IDistributedLock?> TryAcquireLockAsync(string lockKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取锁（带重试机制）
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="expirationTime">锁的过期时间</param>
        /// <param name="timeout">获取锁的超时时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>锁对象</returns>
        /// <exception cref="TimeoutException">获取锁超时</exception>
        Task<IDistributedLock> AcquireLockAsync(string lockKey, TimeSpan expirationTime, TimeSpan timeout, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取锁（带重试机制，使用默认过期时间）
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="timeout">获取锁的超时时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>锁对象</returns>
        /// <exception cref="TimeoutException">获取锁超时</exception>
        Task<IDistributedLock> AcquireLockAsync(string lockKey, TimeSpan timeout, CancellationToken cancellationToken = default);

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="lockValue">锁的值（用于验证锁的所有权）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否成功释放</returns>
        Task<bool> ReleaseLockAsync(string lockKey, string lockValue, CancellationToken cancellationToken = default);

        /// <summary>
        /// 续期锁
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="lockValue">锁的值（用于验证锁的所有权）</param>
        /// <param name="expirationTime">新的过期时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否成功续期</returns>
        Task<bool> RenewLockAsync(string lockKey, string lockValue, TimeSpan expirationTime, CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查锁是否存在
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>锁是否存在</returns>
        Task<bool> IsLockExistsAsync(string lockKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取锁的剩余时间
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>剩余时间，如果锁不存在返回null</returns>
        Task<TimeSpan?> GetLockRemainingTimeAsync(string lockKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行带锁的操作
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="action">要执行的操作</param>
        /// <param name="expirationTime">锁的过期时间</param>
        /// <param name="timeout">获取锁的超时时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>操作结果</returns>
        Task<T> ExecuteWithLockAsync<T>(string lockKey, Func<CancellationToken, Task<T>> action, TimeSpan expirationTime, TimeSpan timeout, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行带锁的操作（无返回值）
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="action">要执行的操作</param>
        /// <param name="expirationTime">锁的过期时间</param>
        /// <param name="timeout">获取锁的超时时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task ExecuteWithLockAsync(string lockKey, Func<CancellationToken, Task> action, TimeSpan expirationTime, TimeSpan timeout, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 分布式锁接口
    /// </summary>
    public interface IDistributedLock : IAsyncDisposable
    {
        /// <summary>
        /// 锁的键
        /// </summary>
        string LockKey { get; }

        /// <summary>
        /// 锁的值（用于验证锁的所有权）
        /// </summary>
        string LockValue { get; }

        /// <summary>
        /// 锁的过期时间
        /// </summary>
        DateTime ExpirationTime { get; }

        /// <summary>
        /// 锁是否仍然有效
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// 续期锁
        /// </summary>
        /// <param name="expirationTime">新的过期时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否成功续期</returns>
        Task<bool> RenewAsync(TimeSpan expirationTime, CancellationToken cancellationToken = default);

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否成功释放</returns>
        Task<bool> ReleaseAsync(CancellationToken cancellationToken = default);
    }
}