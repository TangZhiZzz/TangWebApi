using Microsoft.AspNetCore.Mvc;
using TangWebApi.Filter;
using TangWebApi.Models;
using TangWebApi.Services;

namespace TangWebApi.Controllers
{
    /// <summary>
    /// 系统信息控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ApiResponseFilter))]
    public class SystemInfoController : ControllerBase
    {
        private readonly ISystemInfoService _systemInfoService;
        private readonly ILogger<SystemInfoController> _logger;

        public SystemInfoController(
            ISystemInfoService systemInfoService,
            ILogger<SystemInfoController> logger)
        {
            _systemInfoService = systemInfoService;
            _logger = logger;
        }

        /// <summary>
        /// 获取完整的系统信息
        /// </summary>
        /// <param name="request">查询参数</param>
        /// <returns>系统信息</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TangWebApi.Models.SystemInfoResponse>>> GetSystemInfo([FromBody] SystemInfoRequest? request = null)
        {
            try
            {
                _logger.LogInformation("开始获取系统信息");
                
                var systemInfo = await _systemInfoService.GetSystemInfoAsync(request);
                
                _logger.LogInformation("成功获取系统信息");
                return Ok(new ApiResponse<TangWebApi.Models.SystemInfoResponse>
                {
                    Success = true,
                    Message = "获取系统信息成功",
                    Data = systemInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取系统信息时发生错误");
                return StatusCode(500, new ApiResponse<TangWebApi.Models.SystemInfoResponse>
                {
                    Success = false,
                    Message = $"获取系统信息失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取系统基本信息
        /// </summary>
        /// <returns>系统基本信息</returns>
        [HttpGet("basic")]
        public async Task<ActionResult<ApiResponse<SystemBasicInfo>>> GetBasicInfo()
        {
            try
            {
                _logger.LogInformation("开始获取系统基本信息");
                
                var basicInfo = await _systemInfoService.GetBasicInfoAsync();
                
                return Ok(new ApiResponse<SystemBasicInfo>
                {
                    Success = true,
                    Message = "获取系统基本信息成功",
                    Data = basicInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取系统基本信息时发生错误");
                return StatusCode(500, new ApiResponse<SystemBasicInfo>
                {
                    Success = false,
                    Message = $"获取系统基本信息失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取硬件信息
        /// </summary>
        /// <returns>硬件信息</returns>
        [HttpGet("hardware")]
        public async Task<ActionResult<ApiResponse<HardwareInfo>>> GetHardwareInfo()
        {
            try
            {
                _logger.LogInformation("开始获取硬件信息");
                
                var hardwareInfo = await _systemInfoService.GetHardwareInfoAsync();
                
                return Ok(new ApiResponse<HardwareInfo>
                {
                    Success = true,
                    Message = "获取硬件信息成功",
                    Data = hardwareInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取硬件信息时发生错误");
                return StatusCode(500, new ApiResponse<HardwareInfo>
                {
                    Success = false,
                    Message = $"获取硬件信息失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取应用程序信息
        /// </summary>
        /// <returns>应用程序信息</returns>
        [HttpGet("application")]
        public async Task<ActionResult<ApiResponse<ApplicationInfo>>> GetApplicationInfo()
        {
            try
            {
                _logger.LogInformation("开始获取应用程序信息");
                
                var appInfo = await _systemInfoService.GetApplicationInfoAsync();
                
                return Ok(new ApiResponse<ApplicationInfo>
                {
                    Success = true,
                    Message = "获取应用程序信息成功",
                    Data = appInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取应用程序信息时发生错误");
                return StatusCode(500, new ApiResponse<ApplicationInfo>
                {
                    Success = false,
                    Message = $"获取应用程序信息失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取性能信息
        /// </summary>
        /// <returns>性能信息</returns>
        [HttpGet("performance")]
        public async Task<ActionResult<ApiResponse<PerformanceInfo>>> GetPerformanceInfo()
        {
            try
            {
                _logger.LogInformation("开始获取性能信息");
                
                var performanceInfo = await _systemInfoService.GetPerformanceInfoAsync();
                
                return Ok(new ApiResponse<PerformanceInfo>
                {
                    Success = true,
                    Message = "获取性能信息成功",
                    Data = performanceInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取性能信息时发生错误");
                return StatusCode(500, new ApiResponse<PerformanceInfo>
                {
                    Success = false,
                    Message = $"获取性能信息失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取内存信息
        /// </summary>
        /// <returns>内存信息</returns>
        [HttpGet("memory")]
        public async Task<ActionResult<ApiResponse<MemoryInfo>>> GetMemoryInfo()
        {
            try
            {
                _logger.LogInformation("开始获取内存信息");
                
                var memoryInfo = await _systemInfoService.GetMemoryInfoAsync();
                
                return Ok(new ApiResponse<MemoryInfo>
                {
                    Success = true,
                    Message = "获取内存信息成功",
                    Data = memoryInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取内存信息时发生错误");
                return StatusCode(500, new ApiResponse<MemoryInfo>
                {
                    Success = false,
                    Message = $"获取内存信息失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取磁盘信息
        /// </summary>
        /// <returns>磁盘信息列表</returns>
        [HttpGet("disks")]
        public async Task<ActionResult<ApiResponse<List<DiskInfo>>>> GetDiskInfo()
        {
            try
            {
                _logger.LogInformation("开始获取磁盘信息");
                
                var diskInfo = await _systemInfoService.GetDiskInfoAsync();
                
                return Ok(new ApiResponse<List<DiskInfo>>
                {
                    Success = true,
                    Message = "获取磁盘信息成功",
                    Data = diskInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取磁盘信息时发生错误");
                return StatusCode(500, new ApiResponse<List<DiskInfo>>
                {
                    Success = false,
                    Message = $"获取磁盘信息失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取处理器信息
        /// </summary>
        /// <returns>处理器信息</returns>
        [HttpGet("processor")]
        public async Task<ActionResult<ApiResponse<ProcessorInfo>>> GetProcessorInfo()
        {
            try
            {
                _logger.LogInformation("开始获取处理器信息");
                
                var processorInfo = await _systemInfoService.GetProcessorInfoAsync();
                
                return Ok(new ApiResponse<ProcessorInfo>
                {
                    Success = true,
                    Message = "获取处理器信息成功",
                    Data = processorInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取处理器信息时发生错误");
                return StatusCode(500, new ApiResponse<ProcessorInfo>
                {
                    Success = false,
                    Message = $"获取处理器信息失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取系统健康状态摘要
        /// </summary>
        /// <returns>系统健康状态</returns>
        [HttpGet("health")]
        public async Task<ActionResult<ApiResponse<object>>> GetSystemHealth()
        {
            try
            {
                _logger.LogInformation("开始获取系统健康状态");
                
                var basicInfo = await _systemInfoService.GetBasicInfoAsync();
                var memoryInfo = await _systemInfoService.GetMemoryInfoAsync();
                var performanceInfo = await _systemInfoService.GetPerformanceInfoAsync();
                var diskInfo = await _systemInfoService.GetDiskInfoAsync();

                var health = new
                {
                    Status = "Healthy",
                    Uptime = basicInfo.Uptime,
                    MemoryUsage = memoryInfo.UsagePercentage,
                    DiskUsage = diskInfo.Any() ? diskInfo.Average(d => d.UsagePercentage) : 0,
                    ManagedMemory = performanceInfo.ApplicationMemory.ManagedMemoryFormatted,
                    ThreadCount = performanceInfo.Threads.ThreadPoolThreads,
                    GCCollections = new
                    {
                        Gen0 = performanceInfo.GarbageCollection.Gen0Collections,
                        Gen1 = performanceInfo.GarbageCollection.Gen1Collections,
                        Gen2 = performanceInfo.GarbageCollection.Gen2Collections
                    },
                    Timestamp = DateTime.Now
                };
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "获取系统健康状态成功",
                    Data = health
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取系统健康状态时发生错误");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"获取系统健康状态失败: {ex.Message}"
                });
            }
        }
    }
}