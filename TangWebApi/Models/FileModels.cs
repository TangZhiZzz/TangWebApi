using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace TangWebApi.Models
{
    /// <summary>
    /// 文件信息实体类
    /// </summary>
    [SugarTable("Files")]
    public class FileInfo
    {
        /// <summary>
        /// 文件ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        [SugarColumn(Length = 255, IsNullable = false)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 原始文件名
        /// </summary>
        [SugarColumn(Length = 255, IsNullable = false)]
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件路径
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = false)]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public long FileSize { get; set; }

        /// <summary>
        /// 文件类型/MIME类型
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// 文件扩展名
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = false)]
        public string FileExtension { get; set; } = string.Empty;

        /// <summary>
        /// 文件MD5哈希值
        /// </summary>
        [SugarColumn(Length = 32, IsNullable = true)]
        public string? MD5Hash { get; set; }

        /// <summary>
        /// 上传用户ID
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? UploadUserId { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime UploadTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 文件状态（0-正常，1-已删除）
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int Status { get; set; } = 0;

        /// <summary>
        /// 文件描述
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string? Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 文件上传请求模型
    /// </summary>
    public class FileUploadRequest
    {
        /// <summary>
        /// 上传的文件
        /// </summary>
        [Required(ErrorMessage = "请选择要上传的文件")]
        public IFormFile File { get; set; } = null!;

        /// <summary>
        /// 文件描述
        /// </summary>
        [MaxLength(500, ErrorMessage = "文件描述不能超过500个字符")]
        public string? Description { get; set; }

        /// <summary>
        /// 上传用户ID
        /// </summary>
        public int? UploadUserId { get; set; }
    }

    /// <summary>
    /// 批量文件上传请求模型
    /// </summary>
    public class BatchFileUploadRequest
    {
        /// <summary>
        /// 上传的文件列表
        /// </summary>
        [Required(ErrorMessage = "请选择要上传的文件")]
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();

        /// <summary>
        /// 文件描述
        /// </summary>
        [MaxLength(500, ErrorMessage = "文件描述不能超过500个字符")]
        public string? Description { get; set; }

        /// <summary>
        /// 上传用户ID
        /// </summary>
        public int? UploadUserId { get; set; }
    }

    /// <summary>
    /// 文件上传响应模型
    /// </summary>
    public class FileUploadResponse
    {
        /// <summary>
        /// 文件ID
        /// </summary>
        public int FileId { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 原始文件名
        /// </summary>
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件访问URL
        /// </summary>
        public string FileUrl { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime UploadTime { get; set; }
    }

    /// <summary>
    /// 批量文件上传响应模型
    /// </summary>
    public class BatchFileUploadResponse
    {
        /// <summary>
        /// 成功上传的文件列表
        /// </summary>
        public List<FileUploadResponse> SuccessFiles { get; set; } = new List<FileUploadResponse>();

        /// <summary>
        /// 上传失败的文件列表
        /// </summary>
        public List<FileUploadError> FailedFiles { get; set; } = new List<FileUploadError>();

        /// <summary>
        /// 总文件数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 成功数量
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失败数量
        /// </summary>
        public int FailedCount { get; set; }
    }

    /// <summary>
    /// 文件上传错误信息
    /// </summary>
    public class FileUploadError
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// 文件查询请求模型
    /// </summary>
    public class FileQueryRequest
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 文件名关键字
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// 上传用户ID
        /// </summary>
        public int? UploadUserId { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// 文件设置配置
    /// </summary>
    public class FileSettings
    {
        /// <summary>
        /// 文件上传根目录
        /// </summary>
        public string UploadPath { get; set; } = "uploads";

        /// <summary>
        /// 最大文件大小（字节）
        /// </summary>
        public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB

        /// <summary>
        /// 允许的文件扩展名
        /// </summary>
        public List<string> AllowedExtensions { get; set; } = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" };

        /// <summary>
        /// 是否启用文件MD5校验
        /// </summary>
        public bool EnableMD5Check { get; set; } = true;

        /// <summary>
        /// 文件访问基础URL
        /// </summary>
        public string BaseUrl { get; set; } = "/files";

        /// <summary>
        /// 分片大小（字节）
        /// </summary>
        public long ChunkSize { get; set; } = 2 * 1024 * 1024; // 2MB

        /// <summary>
        /// 分片临时存储目录
        /// </summary>
        public string ChunkTempPath { get; set; } = "temp/chunks";

        /// <summary>
        /// 分片文件保留时间（小时）
        /// </summary>
        public int ChunkRetentionHours { get; set; } = 24;
    }

    /// <summary>
    /// 分片上传信息实体类
    /// </summary>
    [SugarTable("ChunkUploads")]
    public class ChunkUploadInfo
    {
        /// <summary>
        /// 分片上传ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 上传标识符（用于标识同一个文件的所有分片）
        /// </summary>
        [SugarColumn(Length = 64, IsNullable = false)]
        public string UploadId { get; set; } = string.Empty;

        /// <summary>
        /// 原始文件名
        /// </summary>
        [SugarColumn(Length = 255, IsNullable = false)]
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件总大小
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public long TotalSize { get; set; }

        /// <summary>
        /// 文件MD5哈希值
        /// </summary>
        [SugarColumn(Length = 32, IsNullable = true)]
        public string? FileMD5 { get; set; }

        /// <summary>
        /// 总分片数
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int TotalChunks { get; set; }

        /// <summary>
        /// 已上传分片数
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int UploadedChunks { get; set; } = 0;

        /// <summary>
        /// 上传状态（0-进行中，1-已完成，2-已取消）
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int Status { get; set; } = 0;

        /// <summary>
        /// 上传用户ID
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? UploadUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 完成时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// 最终文件ID（合并完成后）
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? FinalFileId { get; set; }
    }

    /// <summary>
    /// 分片详情实体类
    /// </summary>
    [SugarTable("ChunkDetails")]
    public class ChunkDetail
    {
        /// <summary>
        /// 分片详情ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 上传标识符
        /// </summary>
        [SugarColumn(Length = 64, IsNullable = false)]
        public string UploadId { get; set; } = string.Empty;

        /// <summary>
        /// 分片序号（从0开始）
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int ChunkIndex { get; set; }

        /// <summary>
        /// 分片大小
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public long ChunkSize { get; set; }

        /// <summary>
        /// 分片MD5哈希值
        /// </summary>
        [SugarColumn(Length = 32, IsNullable = true)]
        public string? ChunkMD5 { get; set; }

        /// <summary>
        /// 分片文件路径
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = false)]
        public string ChunkPath { get; set; } = string.Empty;

        /// <summary>
        /// 上传状态（0-未上传，1-已上传）
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int Status { get; set; } = 0;

        /// <summary>
        /// 上传时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? UploadedAt { get; set; }
    }

    /// <summary>
    /// 分片上传初始化请求
    /// </summary>
    public class ChunkUploadInitRequest
    {
        /// <summary>
        /// 原始文件名
        /// </summary>
        [Required(ErrorMessage = "文件名不能为空")]
        [MaxLength(255, ErrorMessage = "文件名不能超过255个字符")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件总大小
        /// </summary>
        [Required(ErrorMessage = "文件大小不能为空")]
        [Range(1, long.MaxValue, ErrorMessage = "文件大小必须大于0")]
        public long TotalSize { get; set; }

        /// <summary>
        /// 文件MD5哈希值（可选）
        /// </summary>
        [MaxLength(32, ErrorMessage = "MD5哈希值长度不正确")]
        public string? FileMD5 { get; set; }

        /// <summary>
        /// 分片大小
        /// </summary>
        [Range(1024, 10 * 1024 * 1024, ErrorMessage = "分片大小必须在1KB到10MB之间")]
        public long ChunkSize { get; set; } = 2 * 1024 * 1024; // 默认2MB

        /// <summary>
        /// 上传用户ID
        /// </summary>
        public int? UploadUserId { get; set; }
    }

    /// <summary>
    /// 分片上传初始化响应
    /// </summary>
    public class ChunkUploadInitResponse
    {
        /// <summary>
        /// 上传标识符
        /// </summary>
        public string UploadId { get; set; } = string.Empty;

        /// <summary>
        /// 总分片数
        /// </summary>
        public int TotalChunks { get; set; }

        /// <summary>
        /// 分片大小
        /// </summary>
        public long ChunkSize { get; set; }

        /// <summary>
        /// 已上传的分片列表
        /// </summary>
        public List<int> UploadedChunks { get; set; } = new List<int>();
    }

    /// <summary>
    /// 分片上传请求
    /// </summary>
    public class ChunkUploadRequest
    {
        /// <summary>
        /// 上传标识符
        /// </summary>
        [Required(ErrorMessage = "上传标识符不能为空")]
        public string UploadId { get; set; } = string.Empty;

        /// <summary>
        /// 分片序号
        /// </summary>
        [Required(ErrorMessage = "分片序号不能为空")]
        [Range(0, int.MaxValue, ErrorMessage = "分片序号必须大于等于0")]
        public int ChunkIndex { get; set; }

        /// <summary>
        /// 分片文件
        /// </summary>
        [Required(ErrorMessage = "分片文件不能为空")]
        public IFormFile ChunkFile { get; set; } = null!;

        /// <summary>
        /// 分片MD5哈希值（可选）
        /// </summary>
        [MaxLength(32, ErrorMessage = "MD5哈希值长度不正确")]
        public string? ChunkMD5 { get; set; }
    }

    /// <summary>
    /// 分片上传响应
    /// </summary>
    public class ChunkUploadResponse
    {
        /// <summary>
        /// 上传标识符
        /// </summary>
        public string UploadId { get; set; } = string.Empty;

        /// <summary>
        /// 分片序号
        /// </summary>
        public int ChunkIndex { get; set; }

        /// <summary>
        /// 是否上传成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 已上传分片数
        /// </summary>
        public int UploadedChunks { get; set; }

        /// <summary>
        /// 总分片数
        /// </summary>
        public int TotalChunks { get; set; }

        /// <summary>
        /// 上传进度（百分比）
        /// </summary>
        public decimal Progress { get; set; }
    }

    /// <summary>
    /// 分片合并请求
    /// </summary>
    public class ChunkMergeRequest
    {
        /// <summary>
        /// 上传标识符
        /// </summary>
        [Required(ErrorMessage = "上传标识符不能为空")]
        public string UploadId { get; set; } = string.Empty;

        /// <summary>
        /// 文件描述
        /// </summary>
        [MaxLength(500, ErrorMessage = "文件描述不能超过500个字符")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// 分片合并响应
    /// </summary>
    public class ChunkMergeResponse
    {
        /// <summary>
        /// 是否合并成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 最终文件信息
        /// </summary>
        public FileUploadResponse? FileInfo { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 分片上传状态查询请求
    /// </summary>
    public class ChunkUploadStatusRequest
    {
        /// <summary>
        /// 上传标识符
        /// </summary>
        [Required(ErrorMessage = "上传标识符不能为空")]
        public string UploadId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 分片上传状态响应
    /// </summary>
    public class ChunkUploadStatusResponse
    {
        /// <summary>
        /// 上传标识符
        /// </summary>
        public string UploadId { get; set; } = string.Empty;

        /// <summary>
        /// 原始文件名
        /// </summary>
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件总大小
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// 总分片数
        /// </summary>
        public int TotalChunks { get; set; }

        /// <summary>
        /// 已上传分片数
        /// </summary>
        public int UploadedChunks { get; set; }

        /// <summary>
        /// 上传进度（百分比）
        /// </summary>
        public decimal Progress { get; set; }

        /// <summary>
        /// 上传状态（0-进行中，1-已完成，2-已取消）
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 已上传的分片列表
        /// </summary>
        public List<int> UploadedChunkIndexes { get; set; } = new List<int>();

        /// <summary>
        /// 缺失的分片列表
        /// </summary>
        public List<int> MissingChunkIndexes { get; set; } = new List<int>();
    }
}