using Microsoft.AspNetCore.Mvc;
using TangWebApi.Services;
using TangWebApi.Models;

namespace TangWebApi.Controllers.Demo
{
    /// <summary>
    /// 分布式锁测试控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("分布式锁测试")]
    public class DistributedLockController : ControllerBase
    {
        private readonly IDistributedLockService _distributedLockService;
        private readonly ILogger<DistributedLockController> _logger;

        public DistributedLockController(
            IDistributedLockService distributedLockService,
            ILogger<DistributedLockController> logger)
        {
            _distributedLockService = distributedLockService;
            _logger = logger;
        }

        /// <summary>
        /// 测试获取锁
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="expirationSeconds">过期时间（秒）</param>
        /// <returns></returns>
        [HttpPost("acquire")]
        public async Task<IActionResult> AcquireLock([FromQuery] string lockKey = "test-lock", [FromQuery] int expirationSeconds = 30)
        {
            try
            {
                var lockResult = await _distributedLockService.TryAcquireLockAsync(
                    lockKey, 
                    TimeSpan.FromSeconds(expirationSeconds));

                if (lockResult != null)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "成功获取锁",
                        Data = new
                        {
                            LockKey = lockResult.LockKey,
                            LockValue = lockResult.LockValue,
                            ExpirationTime = lockResult.ExpirationTime,
                            IsValid = lockResult.IsValid
                        }
                    });
                }
                else
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = "获取锁失败，锁可能已被其他进程持有",
                        Data = (object?)null
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取锁时发生异常: {LockKey}", lockKey);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"获取锁时发生异常: {ex.Message}",
                    Data = (object?)null
                });
            }
        }

        /// <summary>
        /// 测试释放锁
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="lockValue">锁的值</param>
        /// <returns></returns>
        [HttpPost("release")]
        public async Task<IActionResult> ReleaseLock([FromQuery] string lockKey, [FromQuery] string lockValue)
        {
            try
            {
                var released = await _distributedLockService.ReleaseLockAsync(lockKey, lockValue);

                return Ok(new
                {
                    Success = released,
                    Message = released ? "成功释放锁" : "释放锁失败，锁可能已过期或被其他进程持有",
                    Data = new { LockKey = lockKey, LockValue = lockValue, Released = released }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放锁时发生异常: {LockKey}", lockKey);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"释放锁时发生异常: {ex.Message}",
                    Data = (object?)null
                });
            }
        }

        /// <summary>
        /// 测试续期锁
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="lockValue">锁的值</param>
        /// <param name="expirationSeconds">新的过期时间（秒）</param>
        /// <returns></returns>
        [HttpPost("renew")]
        public async Task<IActionResult> RenewLock([FromQuery] string lockKey, [FromQuery] string lockValue, [FromQuery] int expirationSeconds = 30)
        {
            try
            {
                var renewed = await _distributedLockService.RenewLockAsync(
                    lockKey, 
                    lockValue, 
                    TimeSpan.FromSeconds(expirationSeconds));

                return Ok(new
                {
                    Success = renewed,
                    Message = renewed ? "成功续期锁" : "续期锁失败，锁可能已过期或被其他进程持有",
                    Data = new 
                    { 
                        LockKey = lockKey, 
                        LockValue = lockValue, 
                        Renewed = renewed,
                        NewExpirationSeconds = expirationSeconds
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "续期锁时发生异常: {LockKey}", lockKey);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"续期锁时发生异常: {ex.Message}",
                    Data = (object?)null
                });
            }
        }

        /// <summary>
        /// 检查锁是否存在
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <returns></returns>
        [HttpGet("exists")]
        public async Task<IActionResult> CheckLockExists([FromQuery] string lockKey = "test-lock")
        {
            try
            {
                var exists = await _distributedLockService.IsLockExistsAsync(lockKey);
                var remainingTime = await _distributedLockService.GetLockRemainingTimeAsync(lockKey);

                return Ok(new
                {
                    Success = true,
                    Message = "检查锁状态成功",
                    Data = new
                    {
                        LockKey = lockKey,
                        Exists = exists,
                        RemainingTime = remainingTime?.ToString() ?? "N/A"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查锁状态时发生异常: {LockKey}", lockKey);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"检查锁状态时发生异常: {ex.Message}",
                    Data = (object?)null
                });
            }
        }

        /// <summary>
        /// 测试带锁执行操作
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="operationDurationSeconds">操作持续时间（秒）</param>
        /// <param name="lockExpirationSeconds">锁过期时间（秒）</param>
        /// <param name="timeoutSeconds">获取锁超时时间（秒）</param>
        /// <returns></returns>
        [HttpPost("execute-with-lock")]
        public async Task<IActionResult> ExecuteWithLock(
            [FromQuery] string lockKey = "test-operation",
            [FromQuery] int operationDurationSeconds = 5,
            [FromQuery] int lockExpirationSeconds = 30,
            [FromQuery] int timeoutSeconds = 10)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                
                var result = await _distributedLockService.ExecuteWithLockAsync(
                    lockKey,
                    async (cancellationToken) =>
                    {
                        _logger.LogInformation("开始执行受保护的操作: {LockKey}", lockKey);
                        
                        // 模拟一些工作
                        await Task.Delay(TimeSpan.FromSeconds(operationDurationSeconds), cancellationToken);
                        
                        _logger.LogInformation("完成执行受保护的操作: {LockKey}", lockKey);
                        
                        return new
                        {
                            OperationId = Guid.NewGuid(),
                            StartTime = startTime,
                            EndTime = DateTime.UtcNow,
                            Duration = DateTime.UtcNow - startTime,
                            Message = "操作执行成功"
                        };
                    },
                    TimeSpan.FromSeconds(lockExpirationSeconds),
                    TimeSpan.FromSeconds(timeoutSeconds));

                return Ok(new
                {
                    Success = true,
                    Message = "带锁操作执行成功",
                    Data = result
                });
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex, "获取锁超时: {LockKey}", lockKey);
                return Ok(new
                {
                    Success = false,
                    Message = $"获取锁超时: {ex.Message}",
                    Data = (object?)null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "带锁操作执行时发生异常: {LockKey}", lockKey);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"带锁操作执行时发生异常: {ex.Message}",
                    Data = (object?)null
                });
            }
        }

        /// <summary>
        /// 测试并发锁竞争
        /// </summary>
        /// <param name="lockKey">锁的键</param>
        /// <param name="concurrentCount">并发数量</param>
        /// <param name="operationDurationSeconds">每个操作持续时间（秒）</param>
        /// <returns></returns>
        [HttpPost("concurrent-test")]
        public async Task<IActionResult> ConcurrentLockTest(
            [FromQuery] string lockKey = "concurrent-test",
            [FromQuery] int concurrentCount = 5,
            [FromQuery] int operationDurationSeconds = 2)
        {
            try
            {
                var tasks = new List<Task<object>>();
                var startTime = DateTime.UtcNow;

                for (int i = 0; i < concurrentCount; i++)
                {
                    var taskId = i + 1;
                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            var taskStartTime = DateTime.UtcNow;
                            
                            var lockResult = await _distributedLockService.TryAcquireLockAsync(
                                lockKey, 
                                TimeSpan.FromSeconds(operationDurationSeconds + 5));

                            if (lockResult != null)
                            {
                                using (lockResult)
                                {
                                    _logger.LogInformation("任务 {TaskId} 获取到锁，开始执行", taskId);
                                    
                                    await Task.Delay(TimeSpan.FromSeconds(operationDurationSeconds));
                                    
                                    _logger.LogInformation("任务 {TaskId} 执行完成", taskId);
                                    
                                    return new
                                    {
                                        TaskId = taskId,
                                        Success = true,
                                        Message = "任务执行成功",
                                        StartTime = taskStartTime,
                                        EndTime = DateTime.UtcNow,
                                        Duration = DateTime.UtcNow - taskStartTime,
                                        LockAcquired = true
                                    };
                                }
                            }
                            else
                            {
                                _logger.LogInformation("任务 {TaskId} 未能获取到锁", taskId);
                                
                                return new
                                {
                                    TaskId = taskId,
                                    Success = false,
                                    Message = "未能获取到锁",
                                    StartTime = taskStartTime,
                                    EndTime = DateTime.UtcNow,
                                    Duration = DateTime.UtcNow - taskStartTime,
                                    LockAcquired = false
                                };
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "任务 {TaskId} 执行时发生异常", taskId);
                            
                            return new
                            {
                                TaskId = taskId,
                                Success = false,
                                Message = $"任务执行异常: {ex.Message}",
                                StartTime = DateTime.UtcNow,
                                EndTime = DateTime.UtcNow,
                                Duration = TimeSpan.Zero,
                                LockAcquired = false
                            };
                        }
                    });
                    
                    tasks.Add(task);
                }

                var results = await Task.WhenAll(tasks);
                var endTime = DateTime.UtcNow;

                var summary = new
                {
                    TotalTasks = concurrentCount,
                    SuccessfulTasks = results.Count(r => ((dynamic)r).Success),
                    FailedTasks = results.Count(r => !((dynamic)r).Success),
                    TasksWithLock = results.Count(r => ((dynamic)r).LockAcquired),
                    TasksWithoutLock = results.Count(r => !((dynamic)r).LockAcquired),
                    TotalDuration = endTime - startTime,
                    StartTime = startTime,
                    EndTime = endTime
                };

                return Ok(new
                {
                    Success = true,
                    Message = "并发锁测试完成",
                    Data = new
                    {
                        Summary = summary,
                        TaskResults = results
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "并发锁测试时发生异常");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"并发锁测试时发生异常: {ex.Message}",
                    Data = (object?)null
                });
            }
        }
    }
}