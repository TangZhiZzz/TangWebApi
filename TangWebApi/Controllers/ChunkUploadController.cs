using Microsoft.AspNetCore.Mvc;
using TangWebApi.Models;
using TangWebApi.Services;
using System.ComponentModel.DataAnnotations;

namespace TangWebApi.Controllers
{
    /// <summary>
    /// 分片上传控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("分片上传")]
    public class ChunkUploadController : ControllerBase
    {
        private readonly IChunkUploadService _chunkUploadService;
        private readonly ILogger<ChunkUploadController> _logger;

        public ChunkUploadController(
            IChunkUploadService chunkUploadService,
            ILogger<ChunkUploadController> logger)
        {
            _chunkUploadService = chunkUploadService;
            _logger = logger;
        }

        /// <summary>
        /// 初始化分片上传
        /// </summary>
        /// <param name="request">初始化请求</param>
        /// <returns>初始化响应</returns>
        [HttpPost("initialize")]
        [ProducesResponseType(typeof(ChunkUploadInitResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> InitializeChunkUpload([FromBody] ChunkUploadInitRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("请求参数不能为空");
                }

                if (string.IsNullOrEmpty(request.FileName))
                {
                    return BadRequest("文件名不能为空");
                }

                if (request.FileSize <= 0)
                {
                    return BadRequest("文件大小必须大于0");
                }

                var result = await _chunkUploadService.InitializeChunkUploadAsync(request);
                
                if (!result.Success)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化分片上传失败");
                return BadRequest($"初始化分片上传失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 上传分片
        /// </summary>
        /// <param name="uploadId">上传ID</param>
        /// <param name="chunkIndex">分片索引</param>
        /// <param name="chunkFile">分片文件</param>
        /// <param name="chunkHash">分片哈希值（可选）</param>
        /// <returns>上传响应</returns>
        [HttpPost("{uploadId}/chunk/{chunkIndex}")]
        [ProducesResponseType(typeof(ChunkUploadResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> UploadChunk(
            [Required] string uploadId,
            [Required] int chunkIndex,
            [Required] IFormFile chunkFile,
            [FromForm] string? chunkHash = null)
        {
            try
            {
                if (string.IsNullOrEmpty(uploadId))
                {
                    return BadRequest("上传ID不能为空");
                }

                if (chunkIndex < 0)
                {
                    return BadRequest("分片索引不能小于0");
                }

                if (chunkFile == null || chunkFile.Length == 0)
                {
                    return BadRequest("分片文件不能为空");
                }

                // 读取分片数据
                byte[] chunkData;
                using (var memoryStream = new MemoryStream())
                {
                    await chunkFile.CopyToAsync(memoryStream);
                    chunkData = memoryStream.ToArray();
                }

                var request = new ChunkUploadRequest
                {
                    UploadId = uploadId,
                    ChunkIndex = chunkIndex,
                    ChunkData = chunkData,
                    ChunkHash = chunkHash
                };

                var result = await _chunkUploadService.UploadChunkAsync(request);
                
                if (!result.Success)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传分片失败: UploadId={UploadId}, ChunkIndex={ChunkIndex}", uploadId, chunkIndex);
                return BadRequest($"上传分片失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查分片上传状态
        /// </summary>
        /// <param name="uploadId">上传ID</param>
        /// <returns>状态响应</returns>
        [HttpGet("{uploadId}/status")]
        [ProducesResponseType(typeof(ChunkUploadStatusResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> GetChunkUploadStatus([Required] string uploadId)
        {
            try
            {
                if (string.IsNullOrEmpty(uploadId))
                {
                    return BadRequest("上传ID不能为空");
                }

                var request = new ChunkUploadStatusRequest
                {
                    UploadId = uploadId
                };

                var result = await _chunkUploadService.GetChunkUploadStatusAsync(request);
                
                if (!result.Success)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取上传状态失败: UploadId={UploadId}", uploadId);
                return BadRequest($"获取上传状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 合并分片文件
        /// </summary>
        /// <param name="uploadId">上传ID</param>
        /// <param name="request">合并请求</param>
        /// <returns>合并响应</returns>
        [HttpPost("{uploadId}/merge")]
        [ProducesResponseType(typeof(ChunkMergeResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> MergeChunks(
            [Required] string uploadId,
            [FromBody] ChunkMergeRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(uploadId))
                {
                    return BadRequest("上传ID不能为空");
                }

                if (request == null)
                {
                    request = new ChunkMergeRequest();
                }

                request.UploadId = uploadId;

                var result = await _chunkUploadService.MergeChunksAsync(request);
                
                if (!result.Success)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "合并分片失败: UploadId={UploadId}", uploadId);
                return BadRequest($"合并分片失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 取消分片上传
        /// </summary>
        /// <param name="uploadId">上传ID</param>
        /// <returns>取消结果</returns>
        [HttpDelete("{uploadId}")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> CancelChunkUpload([Required] string uploadId)
        {
            try
            {
                if (string.IsNullOrEmpty(uploadId))
                {
                    return BadRequest("上传ID不能为空");
                }

                var result = await _chunkUploadService.CancelChunkUploadAsync(uploadId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消分片上传失败: UploadId={UploadId}", uploadId);
                return BadRequest($"取消分片上传失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取已上传的分片列表（断点续传支持）
        /// </summary>
        /// <param name="uploadId">上传ID</param>
        /// <returns>已上传的分片索引列表</returns>
        [HttpGet("{uploadId}/chunks")]
        [ProducesResponseType(typeof(List<int>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> GetUploadedChunks([Required] string uploadId)
        {
            try
            {
                if (string.IsNullOrEmpty(uploadId))
                {
                    return BadRequest("上传ID不能为空");
                }

                var result = await _chunkUploadService.GetUploadedChunksAsync(uploadId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取已上传分片列表失败: UploadId={UploadId}", uploadId);
                return BadRequest($"获取已上传分片列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 验证分片完整性
        /// </summary>
        /// <param name="uploadId">上传ID</param>
        /// <param name="chunkIndex">分片索引</param>
        /// <param name="expectedHash">期望的哈希值</param>
        /// <returns>验证结果</returns>
        [HttpPost("{uploadId}/chunk/{chunkIndex}/validate")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> ValidateChunk(
            [Required] string uploadId,
            [Required] int chunkIndex,
            [FromBody] string expectedHash)
        {
            try
            {
                if (string.IsNullOrEmpty(uploadId))
                {
                    return BadRequest("上传ID不能为空");
                }

                if (chunkIndex < 0)
                {
                    return BadRequest("分片索引不能小于0");
                }

                if (string.IsNullOrEmpty(expectedHash))
                {
                    return BadRequest("期望的哈希值不能为空");
                }

                var result = await _chunkUploadService.ValidateChunkAsync(uploadId, chunkIndex, expectedHash);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证分片完整性失败: UploadId={UploadId}, ChunkIndex={ChunkIndex}", uploadId, chunkIndex);
                return BadRequest($"验证分片完整性失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理过期的分片文件
        /// </summary>
        /// <param name="expireHours">过期小时数，默认24小时</param>
        /// <returns>清理的文件数量</returns>
        [HttpPost("cleanup")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> CleanupExpiredChunks([FromQuery] int expireHours = 24)
        {
            try
            {
                if (expireHours <= 0)
                {
                    return BadRequest("过期小时数必须大于0");
                }

                var result = await _chunkUploadService.CleanupExpiredChunksAsync(expireHours);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理过期分片失败");
                return BadRequest($"清理过期分片失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 计算文件分片信息
        /// </summary>
        /// <param name="fileSize">文件大小</param>
        /// <param name="chunkSize">分片大小（可选，使用配置默认值）</param>
        /// <returns>分片信息</returns>
        [HttpGet("calculate")]
        [ProducesResponseType(typeof(ChunkUploadInfo), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public IActionResult CalculateChunkInfo(
            [Required] long fileSize,
            [FromQuery] int? chunkSize = null)
        {
            try
            {
                if (fileSize <= 0)
                {
                    return BadRequest("文件大小必须大于0");
                }

                // 使用默认分片大小或指定的分片大小
                var actualChunkSize = chunkSize ?? 1024 * 1024; // 默认1MB
                
                if (actualChunkSize <= 0)
                {
                    return BadRequest("分片大小必须大于0");
                }

                var result = _chunkUploadService.CalculateChunkInfo(fileSize, actualChunkSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "计算分片信息失败: FileSize={FileSize}", fileSize);
                return BadRequest($"计算分片信息失败: {ex.Message}");
            }
        }
    }
}