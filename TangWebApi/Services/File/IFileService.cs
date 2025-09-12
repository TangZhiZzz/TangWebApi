using TangWebApi.Models;
using FileInfo = TangWebApi.Entity.FileInfo;

namespace TangWebApi.Services
{
    /// <summary>
    /// 文件服务接口
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// 上传单个文件
        /// </summary>
        /// <param name="request">文件上传请求</param>
        /// <returns>文件上传响应</returns>
        Task<FileUploadResponse> UploadFileAsync(FileUploadRequest request);

        /// <summary>
        /// 批量上传文件
        /// </summary>
        /// <param name="request">批量文件上传请求</param>
        /// <returns>批量文件上传响应</returns>
        Task<BatchFileUploadResponse> UploadFilesAsync(BatchFileUploadRequest request);

        /// <summary>
        /// 根据文件ID获取文件信息
        /// </summary>
        /// <param name="fileId">文件ID</param>
        /// <returns>文件信息</returns>
        Task<FileInfo?> GetFileByIdAsync(int fileId);

        /// <summary>
        /// 根据文件名获取文件信息
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件信息</returns>
        Task<FileInfo?> GetFileByNameAsync(string fileName);

        /// <summary>
        /// 分页查询文件列表
        /// </summary>
        /// <param name="request">查询请求</param>
        /// <returns>文件列表</returns>
        Task<(List<FileInfo> files, int totalCount)> GetFilesAsync(FileQueryRequest request);

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileId">文件ID</param>
        /// <returns>文件流和文件信息</returns>
        Task<(Stream? fileStream, FileInfo? fileInfo)> DownloadFileAsync(int fileId);

        /// <summary>
        /// 根据文件名下载文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件流和文件信息</returns>
        Task<(Stream? fileStream, FileInfo? fileInfo)> DownloadFileByNameAsync(string fileName);

        /// <summary>
        /// 删除文件（逻辑删除）
        /// </summary>
        /// <param name="fileId">文件ID</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeleteFileAsync(int fileId);

        /// <summary>
        /// 物理删除文件
        /// </summary>
        /// <param name="fileId">文件ID</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeleteFilePhysicallyAsync(int fileId);

        /// <summary>
        /// 批量删除文件
        /// </summary>
        /// <param name="fileIds">文件ID列表</param>
        /// <returns>删除结果</returns>
        Task<(int successCount, int failedCount)> DeleteFilesAsync(List<int> fileIds);

        /// <summary>
        /// 更新文件信息
        /// </summary>
        /// <param name="fileId">文件ID</param>
        /// <param name="description">文件描述</param>
        /// <returns>是否更新成功</returns>
        Task<bool> UpdateFileInfoAsync(int fileId, string? description);

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="fileId">文件ID</param>
        /// <returns>是否存在</returns>
        Task<bool> FileExistsAsync(int fileId);

        /// <summary>
        /// 根据MD5哈希值查找文件
        /// </summary>
        /// <param name="md5Hash">MD5哈希值</param>
        /// <returns>文件信息</returns>
        Task<FileInfo?> GetFileByMD5Async(string md5Hash);

        /// <summary>
        /// 获取文件统计信息
        /// </summary>
        /// <returns>文件统计信息</returns>
        Task<object> GetFileStatisticsAsync();

        /// <summary>
        /// 清理过期的临时文件
        /// </summary>
        /// <param name="expireDays">过期天数</param>
        /// <returns>清理的文件数量</returns>
        Task<int> CleanupExpiredFilesAsync(int expireDays = 30);

        /// <summary>
        /// 验证文件类型是否允许
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="contentType">文件类型</param>
        /// <returns>是否允许</returns>
        bool IsFileTypeAllowed(string fileName, string contentType);

        /// <summary>
        /// 验证文件大小是否符合要求
        /// </summary>
        /// <param name="fileSize">文件大小</param>
        /// <returns>是否符合要求</returns>
        bool IsFileSizeValid(long fileSize);

        /// <summary>
        /// 生成唯一文件名
        /// </summary>
        /// <param name="originalFileName">原始文件名</param>
        /// <returns>唯一文件名</returns>
        string GenerateUniqueFileName(string originalFileName);

        /// <summary>
        /// 计算文件MD5哈希值
        /// </summary>
        /// <param name="fileStream">文件流</param>
        /// <returns>MD5哈希值</returns>
        Task<string> CalculateMD5Async(Stream fileStream);

        /// <summary>
        /// 获取文件访问URL
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件访问URL</returns>
        string GetFileUrl(string fileName);
    }
}