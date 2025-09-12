using Microsoft.AspNetCore.Mvc;
using TangWebApi.Models;
using TangWebApi.Services;
using System.ComponentModel.DataAnnotations;

namespace TangWebApi.Controllers
{
    /// <summary>
    /// 文件管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("文件管理")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileService fileService, ILogger<FileController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// 上传单个文件
        /// </summary>
        /// <param name="file">文件</param>
        /// <param name="description">文件描述</param>
        /// <param name="userId">用户ID</param>
        /// <returns>上传结果</returns>
        [HttpPost("upload")]
        public async Task<FileUploadResponse> UploadFile(
            [Required] IFormFile file,
            [FromForm] string? description = null,
            [FromForm] int? userId = null)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("请选择要上传的文件");
            }

            var request = new FileUploadRequest
            {
                File = file,
                Description = description,
                UploadUserId = userId
            };

            return await _fileService.UploadFileAsync(request);
        }

        /// <summary>
        /// 批量上传文件
        /// </summary>
        /// <param name="files">文件列表</param>
        /// <param name="description">文件描述</param>
        /// <param name="userId">用户ID</param>
        /// <returns>批量上传结果</returns>
        [HttpPost("upload/batch")]
        public async Task<BatchFileUploadResponse> UploadFiles(
            [Required] List<IFormFile> files,
            [FromForm] string? description = null,
            [FromForm] int? userId = null)
        {
            if (files == null || !files.Any())
            {
                throw new ArgumentException("请选择要上传的文件");
            }

            var request = new BatchFileUploadRequest
            {
                Files = files,
                Description = description,
                UploadUserId = userId
            };

            return await _fileService.UploadFilesAsync(request);
        }

        /// <summary>
        /// 根据文件ID下载文件
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <returns>文件流</returns>
        [HttpGet("download/{id}")]
        public async Task<FileResult> DownloadFile(int id)
        {
            var (fileStream, fileInfo) = await _fileService.DownloadFileAsync(id);
            if (fileStream == null || fileInfo == null)
            {
                throw new KeyNotFoundException("文件不存在");
            }

            return File(fileStream, fileInfo.ContentType, fileInfo.FileName);
        }

        /// <summary>
        /// 根据文件名下载文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件流</returns>
        [HttpGet("download/name/{fileName}")]
        public async Task<FileResult> DownloadFileByName(string fileName)
        {
            var (fileStream, fileInfo) = await _fileService.DownloadFileByNameAsync(fileName);
            if (fileStream == null || fileInfo == null)
            {
                throw new KeyNotFoundException("文件不存在");
            }

            return File(fileStream, fileInfo.ContentType, fileInfo.FileName);
        }

        /// <summary>
        /// 逻辑删除文件
        /// </summary>
        /// <param name="id">文件ID</param>
        [HttpDelete("{id}")]
        public async Task DeleteFile(int id)
        {
            var result = await _fileService.DeleteFileAsync(id);
            if (!result)
            {
                throw new KeyNotFoundException("文件不存在");
            }
        }

        /// <summary>
        /// 物理删除文件
        /// </summary>
        /// <param name="id">文件ID</param>
        [HttpDelete("{id}/physical")]
        public async Task PhysicalDeleteFile(int id)
        {
            var result = await _fileService.DeleteFilePhysicallyAsync(id);
            if (!result)
            {
                throw new KeyNotFoundException("文件不存在");
            }
        }

        /// <summary>
        /// 根据ID获取文件信息
        /// </summary>
        /// <param name="id">文件ID</param>
        [HttpGet("{id}")]
        public async Task<TangWebApi.Entity.FileInfo> GetFileById(int id)
        {
            var file = await _fileService.GetFileByIdAsync(id);
            if (file == null)
            {
                throw new KeyNotFoundException("文件不存在");
            }

            return file;
        }

        /// <summary>
        /// 分页查询文件列表
        /// </summary>
        /// <param name="request">查询条件</param>
        [HttpPost("query")]
        public async Task<(List<Entity.FileInfo> files, int totalCount)> QueryFiles([FromBody] FileQueryRequest request)
        {
            return await _fileService.GetFilesAsync(request);
        }

        /// <summary>
        /// 更新文件信息
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <param name="fileName">新文件名</param>
        /// <param name="description">新描述</param>
        [HttpPut("{id}")]
        public async Task<bool> UpdateFileInfo(
            int id,
            [FromBody] string? fileName = null,
            [FromQuery] string? description = null)
        {
            var result = await _fileService.UpdateFileInfoAsync(id, description);
            if (!result)
            {
                throw new KeyNotFoundException("文件不存在");
            }

            return result;
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="id">文件ID</param>
        [HttpGet("{id}/exists")]
        public async Task<bool> FileExists(int id)
        {
            return await _fileService.FileExistsAsync(id);
        }

        /// <summary>
        /// 获取文件统计信息
        /// </summary>
        /// <param name="userId">用户ID（可选）</param>
        [HttpGet("statistics")]
        public async Task<object> GetFileStatistics([FromQuery] int? userId = null)
        {
            return await _fileService.GetFileStatisticsAsync();
        }

        /// <summary>
        /// 清理过期文件
        /// </summary>
        /// <param name="days">过期天数</param>
        [HttpPost("cleanup")]
        public async Task<object> CleanupExpiredFiles([FromQuery] int days = 30)
        {
            var count = await _fileService.CleanupExpiredFilesAsync(days);
            return new { CleanedCount = count, Message = $"已清理 {count} 个过期文件" };
        }

        /// <summary>
        /// 获取文件访问URL
        /// </summary>
        /// <param name="id">文件ID</param>
        [HttpGet("{id}/url")]
        public async Task<object> GetFileUrl(int id)
        {
            var fileInfo = await _fileService.GetFileByIdAsync(id);
            if (fileInfo == null)
            {
                throw new KeyNotFoundException("文件不存在");
            }

            var url = _fileService.GetFileUrl(fileInfo.FileName);
            return new { Url = url };
        }
    }
}