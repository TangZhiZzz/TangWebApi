using Microsoft.Extensions.Options;
using SqlSugar;
using System.Security.Cryptography;
using System.Text;
using TangWebApi.Models;
using FileInfo = TangWebApi.Models.FileInfo;

namespace TangWebApi.Services
{
    /// <summary>
    /// 分片上传服务实现类
    /// </summary>
    public class ChunkUploadService : IChunkUploadService
    {
        private readonly ISqlSugarClient _db;
        private readonly FileSettings _fileSettings;
        private readonly ILogger<ChunkUploadService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IFileService _fileService;

        public ChunkUploadService(
            ISqlSugarClient db,
            IOptions<FileSettings> fileSettings,
            ILogger<ChunkUploadService> logger,
            IWebHostEnvironment environment,
            IFileService fileService)
        {
            _db = db;
            _fileSettings = fileSettings.Value;
            _logger = logger;
            _environment = environment;
            _fileService = fileService;

            // 确保临时目录存在
            EnsureTempDirectoryExists();
        }

        /// <summary>
        /// 初始化分片上传
        /// </summary>
        public async Task<ChunkUploadInitResponse> InitializeChunkUploadAsync(ChunkUploadInitRequest request)
        {
            try
            {
                // 验证文件信息
                ValidateFileInfo(request.FileName, request.FileSize, request.ContentType);

                // 生成唯一上传ID
                var uploadId = Guid.NewGuid().ToString("N");

                // 计算分片信息
                var chunkInfo = CalculateChunkInfo(request.FileSize, _fileSettings.ChunkSize);

                // 创建上传记录
                var uploadRecord = new ChunkUploadInfo
                {
                    UploadId = uploadId,
                    FileName = request.FileName,
                    FileSize = request.FileSize,
                    ContentType = request.ContentType,
                    TotalChunks = chunkInfo.TotalChunks,
                    ChunkSize = _fileSettings.ChunkSize,
                    UploadUserId = request.UploadUserId,
                    Status = "Initialized",
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddHours(_fileSettings.ChunkRetentionHours)
                };

                // 保存到数据库
                await _db.Insertable(uploadRecord).ExecuteCommandAsync();

                // 创建临时目录
                var tempDir = GetTempDirectory(uploadId);
                Directory.CreateDirectory(tempDir);

                _logger.LogInformation("分片上传初始化成功: UploadId={UploadId}, FileName={FileName}, TotalChunks={TotalChunks}", 
                    uploadId, request.FileName, chunkInfo.TotalChunks);

                return new ChunkUploadInitResponse
                {
                    Success = true,
                    UploadId = uploadId,
                    ChunkSize = _fileSettings.ChunkSize,
                    TotalChunks = chunkInfo.TotalChunks,
                    ExpiresAt = uploadRecord.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分片上传初始化失败: FileName={FileName}", request.FileName);
                return new ChunkUploadInitResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 上传分片
        /// </summary>
        public async Task<ChunkUploadResponse> UploadChunkAsync(ChunkUploadRequest request)
        {
            try
            {
                // 验证上传记录
                var uploadRecord = await GetUploadRecordAsync(request.UploadId);
                if (uploadRecord == null)
                {
                    throw new ArgumentException("无效的上传ID");
                }

                if (uploadRecord.ExpiresAt < DateTime.Now)
                {
                    throw new ArgumentException("上传已过期");
                }

                // 验证分片索引
                if (request.ChunkIndex < 0 || request.ChunkIndex >= uploadRecord.TotalChunks)
                {
                    throw new ArgumentException("无效的分片索引");
                }

                // 检查分片是否已存在
                var chunkPath = GetChunkPath(request.UploadId, request.ChunkIndex);
                if (File.Exists(chunkPath))
                {
                    _logger.LogWarning("分片已存在: UploadId={UploadId}, ChunkIndex={ChunkIndex}", request.UploadId, request.ChunkIndex);
                    return new ChunkUploadResponse
                    {
                        Success = true,
                        ChunkIndex = request.ChunkIndex,
                        Message = "分片已存在"
                    };
                }

                // 验证分片大小
                var expectedSize = CalculateChunkSize(uploadRecord.FileSize, request.ChunkIndex, uploadRecord.ChunkSize);
                if (request.ChunkData.Length != expectedSize)
                {
                    throw new ArgumentException($"分片大小不匹配，期望: {expectedSize}, 实际: {request.ChunkData.Length}");
                }

                // 验证分片哈希
                if (!string.IsNullOrEmpty(request.ChunkHash))
                {
                    var actualHash = CalculateHash(request.ChunkData);
                    if (!string.Equals(actualHash, request.ChunkHash, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException("分片哈希验证失败");
                    }
                }

                // 保存分片文件
                await File.WriteAllBytesAsync(chunkPath, request.ChunkData);

                // 更新分片记录
                var chunkDetail = new ChunkDetail
                {
                    UploadId = request.UploadId,
                    ChunkIndex = request.ChunkIndex,
                    ChunkSize = request.ChunkData.Length,
                    ChunkHash = request.ChunkHash,
                    FilePath = chunkPath,
                    UploadedAt = DateTime.Now
                };

                await _db.Insertable(chunkDetail).ExecuteCommandAsync();

                // 更新上传记录状态
                var uploadedChunks = await GetUploadedChunksAsync(request.UploadId);
                var progress = (double)uploadedChunks.Count / uploadRecord.TotalChunks * 100;
                
                await _db.Updateable<ChunkUploadInfo>()
                    .SetColumns(u => new ChunkUploadInfo 
                    { 
                        Status = uploadedChunks.Count == uploadRecord.TotalChunks ? "Completed" : "Uploading",
                        UpdatedAt = DateTime.Now
                    })
                    .Where(u => u.UploadId == request.UploadId)
                    .ExecuteCommandAsync();

                _logger.LogInformation("分片上传成功: UploadId={UploadId}, ChunkIndex={ChunkIndex}, Progress={Progress:F1}%", 
                    request.UploadId, request.ChunkIndex, progress);

                return new ChunkUploadResponse
                {
                    Success = true,
                    ChunkIndex = request.ChunkIndex,
                    Progress = progress,
                    Message = "分片上传成功"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分片上传失败: UploadId={UploadId}, ChunkIndex={ChunkIndex}", request.UploadId, request.ChunkIndex);
                return new ChunkUploadResponse
                {
                    Success = false,
                    ChunkIndex = request.ChunkIndex,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 检查分片上传状态
        /// </summary>
        public async Task<ChunkUploadStatusResponse> GetChunkUploadStatusAsync(ChunkUploadStatusRequest request)
        {
            try
            {
                var uploadRecord = await GetUploadRecordAsync(request.UploadId);
                if (uploadRecord == null)
                {
                    return new ChunkUploadStatusResponse
                    {
                        Success = false,
                        ErrorMessage = "无效的上传ID"
                    };
                }

                var uploadedChunks = await GetUploadedChunksAsync(request.UploadId);
                var progress = (double)uploadedChunks.Count / uploadRecord.TotalChunks * 100;

                return new ChunkUploadStatusResponse
                {
                    Success = true,
                    UploadId = request.UploadId,
                    Status = uploadRecord.Status,
                    Progress = progress,
                    TotalChunks = uploadRecord.TotalChunks,
                    UploadedChunks = uploadedChunks,
                    ExpiresAt = uploadRecord.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取上传状态失败: UploadId={UploadId}", request.UploadId);
                return new ChunkUploadStatusResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 合并分片文件
        /// </summary>
        public async Task<ChunkMergeResponse> MergeChunksAsync(ChunkMergeRequest request)
        {
            try
            {
                var uploadRecord = await GetUploadRecordAsync(request.UploadId);
                if (uploadRecord == null)
                {
                    throw new ArgumentException("无效的上传ID");
                }

                if (uploadRecord.Status != "Completed")
                {
                    throw new InvalidOperationException("分片上传未完成，无法合并");
                }

                // 验证所有分片都已上传
                var uploadedChunks = await GetUploadedChunksAsync(request.UploadId);
                if (uploadedChunks.Count != uploadRecord.TotalChunks)
                {
                    throw new InvalidOperationException($"分片不完整，期望: {uploadRecord.TotalChunks}, 实际: {uploadedChunks.Count}");
                }

                // 生成最终文件名
                var finalFileName = _fileService.GenerateUniqueFileName(uploadRecord.FileName);
                var finalFilePath = Path.Combine(GetUploadPath(), finalFileName);

                // 合并分片文件
                using (var outputStream = new FileStream(finalFilePath, FileMode.Create))
                {
                    for (int i = 0; i < uploadRecord.TotalChunks; i++)
                    {
                        var chunkPath = GetChunkPath(request.UploadId, i);
                        if (!File.Exists(chunkPath))
                        {
                            throw new FileNotFoundException($"分片文件不存在: {chunkPath}");
                        }

                        using (var chunkStream = new FileStream(chunkPath, FileMode.Open, FileAccess.Read))
                        {
                            await chunkStream.CopyToAsync(outputStream);
                        }
                    }
                }

                // 验证合并后的文件大小
                var fileInfo = new System.IO.FileInfo(finalFilePath);
                if (fileInfo.Length != uploadRecord.FileSize)
                {
                    File.Delete(finalFilePath);
                    throw new InvalidOperationException($"合并后文件大小不匹配，期望: {uploadRecord.FileSize}, 实际: {fileInfo.Length}");
                }

                // 计算文件MD5
                string md5Hash;
                using (var fileStream = new FileStream(finalFilePath, FileMode.Open, FileAccess.Read))
                {
                    md5Hash = await _fileService.CalculateMD5Async(fileStream);
                }

                // 验证文件MD5（如果提供）
                if (!string.IsNullOrEmpty(request.ExpectedMD5) && 
                    !string.Equals(md5Hash, request.ExpectedMD5, StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(finalFilePath);
                    throw new InvalidOperationException("文件MD5验证失败");
                }

                // 保存文件信息到数据库
                var dbFileInfo = new FileInfo
                {
                    FileName = finalFileName,
                    OriginalFileName = uploadRecord.FileName,
                    FilePath = finalFilePath,
                    FileSize = uploadRecord.FileSize,
                    ContentType = uploadRecord.ContentType,
                    FileExtension = Path.GetExtension(uploadRecord.FileName).ToLower(),
                    MD5Hash = md5Hash,
                    UploadUserId = uploadRecord.UploadUserId,
                    Description = request.Description,
                    UploadTime = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var result = await _db.Insertable(dbFileInfo).ExecuteReturnEntityAsync();

                // 更新上传记录状态
                await _db.Updateable<ChunkUploadInfo>()
                    .SetColumns(u => new ChunkUploadInfo 
                    { 
                        Status = "Merged",
                        FileId = result.Id,
                        UpdatedAt = DateTime.Now
                    })
                    .Where(u => u.UploadId == request.UploadId)
                    .ExecuteCommandAsync();

                // 清理临时文件
                _ = Task.Run(() => CleanupTempFiles(request.UploadId));

                _logger.LogInformation("分片合并成功: UploadId={UploadId}, FileId={FileId}, FileName={FileName}", 
                    request.UploadId, result.Id, finalFileName);

                return new ChunkMergeResponse
                {
                    Success = true,
                    FileId = result.Id,
                    FileName = finalFileName,
                    FileUrl = _fileService.GetFileUrl(finalFileName),
                    FileSize = result.FileSize,
                    MD5Hash = md5Hash
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分片合并失败: UploadId={UploadId}", request.UploadId);
                return new ChunkMergeResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 取消分片上传
        /// </summary>
        public async Task<bool> CancelChunkUploadAsync(string uploadId)
        {
            try
            {
                // 更新上传记录状态
                await _db.Updateable<ChunkUploadInfo>()
                    .SetColumns(u => new ChunkUploadInfo 
                    { 
                        Status = "Cancelled",
                        UpdatedAt = DateTime.Now
                    })
                    .Where(u => u.UploadId == uploadId)
                    .ExecuteCommandAsync();

                // 清理临时文件
                CleanupTempFiles(uploadId);

                _logger.LogInformation("分片上传已取消: UploadId={UploadId}", uploadId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消分片上传失败: UploadId={UploadId}", uploadId);
                return false;
            }
        }

        /// <summary>
        /// 清理过期的分片文件
        /// </summary>
        public async Task<int> CleanupExpiredChunksAsync(int expireHours = 24)
        {
            try
            {
                var expireTime = DateTime.Now.AddHours(-expireHours);
                
                // 获取过期的上传记录
                var expiredUploads = await _db.Queryable<ChunkUploadInfo>()
                    .Where(u => u.ExpiresAt < DateTime.Now || 
                               (u.Status != "Merged" && u.CreatedAt < expireTime))
                    .ToListAsync();

                int cleanedCount = 0;
                foreach (var upload in expiredUploads)
                {
                    try
                    {
                        // 清理临时文件
                        CleanupTempFiles(upload.UploadId);
                        
                        // 删除数据库记录
                        await _db.Deleteable<ChunkUploadInfo>()
                            .Where(u => u.UploadId == upload.UploadId)
                            .ExecuteCommandAsync();
                            
                        await _db.Deleteable<ChunkDetail>()
                            .Where(c => c.UploadId == upload.UploadId)
                            .ExecuteCommandAsync();
                            
                        cleanedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "清理过期分片失败: UploadId={UploadId}", upload.UploadId);
                    }
                }

                _logger.LogInformation("清理过期分片完成，清理数量: {CleanedCount}", cleanedCount);
                return cleanedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理过期分片失败");
                return 0;
            }
        }

        /// <summary>
        /// 验证分片完整性
        /// </summary>
        public async Task<bool> ValidateChunkAsync(string uploadId, int chunkIndex, string expectedHash)
        {
            try
            {
                var chunkPath = GetChunkPath(uploadId, chunkIndex);
                if (!File.Exists(chunkPath))
                {
                    return false;
                }

                var chunkData = await File.ReadAllBytesAsync(chunkPath);
                var actualHash = CalculateHash(chunkData);
                
                return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证分片完整性失败: UploadId={UploadId}, ChunkIndex={ChunkIndex}", uploadId, chunkIndex);
                return false;
            }
        }

        /// <summary>
        /// 获取已上传的分片列表
        /// </summary>
        public async Task<List<int>> GetUploadedChunksAsync(string uploadId)
        {
            return await _db.Queryable<ChunkDetail>()
                .Where(c => c.UploadId == uploadId)
                .OrderBy(c => c.ChunkIndex)
                .Select(c => c.ChunkIndex)
                .ToListAsync();
        }

        /// <summary>
        /// 计算文件分片信息
        /// </summary>
        public ChunkUploadInfo CalculateChunkInfo(long fileSize, int chunkSize)
        {
            var totalChunks = (int)Math.Ceiling((double)fileSize / chunkSize);
            
            return new ChunkUploadInfo
            {
                FileSize = fileSize,
                ChunkSize = chunkSize,
                TotalChunks = totalChunks
            };
        }

        #region 私有方法

        /// <summary>
        /// 获取上传记录
        /// </summary>
        private async Task<ChunkUploadInfo?> GetUploadRecordAsync(string uploadId)
        {
            return await _db.Queryable<ChunkUploadInfo>()
                .Where(u => u.UploadId == uploadId)
                .FirstAsync();
        }

        /// <summary>
        /// 验证文件信息
        /// </summary>
        private void ValidateFileInfo(string fileName, long fileSize, string contentType)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("文件名不能为空");
            }

            if (!_fileService.IsFileSizeValid(fileSize))
            {
                throw new ArgumentException($"文件大小超过限制，最大允许 {_fileSettings.MaxFileSize / 1024 / 1024} MB");
            }

            if (!_fileService.IsFileTypeAllowed(fileName, contentType))
            {
                throw new ArgumentException($"不支持的文件类型: {Path.GetExtension(fileName)}");
            }
        }

        /// <summary>
        /// 计算分片大小
        /// </summary>
        private long CalculateChunkSize(long fileSize, int chunkIndex, int chunkSize)
        {
            var totalChunks = (int)Math.Ceiling((double)fileSize / chunkSize);
            
            if (chunkIndex == totalChunks - 1)
            {
                // 最后一个分片
                return fileSize - (long)chunkIndex * chunkSize;
            }
            else
            {
                return chunkSize;
            }
        }

        /// <summary>
        /// 计算哈希值
        /// </summary>
        private string CalculateHash(byte[] data)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(data);
            return Convert.ToHexString(hash);
        }

        /// <summary>
        /// 获取临时目录路径
        /// </summary>
        private string GetTempDirectory(string uploadId)
        {
            return Path.Combine(GetTempBasePath(), uploadId);
        }

        /// <summary>
        /// 获取分片文件路径
        /// </summary>
        private string GetChunkPath(string uploadId, int chunkIndex)
        {
            return Path.Combine(GetTempDirectory(uploadId), $"chunk_{chunkIndex:D6}");
        }

        /// <summary>
        /// 获取临时文件基础路径
        /// </summary>
        private string GetTempBasePath()
        {
            return Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, _fileSettings.TempDirectory);
        }

        /// <summary>
        /// 获取上传路径
        /// </summary>
        private string GetUploadPath()
        {
            return Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, _fileSettings.UploadPath);
        }

        /// <summary>
        /// 确保临时目录存在
        /// </summary>
        private void EnsureTempDirectoryExists()
        {
            var tempPath = GetTempBasePath();
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
                _logger.LogInformation("创建临时目录: {TempPath}", tempPath);
            }
        }

        /// <summary>
        /// 清理临时文件
        /// </summary>
        private void CleanupTempFiles(string uploadId)
        {
            try
            {
                var tempDir = GetTempDirectory(uploadId);
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                    _logger.LogInformation("清理临时文件: {TempDir}", tempDir);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理临时文件失败: UploadId={UploadId}", uploadId);
            }
        }

        #endregion
    }
}