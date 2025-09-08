using System.Diagnostics;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using TangWebApi.Models;

namespace TangWebApi.Services
{
    /// <summary>
    /// 系统信息服务实现
    /// </summary>
    public class SystemInfoService : ISystemInfoService
    {
        private readonly ILogger<SystemInfoService> _logger;
        private static readonly DateTime _applicationStartTime = DateTime.Now;

        public SystemInfoService(ILogger<SystemInfoService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取完整的系统信息
        /// </summary>
        public async Task<SystemInfoResponse> GetSystemInfoAsync(SystemInfoRequest? request = null)
        {
            request ??= new SystemInfoRequest();

            var response = new SystemInfoResponse
            {
                BasicInfo = await GetBasicInfoAsync(),
                Application = await GetApplicationInfoAsync()
            };

            if (request.IncludeHardware)
            {
                response.Hardware = await GetHardwareInfoAsync();
            }

            if (request.IncludePerformance)
            {
                response.Performance = await GetPerformanceInfoAsync();
            }

            return response;
        }

        /// <summary>
        /// 获取系统基本信息
        /// </summary>
        public async Task<SystemBasicInfo> GetBasicInfoAsync()
        {
            return await Task.Run(() =>
            {
                var bootTime = DateTime.Now - TimeSpan.FromMilliseconds(Environment.TickCount64);
                
                return new SystemBasicInfo
                {
                    OperatingSystem = RuntimeInformation.OSDescription,
                    OSVersion = Environment.OSVersion.ToString(),
                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName,
                    Architecture = RuntimeInformation.OSArchitecture.ToString(),
                    SystemStartTime = bootTime,
                    Uptime = DateTime.Now - bootTime
                };
            });
        }

        /// <summary>
        /// 获取硬件信息
        /// </summary>
        public async Task<HardwareInfo> GetHardwareInfoAsync()
        {
            var hardware = new HardwareInfo
            {
                Processor = await GetProcessorInfoAsync(),
                Memory = await GetMemoryInfoAsync()
            };

            // 只在请求包含磁盘信息时获取
            hardware.Disks = await GetDiskInfoAsync();

            return hardware;
        }

        /// <summary>
        /// 获取应用程序信息
        /// </summary>
        public async Task<ApplicationInfo> GetApplicationInfoAsync()
        {
            return await Task.Run(() =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                var process = Process.GetCurrentProcess();

                return new ApplicationInfo
                {
                    Name = assembly.GetName().Name ?? "TangWebApi",
                    Version = assembly.GetName().Version?.ToString() ?? "1.0.0",
                    DotNetVersion = RuntimeInformation.FrameworkDescription,
                    StartTime = _applicationStartTime,
                    Uptime = DateTime.Now - _applicationStartTime,
                    WorkingDirectory = Environment.CurrentDirectory,
                    ProcessId = process.Id
                };
            });
        }

        /// <summary>
        /// 获取性能信息
        /// </summary>
        public async Task<PerformanceInfo> GetPerformanceInfoAsync()
        {
            return await Task.Run(() =>
            {
                var process = Process.GetCurrentProcess();
                
                // 获取线程池信息
                ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
                ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
                
                return new PerformanceInfo
                {
                    GarbageCollection = new GCInfo
                    {
                        Gen0Collections = GC.CollectionCount(0),
                        Gen1Collections = GC.CollectionCount(1),
                        Gen2Collections = GC.CollectionCount(2),
                        TotalAllocatedBytes = GC.GetTotalAllocatedBytes(),
                        TotalAllocatedBytesFormatted = FormatBytes(GC.GetTotalAllocatedBytes())
                    },
                    Threads = new ThreadInfo
                    {
                        ThreadPoolThreads = Process.GetCurrentProcess().Threads.Count,
                        WorkerThreads = maxWorkerThreads - workerThreads,
                        CompletionPortThreads = maxCompletionPortThreads - completionPortThreads
                    },
                    ApplicationMemory = new ApplicationMemoryInfo
                    {
                        WorkingSet = process.WorkingSet64,
                        PrivateMemory = process.PrivateMemorySize64,
                        ManagedMemory = GC.GetTotalMemory(false),
                        WorkingSetFormatted = FormatBytes(process.WorkingSet64),
                        PrivateMemoryFormatted = FormatBytes(process.PrivateMemorySize64),
                        ManagedMemoryFormatted = FormatBytes(GC.GetTotalMemory(false))
                    }
                };
            });
        }

