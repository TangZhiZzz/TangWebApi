using SqlSugar;

namespace TangWebApi.Entity;

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
