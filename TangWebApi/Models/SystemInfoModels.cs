using System.ComponentModel.DataAnnotations;

namespace TangWebApi.Models
{
    /// <summary>
    /// 系统信息响应模型
    /// </summary>
    public class SystemInfoResponse
    {
        /// <summary>
        /// 系统基本信息
        /// </summary>
        public SystemBasicInfo BasicInfo { get; set; } = new();

        /// <summary>
        /// 硬件信息
        /// </summary>
        public HardwareInfo Hardware { get; set; } = new();

        /// <summary>
        /// 应用程序信息
        /// </summary>
        public ApplicationInfo Application { get; set; } = new();

        /// <summary>
        /// 性能信息
        /// </summary>
        public PerformanceInfo Performance { get; set; } = new();
    }

    /// <summary>
    /// 系统基本信息
    /// </summary>
    public class SystemBasicInfo
    {
        /// <summary>
        /// 操作系统名称
        /// </summary>
        public string OperatingSystem { get; set; } = string.Empty;

        /// <summary>
        /// 操作系统版本
        /// </summary>
        public string OSVersion { get; set; } = string.Empty;

        /// <summary>
        /// 机器名称
        /// </summary>
        public string MachineName { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 系统架构
        /// </summary>
        public string Architecture { get; set; } = string.Empty;

        /// <summary>
        /// 系统启动时间
        /// </summary>
        public DateTime SystemStartTime { get; set; }

        /// <summary>
        /// 系统运行时间
        /// </summary>
        public TimeSpan Uptime { get; set; }
    }

    /// <summary>
    /// 硬件信息
    /// </summary>
    public class HardwareInfo
    {
        /// <summary>
        /// 处理器信息
        /// </summary>
        public ProcessorInfo Processor { get; set; } = new();

        /// <summary>
        /// 内存信息
        /// </summary>
        public MemoryInfo Memory { get; set; } = new();

        /// <summary>
        /// 磁盘信息
        /// </summary>
        public List<DiskInfo> Disks { get; set; } = new();
    }

    /// <summary>
    /// 处理器信息
    /// </summary>
    public class ProcessorInfo
    {
        /// <summary>
        /// 处理器名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 核心数
        /// </summary>
        public int CoreCount { get; set; }

        /// <summary>
        /// 逻辑处理器数
        /// </summary>
        public int LogicalProcessorCount { get; set; }

        /// <summary>
        /// 当前使用率 (%)
        /// </summary>
        public double UsagePercentage { get; set; }
    }

    /// <summary>
    /// 内存信息
    /// </summary>
    public class MemoryInfo
    {
        /// <summary>
        /// 总内存 (字节)
        /// </summary>
        public long TotalMemory { get; set; }

        /// <summary>
        /// 可用内存 (字节)
        /// </summary>
        public long AvailableMemory { get; set; }

        /// <summary>
        /// 已使用内存 (字节)
        /// </summary>
        public long UsedMemory { get; set; }

        /// <summary>
        /// 内存使用率 (%)
        /// </summary>
        public double UsagePercentage { get; set; }

        /// <summary>
        /// 总内存 (格式化字符串)
        /// </summary>
        public string TotalMemoryFormatted { get; set; } = string.Empty;

        /// <summary>
        /// 可用内存 (格式化字符串)
        /// </summary>
        public string AvailableMemoryFormatted { get; set; } = string.Empty;

        /// <summary>
        /// 已使用内存 (格式化字符串)
        /// </summary>
        public string UsedMemoryFormatted { get; set; } = string.Empty;
    }

    /// <summary>
    /// 磁盘信息
    /// </summary>
    public class DiskInfo
    {
        /// <summary>
        /// 驱动器名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 文件系统类型
        /// </summary>
        public string FileSystem { get; set; } = string.Empty;

        /// <summary>
        /// 总空间 (字节)
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// 可用空间 (字节)
        /// </summary>
        public long AvailableSpace { get; set; }

        /// <summary>
        /// 已使用空间 (字节)
        /// </summary>
        public long UsedSpace { get; set; }

        /// <summary>
        /// 使用率 (%)
        /// </summary>
        public double UsagePercentage { get; set; }

