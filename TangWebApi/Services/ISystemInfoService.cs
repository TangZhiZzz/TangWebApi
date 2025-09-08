using TangWebApi.Models;

namespace TangWebApi.Services
{
    /// <summary>
    /// 系统信息服务接口
    /// </summary>
    public interface ISystemInfoService
    {
        /// <summary>
        /// 获取完整的系统信息
        /// </summary>
        /// <param name="request">查询参数</param>
        /// <returns>系统信息</returns>
        Task<SystemInfoResponse> GetSystemInfoAsync(SystemInfoRequest? request = null);

        /// <summary>
        /// 获取系统基本信息
        /// </summary>
        /// <returns>系统基本信息</returns>
        Task<SystemBasicInfo> GetBasicInfoAsync();

        /// <summary>
        /// 获取硬件信息
        /// </summary>
        /// <returns>硬件信息</returns>
        Task<HardwareInfo> GetHardwareInfoAsync();

        /// <summary>
        /// 获取应用程序信息
        /// </summary>
        /// <returns>应用程序信息</returns>
        Task<ApplicationInfo> GetApplicationInfoAsync();

        /// <summary>
        /// 获取性能信息
        /// </summary>
        /// <returns>性能信息</returns>
        Task<PerformanceInfo> GetPerformanceInfoAsync();

        /// <summary>
        /// 获取内存信息
        /// </summary>
        /// <returns>内存信息</returns>
        Task<MemoryInfo> GetMemoryInfoAsync();

        /// <summary>
        /// 获取磁盘信息
        /// </summary>
        /// <returns>磁盘信息列表</returns>
        Task<List<DiskInfo>> GetDiskInfoAsync();

        /// <summary>
        /// 获取处理器信息
        /// </summary>
        /// <returns>处理器信息</returns>
        Task<ProcessorInfo> GetProcessorInfoAsync();
    }
}