using Microsoft.Extensions.Options;
using SqlSugar;
using System.Security.Cryptography;
using System.Text;
using TangWebApi.Models;
using FileInfo = TangWebApi.Entity.FileInfo;

namespace TangWebApi.Services
{
    /// <summary>
    /// 文件服务实现类
    /// </summary>
    public class FileService : IFileService
    {
        private readonly ISqlSugarClient _db;
        private readonly TangWebApi.Options.FileSettings _fileSettings;
        private readonly ILogger<FileService> _logger;
        private readonly IWebHostEnvironment _environment;

        public FileService(
            ISqlSugarClient db,
            IOptions<TangWebApi.Options.FileSettings> fileSettings,
            ILogger<FileService> logger,
            IWebHostEnvironment environment)
        {
            _db = db;
            _fileSettings = fileSettings.Value;
            _logger = logger;
            _environment = environment;

            // 确保上传目录存在
            EnsureUploadDirectoryExists();
        }

        /// <summary>
        /// 上传单个文件
        /// </summary>
        public async Task<FileUploadResponse> UploadFileAsync(FileUploadRequest request)
        {
            try
            {
                // 验证文件
                ValidateFile(request.File);

                // 生成唯一文件名
                var uniqueFileName = GenerateUniqueFileName(request.File.FileName);
                var filePath = Path.Combine(GetUploadPath(), uniqueFileName);

                // 计算MD5
                string? md5Hash = null;
                if (_fileSettings.EnableMD5Check)
                {
                    using var stream = request.File.OpenReadStream();
                    md5Hash = await CalculateMD5Async(stream);
                    
                    // 检查是否已存在相同文件
                    var existingFile = await GetFileByMD5Async(md5Hash);
                    if (existingFile != null)
                    {
                        return new FileUploadResponse
                        {
                            FileId = existingFile.Id,
                            FileName = existingFile.FileName,
                            OriginalFileName = existingFile.OriginalFileName,
                            FileUrl = GetFileUrl(existingFile.FileName),
                            FileSize = existingFile.FileSize,
                            ContentType = existingFile.ContentType,
                            UploadTime = existingFile.UploadTime
                        };
                    }
                }

                // 保存文件到磁盘
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(fileStream);
                }

                // 保存文件信息到数据库
                var fileInfo = new FileInfo
                {
                    FileName = uniqueFileName,
                    OriginalFileName = request.File.FileName,
                    FilePath = filePath,
                    FileSize = request.File.Length,
                    ContentType = request.File.ContentType ?? "application/octet-stream",
                    FileExtension = Path.GetExtension(request.File.FileName).ToLower(),
                    MD5Hash = md5Hash,
                    UploadUserId = request.UploadUserId,
                    Description = request.Description,
                    UploadTime = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var result = await _db.Insertable(fileInfo).ExecuteReturnEntityAsync();

                _logger.LogInformation("文件上传成功: {FileName}, 大小: {FileSize} 字节", request.File.FileName, request.File.Length);

                return new FileUploadResponse
                {
                    FileId = result.Id,
                    FileName = result.FileName,
                    OriginalFileName = result.OriginalFileName,
                    FileUrl = GetFileUrl(result.FileName),
                    FileSize = result.FileSize,
                    ContentType = result.ContentType,
                    UploadTime = result.UploadTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件上传失败: {FileName}", request.File.FileName);
                throw;
            }
        }

        /// <summary>
        /// 批量上传文件
        /// </summary>
        public async Task<BatchFileUploadResponse> UploadFilesAsync(BatchFileUploadRequest request)
        {
            var response = new BatchFileUploadResponse
            {
                TotalCount = request.Files.Count
            };

            foreach (var file in request.Files)
            {
                try
                {
                    var uploadRequest = new FileUploadRequest
                    {
                        File = file,
                        Description = request.Description,
                        UploadUserId = request.UploadUserId
                    };

                    var uploadResponse = await UploadFileAsync(uploadRequest);
                    response.SuccessFiles.Add(uploadResponse);
                    response.SuccessCount++;
                }
                catch (Exception ex)
                {
                    response.FailedFiles.Add(new FileUploadError
                    {
                        FileName = file.FileName,
                        ErrorMessage = ex.Message
                    });
                    response.FailedCount++;
                }
            }

            return response;
        }

        /// <summary>
        /// 根据文件ID获取文件信息
        /// </summary>
        public async Task<FileInfo?> GetFileByIdAsync(int fileId)
        {
            return await _db.Queryable<FileInfo>()
                .Where(f => f.Id == fileId && f.Status == 0)
                .FirstAsync();
        }

        /// <summary>
        /// 根据文件名获取文件信息
        /// </summary>
        public async Task<FileInfo?> GetFileByNameAsync(string fileName)
        {
            return await _db.Queryable<FileInfo>()
                .Where(f => f.FileName == fileName && f.Status == 0)
                .FirstAsync();
        }

        /// <summary>
        /// 分页查询文件列表
        /// </summary>
        public async Task<(List<FileInfo> files, int totalCount)> GetFilesAsync(FileQueryRequest request)
        {
            var query = _db.Queryable<FileInfo>()
                .Where(f => f.Status == 0);

            // 文件名筛选
            if (!string.IsNullOrEmpty(request.FileName))
            {
                query = query.Where(f => f.OriginalFileName.Contains(request.FileName) || f.FileName.Contains(request.FileName));
            }

            // 文件类型筛选
            if (!string.IsNullOrEmpty(request.ContentType))
            {
                query = query.Where(f => f.ContentType.Contains(request.ContentType));
            }

            // 上传用户筛选
            if (request.UploadUserId.HasValue)
            {
                query = query.Where(f => f.UploadUserId == request.UploadUserId.Value);
            }

            // 时间范围筛选
            if (request.StartTime.HasValue)
            {
                query = query.Where(f => f.UploadTime >= request.StartTime.Value);
            }

            if (request.EndTime.HasValue)
            {
                query = query.Where(f => f.UploadTime <= request.EndTime.Value);
            }

            var totalCount = await query.CountAsync();
            var files = await query
                .OrderBy(f => f.UploadTime, OrderByType.Desc)
                .ToPageListAsync(request.PageIndex, request.PageSize);

            return (files, totalCount);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        public async Task<(Stream? fileStream, FileInfo? fileInfo)> DownloadFileAsync(int fileId)
        {
            var fileInfo = await GetFileByIdAsync(fileId);
            if (fileInfo == null)
            {
                return (null, null);
            }

            if (!File.Exists(fileInfo.FilePath))
            {
                _logger.LogWarning("文件不存在: {FilePath}", fileInfo.FilePath);
                return (null, fileInfo);
            }

            var fileStream = new FileStream(fileInfo.FilePath, FileMode.Open, FileAccess.Read);
            return (fileStream, fileInfo);
        }

        /// <summary>
        /// 根据文件名下载文件
        /// </summary>
        public async Task<(Stream? fileStream, FileInfo? fileInfo)> DownloadFileByNameAsync(string fileName)
        {
            var fileInfo = await GetFileByNameAsync(fileName);
            if (fileInfo == null)
            {
                return (null, null);
            }

            return await DownloadFileAsync(fileInfo.Id);
        }

        /// <summary>
        /// 删除文件（逻辑删除）
        /// </summary>
        public async Task<bool> DeleteFileAsync(int fileId)
        {
            try
            {
                var result = await _db.Updateable<FileInfo>()
                    .SetColumns(f => new FileInfo { Status = 1, UpdatedAt = DateTime.Now })
                    .Where(f => f.Id == fileId)
                    .ExecuteCommandAsync();

                if (result > 0)
                {
                    _logger.LogInformation("文件逻辑删除成功: FileId = {FileId}", fileId);
                }

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件删除失败: FileId = {FileId}", fileId);
                return false;
            }
        }

        /// <summary>
        /// 物理删除文件
        /// </summary>
        public async Task<bool> DeleteFilePhysicallyAsync(int fileId)
        {
            try
            {
                var fileInfo = await _db.Queryable<FileInfo>()
                    .Where(f => f.Id == fileId)
                    .FirstAsync();

                if (fileInfo == null)
                {
                    return false;
                }

                // 删除物理文件
                if (File.Exists(fileInfo.FilePath))
                {
                    File.Delete(fileInfo.FilePath);
                }

                // 删除数据库记录
                var result = await _db.Deleteable<FileInfo>()
                    .Where(f => f.Id == fileId)
                    .ExecuteCommandAsync();

                if (result > 0)
                {
                    _logger.LogInformation("文件物理删除成功: FileId = {FileId}, FilePath = {FilePath}", fileId, fileInfo.FilePath);
                }

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件物理删除失败: FileId = {FileId}", fileId);
                return false;
            }
        }

        /// <summary>
        /// 批量删除文件
        /// </summary>
        public async Task<(int successCount, int failedCount)> DeleteFilesAsync(List<int> fileIds)
        {
            int successCount = 0;
            int failedCount = 0;

            foreach (var fileId in fileIds)
            {
                if (await DeleteFileAsync(fileId))
                {
                    successCount++;
                }
                else
                {
                    failedCount++;
                }
            }

            return (successCount, failedCount);
        }

        /// <summary>
        /// 更新文件信息
        /// </summary>
        public async Task<bool> UpdateFileInfoAsync(int fileId, string? description)
        {
            try
            {
                var result = await _db.Updateable<FileInfo>()
                    .SetColumns(f => new FileInfo { Description = description, UpdatedAt = DateTime.Now })
                    .Where(f => f.Id == fileId && f.Status == 0)
                    .ExecuteCommandAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新文件信息失败: FileId = {FileId}", fileId);
                return false;
            }
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        public async Task<bool> FileExistsAsync(int fileId)
        {
            var count = await _db.Queryable<FileInfo>()
                .Where(f => f.Id == fileId && f.Status == 0)
                .CountAsync();

            return count > 0;
        }

        /// <summary>
        /// 根据MD5哈希值查找文件
        /// </summary>
        public async Task<FileInfo?> GetFileByMD5Async(string md5Hash)
        {
            if (string.IsNullOrEmpty(md5Hash))
            {
                return null;
            }

            return await _db.Queryable<FileInfo>()
                .Where(f => f.MD5Hash == md5Hash && f.Status == 0)
                .FirstAsync();
        }

        /// <summary>
        /// 获取文件统计信息
        /// </summary>
        public async Task<object> GetFileStatisticsAsync()
        {
            var totalCount = await _db.Queryable<FileInfo>()
                .Where(f => f.Status == 0)
                .CountAsync();

            var totalSize = await _db.Queryable<FileInfo>()
                .Where(f => f.Status == 0)
                .SumAsync(f => f.FileSize);

            var todayCount = await _db.Queryable<FileInfo>()
                .Where(f => f.Status == 0 && f.UploadTime >= DateTime.Today)
                .CountAsync();

            var typeStats = await _db.Queryable<FileInfo>()
                .Where(f => f.Status == 0)
                .GroupBy(f => f.ContentType)
                .Select(g => new { ContentType = g.ContentType, Count = SqlFunc.AggregateCount(g.ContentType) })
                .ToListAsync();

            return new
            {
                TotalCount = totalCount,
                TotalSize = totalSize,
                TodayCount = todayCount,
                TypeStatistics = typeStats
            };
        }

        /// <summary>
        /// 清理过期的临时文件
        /// </summary>
        public async Task<int> CleanupExpiredFilesAsync(int expireDays = 30)
        {
            try
            {
                var expireDate = DateTime.Now.AddDays(-expireDays);
                var expiredFiles = await _db.Queryable<FileInfo>()
                    .Where(f => f.Status == 1 && f.UpdatedAt < expireDate)
                    .ToListAsync();

                int cleanedCount = 0;
                foreach (var file in expiredFiles)
                {
                    if (await DeleteFilePhysicallyAsync(file.Id))
                    {
                        cleanedCount++;
                    }
                }

                _logger.LogInformation("清理过期文件完成，共清理 {Count} 个文件", cleanedCount);
                return cleanedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理过期文件失败");
                return 0;
            }
        }

        /// <summary>
        /// 验证文件类型是否允许
        /// </summary>
        public bool IsFileTypeAllowed(string fileName, string contentType)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return _fileSettings.AllowedExtensions.Contains(extension);
        }

        /// <summary>
        /// 验证文件大小是否符合要求
        /// </summary>
        public bool IsFileSizeValid(long fileSize)
        {
            return fileSize <= _fileSettings.MaxFileSize;
        }

        /// <summary>
        /// 生成唯一文件名
        /// </summary>
        public string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileName = Path.GetFileNameWithoutExtension(originalFileName);
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];
            
            return $"{fileName}_{timestamp}_{guid}{extension}";
        }

