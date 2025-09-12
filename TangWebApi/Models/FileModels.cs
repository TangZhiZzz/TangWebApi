using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace TangWebApi.Models
{
    

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
    }
}