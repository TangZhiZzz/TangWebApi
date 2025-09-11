using TangWebApi.Models;

namespace TangWebApi.Services
{
    /// <summary>
    /// 分片上传服务接口
    /// </summary>
    public interface IChunkUploadService
    {
        /// <summary>
        /// 初始化分片上传
        /// </summary>
        /// <param name="request">初始化请求</param>
        /// <returns>初始化响应</returns>
        Task<ChunkUploadInitResponse> InitializeChunkUploadAsync(ChunkUploadInitRequest request);

        /// <summary>
        /// 上传分片
        /// </summary>
        /// <param name="request">分片上传请求</param>
        /// <returns>分片上传响应</returns>
        Task<ChunkUploadResponse> UploadChunkAsync(ChunkUploadRequest request);

        /// <summary>
        /// 检查分片上传状态
        /// </summary>
        /// <param name="request">状态检查请求</param>
        /// <returns>状态检查响应</returns>
        Task<ChunkUploadStatusResponse> GetChunkUploadStatusAsync(ChunkUploadStatusRequest request);

        /// <summary>
        /// 合并分片文件
        /// </summary>
        /// <param name="request">合并请求</param>
        /// <returns>合并响应</returns>
        Task<ChunkMergeResponse> MergeChunksAsync(ChunkMergeRequest request);

        /// <summary>
        /// 取消分片上传
        /// </summary>
        /// <param name="uploadId">上传ID</param>
        /// <returns>是否成功</returns>
        Task<bool> CancelChunkUploadAsync(string uploadId);

        /// <summary>
        /// 清理过期的分片文件
        /// </summary>
        /// <param name="expireHours">过期小时数</param>
        /// <returns>清理的文件数量</returns>
        Task<int> CleanupExpiredChunksAsync(int expireHours = 24);

        /// <summary>
        /// 验证分片完整性
        /// </summary>
        /// <param name="uploadId">上传ID</param>
        /// <param name="chunkIndex">分片索引</param>
        /// <param name="expectedHash">期望的哈希值</param>
        /// <returns>是否验证通过</returns>
        Task<bool> ValidateChunkAsync(string uploadId, int chunkIndex, string expectedHash);

        /// <summary>
        /// 获取已上传的分片列表
        /// </summary>
        /// <param name="uploadId">上传ID</param>
        /// <returns>已上传的分片索引列表</returns>
        Task<List<int>> GetUploadedChunksAsync(string uploadId);

        /// <summary>
        /// 计算文件分片信息
        /// </summary>
        /// <param name="fileSize">文件大小</param>
        /// <param name="chunkSize">分片大小</param>
        /// <returns>分片信息</returns>
        ChunkUploadInfo CalculateChunkInfo(long fileSize, int chunkSize);
    }
}