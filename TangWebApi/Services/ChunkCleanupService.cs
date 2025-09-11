using Microsoft.Extensions.Options;
using TangWebApi.Models;
using TangWebApi.Services;

namespace TangWebApi.Services
{
    /// <summary>
    /// 分片文件清理后台服务
    /// </summary>
    public class ChunkCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ChunkCleanupService> _logger;
        private readonly FileSettings _fileSettings;
        private readonly TimeSpan _cleanupInterval;

        public ChunkCleanupService(
            IServiceProvider serviceProvider,
            ILogger<ChunkCleanupService> logger,
            IOptions<FileSettings> fileSettings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _fileSettings = fileSettings.Value;
            _cleanupInterval = TimeSpan.FromHours(1); // 每小时执行一次清理
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("分片文件清理服务已启动");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformCleanupAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "执行分片文件清理时发生错误");
                }

                // 等待下一次清理
                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("分片文件清理服务已停止");
        }

        /// <summary>
        /// 执行清理操作
        /// </summary>
        /// <returns></returns>
        private async Task PerformCleanupAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var chunkUploadService = scope.ServiceProvider.GetRequiredService<IChunkUploadService>();

                var expireHours = _fileSettings.ChunkRetentionHours;
                var cleanedCount = await chunkUploadService.CleanupExpiredChunksAsync(expireHours);

                if (cleanedCount > 0)
                {
                    _logger.LogInformation("清理了 {CleanedCount} 个过期的分片文件", cleanedCount);
                }
                else
                {
                    _logger.LogDebug("没有发现需要清理的过期分片文件");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理分片文件时发生错误");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("正在停止分片文件清理服务...");
            await base.StopAsync(stoppingToken);
        }
    }
}