        /// <summary>
        /// 计算文件MD5哈希值
        /// </summary>
        public async Task<string> CalculateMD5Async(Stream fileStream)
        {
            using var md5 = MD5.Create();
            var hash = await Task.Run(() => md5.ComputeHash(fileStream));
            fileStream.Position = 0; // 重置流位置
            return Convert.ToHexString(hash).ToLower();
        }

        /// <summary>
        /// 获取文件访问URL
        /// </summary>
        public string GetFileUrl(string fileName)
        {
            return $"{_fileSettings.BaseUrl}/uploads/{fileName}";
        }

        /// <summary>
        /// 验证文件
        /// </summary>
        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("文件不能为空");
            }

            if (!IsFileSizeValid(file.Length))
            {
                throw new ArgumentException($"文件大小超过限制，最大允许 {_fileSettings.MaxFileSize / 1024 / 1024} MB");
            }

            if (!IsFileTypeAllowed(file.FileName, file.ContentType ?? ""))
            {
                throw new ArgumentException($"不支持的文件类型: {Path.GetExtension(file.FileName)}");
            }
        }

        /// <summary>
        /// 确保上传目录存在
        /// </summary>
        private void EnsureUploadDirectoryExists()
        {
            var uploadPath = GetUploadPath();
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
                _logger.LogInformation("创建上传目录: {UploadPath}", uploadPath);
            }
        }

        /// <summary>
        /// 获取上传路径
        /// </summary>
        private string GetUploadPath()
        {
            return Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, _fileSettings.UploadPath);
        }
    }
}