        /// <summary>
        /// 获取内存信息
        /// </summary>
        public async Task<MemoryInfo> GetMemoryInfoAsync()
        {
            return await Task.Run(() =>
            {
                var memoryInfo = new MemoryInfo();

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    try
                    {
                        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            memoryInfo.TotalMemory = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                            break;
                        }

                        using var memSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                        foreach (ManagementObject obj in memSearcher.Get())
                        {
                            memoryInfo.AvailableMemory = Convert.ToInt64(obj["FreePhysicalMemory"]) * 1024;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("无法获取详细内存信息: {Message}", ex.Message);
                        // 使用 GC 信息作为备选
                        memoryInfo.TotalMemory = GC.GetTotalMemory(false);
                        memoryInfo.AvailableMemory = memoryInfo.TotalMemory / 2; // 估算值
                    }
                }
                else
                {
                    // 非 Windows 系统的处理
                    memoryInfo.TotalMemory = GC.GetTotalMemory(false);
                    memoryInfo.AvailableMemory = memoryInfo.TotalMemory / 2;
                }

                memoryInfo.UsedMemory = memoryInfo.TotalMemory - memoryInfo.AvailableMemory;
                memoryInfo.UsagePercentage = memoryInfo.TotalMemory > 0 
                    ? (double)memoryInfo.UsedMemory / memoryInfo.TotalMemory * 100 
                    : 0;

                memoryInfo.TotalMemoryFormatted = FormatBytes(memoryInfo.TotalMemory);
                memoryInfo.AvailableMemoryFormatted = FormatBytes(memoryInfo.AvailableMemory);
                memoryInfo.UsedMemoryFormatted = FormatBytes(memoryInfo.UsedMemory);

                return memoryInfo;
            });
        }

        /// <summary>
        /// 获取磁盘信息
        /// </summary>
        public async Task<List<DiskInfo>> GetDiskInfoAsync()
        {
            return await Task.Run(() =>
            {
                var diskInfos = new List<DiskInfo>();

                try
                {
                    var drives = DriveInfo.GetDrives();
                    foreach (var drive in drives)
                    {
                        if (drive.IsReady)
                        {
                            var usedSpace = drive.TotalSize - drive.AvailableFreeSpace;
                            var usagePercentage = drive.TotalSize > 0 
                                ? (double)usedSpace / drive.TotalSize * 100 
                                : 0;

                            diskInfos.Add(new DiskInfo
                            {
                                Name = drive.Name,
                                FileSystem = drive.DriveFormat,
                                TotalSize = drive.TotalSize,
                                AvailableSpace = drive.AvailableFreeSpace,
                                UsedSpace = usedSpace,
                                UsagePercentage = usagePercentage,
                                TotalSizeFormatted = FormatBytes(drive.TotalSize),
                                AvailableSpaceFormatted = FormatBytes(drive.AvailableFreeSpace),
                                UsedSpaceFormatted = FormatBytes(usedSpace)
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "获取磁盘信息时发生错误");
                }

                return diskInfos;
            });
        }

        /// <summary>
        /// 获取处理器信息
        /// </summary>
        public async Task<ProcessorInfo> GetProcessorInfoAsync()
        {
            return await Task.Run(() =>
            {
                var processorInfo = new ProcessorInfo
                {
                    CoreCount = Environment.ProcessorCount,
                    LogicalProcessorCount = Environment.ProcessorCount
                };

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    try
                    {
                        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            processorInfo.Name = obj["Name"]?.ToString() ?? "Unknown Processor";
                            if (obj["NumberOfCores"] != null)
                            {
                                processorInfo.CoreCount = Convert.ToInt32(obj["NumberOfCores"]);
                            }
                            if (obj["NumberOfLogicalProcessors"] != null)
                            {
                                processorInfo.LogicalProcessorCount = Convert.ToInt32(obj["NumberOfLogicalProcessors"]);
                            }
                            break;
                        }

                        // 获取 CPU 使用率
                        using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                        cpuCounter.NextValue(); // 第一次调用返回 0
                        Thread.Sleep(100); // 等待一小段时间
                        processorInfo.UsagePercentage = Math.Round(cpuCounter.NextValue(), 2);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("无法获取详细处理器信息: {Message}", ex.Message);
                        processorInfo.Name = "Unknown Processor";
                        processorInfo.UsagePercentage = 0;
                    }
                }
                else
                {
                    processorInfo.Name = "Unknown Processor";
                    processorInfo.UsagePercentage = 0;
                }

                return processorInfo;
            });
        }

        /// <summary>
        /// 格式化字节数
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }
}