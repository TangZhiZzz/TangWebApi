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
        [ProducesResponseType(typeof(FileUploadResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> UploadFile(
            [Required] IFormFile file,
            [FromForm] string? description = null,
            [FromForm] int? userId = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("请选择要上传的文件");
                }

                var request = new FileUploadRequest
                {
                    File = file,
                    Description = description,
                    UploadUserId = userId
                };

                var result = await _fileService.UploadFileAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件上传失败");
                return BadRequest($"文件上传失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 批量上传文件
        /// </summary>
        /// <param name="files">文件列表</param>
        /// <param name="description">文件描述</param>
        /// <param name="userId">用户ID</param>
        /// <returns>批量上传结果</returns>
        [HttpPost("upload/batch")]
        [ProducesResponseType(typeof(BatchFileUploadResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> UploadFiles(
            [Required] List<IFormFile> files,
            [FromForm] string? description = null,
            [FromForm] int? userId = null)
        {
            try
            {
                if (files == null || !files.Any())
                {
                    return BadRequest("请选择要上传的文件");
                }

                var request = new BatchFileUploadRequest
                {
                    Files = files,
                    Description = description,
                    UploadUserId = userId
                };

                var result = await _fileService.UploadFilesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量文件上传失败");
                return BadRequest($"批量文件上传失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 根据文件ID下载文件
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <returns>文件流</returns>
        [HttpGet("download/{id}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> DownloadFile(int id)
        {
            try
            {
                var (fileStream, fileInfo) = await _fileService.DownloadFileAsync(id);
                if (fileStream == null || fileInfo == null)
                {
                    return NotFound("文件不存在");
                }

                return File(fileStream, fileInfo.ContentType, fileInfo.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件下载失败，文件ID: {FileId}", id);
                return BadRequest($"文件下载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 根据文件名下载文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件流</returns>
        [HttpGet("download/name/{fileName}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> DownloadFileByName(string fileName)
        {
            try
            {
                var (fileStream, fileInfo) = await _fileService.DownloadFileByNameAsync(fileName);
                if (fileStream == null || fileInfo == null)
                {
                    return NotFound("文件不存在");
                }

                return File(fileStream, fileInfo.ContentType, fileInfo.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件下载失败，文件名: {FileName}", fileName);
                return BadRequest($"文件下载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 逻辑删除文件
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> DeleteFile(int id)
        {
            try
            {
                var result = await _fileService.DeleteFileAsync(id);
                if (!result)
                {
                    return NotFound("文件不存在");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件删除失败，文件ID: {FileId}", id);
                return BadRequest($"文件删除失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 物理删除文件
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}/physical")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> PhysicalDeleteFile(int id)
        {
            try
            {
                var result = await _fileService.DeleteFilePhysicallyAsync(id);
                if (!result)
                {
                    return NotFound("文件不存在");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件物理删除失败，文件ID: {FileId}", id);
                return BadRequest($"文件物理删除失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 根据ID获取文件信息
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <returns>文件信息</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TangWebApi.Models.FileInfo), 200)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> GetFileById(int id)
        {
            try
            {
                var file = await _fileService.GetFileByIdAsync(id);
                if (file == null)
                {
                    return NotFound("文件不存在");
                }

                return Ok(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取文件信息失败，文件ID: {FileId}", id);
                return BadRequest($"获取文件信息失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 分页查询文件列表
        /// </summary>
        /// <param name="request">查询条件</param>
        /// <returns>文件列表</returns>
        [HttpPost("query")]
        [ProducesResponseType(typeof(List<TangWebApi.Models.FileInfo>), 200)]
        public async Task<IActionResult> QueryFiles([FromBody] FileQueryRequest request)
        {
            try
            {
                var files = await _fileService.GetFilesAsync(request);
                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询文件列表失败");
                return BadRequest($"查询文件列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新文件信息
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <param name="fileName">新文件名</param>
        /// <param name="description">新描述</param>
        /// <returns>更新结果</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> UpdateFileInfo(
            int id,
            [FromBody] string? fileName = null,
            [FromQuery] string? description = null)
        {
            try
            {
                var result = await _fileService.UpdateFileInfoAsync(id, description);
                if (!result)
                {
                    return NotFound("文件不存在");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新文件信息失败，文件ID: {FileId}", id);
                return BadRequest($"更新文件信息失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <returns>是否存在</returns>
        [HttpGet("{id}/exists")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> FileExists(int id)
        {
            try
            {
                var exists = await _fileService.FileExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查文件存在性失败，文件ID: {FileId}", id);
                return BadRequest($"检查文件存在性失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取文件统计信息
        /// </summary>
        /// <param name="userId">用户ID（可选）</param>
        /// <returns>统计信息</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetFileStatistics([FromQuery] int? userId = null)
        {
            try
            {
                var statistics = await _fileService.GetFileStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取文件统计信息失败");
                return BadRequest($"获取文件统计信息失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理过期文件
        /// </summary>
        /// <param name="days">过期天数</param>
        /// <returns>清理结果</returns>
        [HttpPost("cleanup")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> CleanupExpiredFiles([FromQuery] int days = 30)
        {
            try
            {
                var count = await _fileService.CleanupExpiredFilesAsync(days);
                return Ok(new { CleanedCount = count, Message = $"已清理 {count} 个过期文件" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理过期文件失败");
                return BadRequest($"清理过期文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取文件访问URL
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <returns>访问URL</returns>
        [HttpGet("{id}/url")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> GetFileUrl(int id)
        {
            try
            {
                var fileInfo = await _fileService.GetFileByIdAsync(id);
                if (fileInfo == null)
                {
                    return NotFound("文件不存在");
                }

                var url = _fileService.GetFileUrl(fileInfo.FileName);
                return Ok(new { Url = url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取文件URL失败，文件ID: {FileId}", id);
                return BadRequest($"获取文件URL失败: {ex.Message}");
            }
        }
    }
}