        /// <summary>
        /// 总空间 (格式化字符串)
        /// </summary>
        public string TotalSizeFormatted { get; set; } = string.Empty;

        /// <summary>
        /// 可用空间 (格式化字符串)
        /// </summary>
        public string AvailableSpaceFormatted { get; set; } = string.Empty;

        /// <summary>
        /// 已使用空间 (格式化字符串)
        /// </summary>
        public string UsedSpaceFormatted { get; set; } = string.Empty;
    }

    /// <summary>
    /// 应用程序信息
    /// </summary>
    public class ApplicationInfo
    {
        /// <summary>
        /// 应用程序名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 应用程序版本
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// .NET 版本
        /// </summary>
        public string DotNetVersion { get; set; } = string.Empty;

        /// <summary>
        /// 应用程序启动时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 应用程序运行时间
        /// </summary>
        public TimeSpan Uptime { get; set; }

        /// <summary>
        /// 工作目录
        /// </summary>
        public string WorkingDirectory { get; set; } = string.Empty;

        /// <summary>
        /// 进程ID
        /// </summary>
        public int ProcessId { get; set; }
    }

    /// <summary>
    /// 性能信息
    /// </summary>
    public class PerformanceInfo
    {
        /// <summary>
        /// GC信息
        /// </summary>
        public GCInfo GarbageCollection { get; set; } = new();

        /// <summary>
        /// 线程信息
        /// </summary>
        public ThreadInfo Threads { get; set; } = new();

        /// <summary>
        /// 应用程序内存使用
        /// </summary>
        public ApplicationMemoryInfo ApplicationMemory { get; set; } = new();
    }

    /// <summary>
    /// 垃圾回收信息
    /// </summary>
    public class GCInfo
    {
        /// <summary>
        /// Gen0 回收次数
        /// </summary>
        public int Gen0Collections { get; set; }

        /// <summary>
        /// Gen1 回收次数
        /// </summary>
        public int Gen1Collections { get; set; }

        /// <summary>
        /// Gen2 回收次数
        /// </summary>
        public int Gen2Collections { get; set; }

        /// <summary>
        /// 总分配内存 (字节)
        /// </summary>
        public long TotalAllocatedBytes { get; set; }

        /// <summary>
        /// 总分配内存 (格式化字符串)
        /// </summary>
        public string TotalAllocatedBytesFormatted { get; set; } = string.Empty;
    }

    /// <summary>
    /// 线程信息
    /// </summary>
    public class ThreadInfo
    {
        /// <summary>
        /// 线程池线程数
        /// </summary>
        public int ThreadPoolThreads { get; set; }

        /// <summary>
        /// 工作线程数
        /// </summary>
        public int WorkerThreads { get; set; }

        /// <summary>
        /// 完成端口线程数
        /// </summary>
        public int CompletionPortThreads { get; set; }
    }

    /// <summary>
    /// 应用程序内存信息
    /// </summary>
    public class ApplicationMemoryInfo
    {
        /// <summary>
        /// 工作集 (字节)
        /// </summary>
        public long WorkingSet { get; set; }

        /// <summary>
        /// 私有内存 (字节)
        /// </summary>
        public long PrivateMemory { get; set; }

        /// <summary>
        /// 托管内存 (字节)
        /// </summary>
        public long ManagedMemory { get; set; }

        /// <summary>
        /// 工作集 (格式化字符串)
        /// </summary>
        public string WorkingSetFormatted { get; set; } = string.Empty;

        /// <summary>
        /// 私有内存 (格式化字符串)
        /// </summary>
        public string PrivateMemoryFormatted { get; set; } = string.Empty;

        /// <summary>
        /// 托管内存 (格式化字符串)
        /// </summary>
        public string ManagedMemoryFormatted { get; set; } = string.Empty;
    }

    /// <summary>
    /// 系统信息查询参数
    /// </summary>
    public class SystemInfoRequest
    {
        /// <summary>
        /// 是否包含硬件信息
        /// </summary>
        public bool IncludeHardware { get; set; } = true;

        /// <summary>
        /// 是否包含性能信息
        /// </summary>
        public bool IncludePerformance { get; set; } = true;

        /// <summary>
        /// 是否包含磁盘信息
        /// </summary>
        public bool IncludeDiskInfo { get; set; } = true;
    }
}