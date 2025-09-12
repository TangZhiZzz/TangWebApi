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
        [HttpPost]
        public async Task<TangWebApi.Models.SystemInfoResponse> GetSystemInfo([FromBody] SystemInfoRequest? request = null)
        {
            _logger.LogInformation("开始获取系统信息");
            
            var systemInfo = await _systemInfoService.GetSystemInfoAsync(request);
            
            _logger.LogInformation("成功获取系统信息");
            return systemInfo;
        }

        /// <summary>
        /// 获取系统基本信息
        /// </summary>
        [HttpGet("basic")]
        public async Task<SystemBasicInfo> GetBasicInfo()
        {
            _logger.LogInformation("开始获取系统基本信息");
            
            return await _systemInfoService.GetBasicInfoAsync();
        }

        /// <summary>
        /// 获取硬件信息
        /// </summary>
        [HttpGet("hardware")]
        public async Task<HardwareInfo> GetHardwareInfo()
        {
            _logger.LogInformation("开始获取硬件信息");
            
            return await _systemInfoService.GetHardwareInfoAsync();
        }

        /// <summary>
        /// 获取应用程序信息
        /// </summary>
        [HttpGet("application")]
        public async Task<ApplicationInfo> GetApplicationInfo()
        {
            _logger.LogInformation("开始获取应用程序信息");
            
            return await _systemInfoService.GetApplicationInfoAsync();
        }

        /// <summary>
        /// 获取性能信息
        /// </summary>
        [HttpGet("performance")]
        public async Task<PerformanceInfo> GetPerformanceInfo()
        {
            _logger.LogInformation("开始获取性能信息");
            
            return await _systemInfoService.GetPerformanceInfoAsync();
        }

        /// <summary>
        /// 获取内存信息
        /// </summary>
        [HttpGet("memory")]
        public async Task<MemoryInfo> GetMemoryInfo()
        {
            _logger.LogInformation("开始获取内存信息");
            
            return await _systemInfoService.GetMemoryInfoAsync();
        }

        /// <summary>
        /// 获取磁盘信息
        /// </summary>
        [HttpGet("disks")]
        public async Task<List<DiskInfo>> GetDiskInfo()
        {
            _logger.LogInformation("开始获取磁盘信息");
            
            return await _systemInfoService.GetDiskInfoAsync();
        }

        /// <summary>
        /// 获取处理器信息
        /// </summary>
        [HttpGet("processor")]
        public async Task<ProcessorInfo> GetProcessorInfo()
        {
            _logger.LogInformation("开始获取处理器信息");
            
            return await _systemInfoService.GetProcessorInfoAsync();
        }

        /// <summary>
        /// 获取系统健康状态摘要
        /// </summary>
        [HttpGet("health")]
        public async Task<object> GetSystemHealth()
        {
            _logger.LogInformation("开始获取系统健康状态");
            
            var basicInfo = await _systemInfoService.GetBasicInfoAsync();
            var memoryInfo = await _systemInfoService.GetMemoryInfoAsync();
            var performanceInfo = await _systemInfoService.GetPerformanceInfoAsync();
            var diskInfo = await _systemInfoService.GetDiskInfoAsync();

            return new
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
        }
    }
}