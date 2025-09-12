namespace TangWebApi.Options
{
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