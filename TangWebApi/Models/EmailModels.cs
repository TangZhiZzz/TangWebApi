using System.ComponentModel.DataAnnotations;

namespace TangWebApi.Models
{
    /// <summary>
    /// 邮件发送请求模型
    /// </summary>
    public class SendEmailRequest
    {
        /// <summary>
        /// 收件人邮箱地址
        /// </summary>
        [Required(ErrorMessage = "收件人邮箱地址不能为空")]
        [EmailAddress(ErrorMessage = "邮箱地址格式不正确")]
        public required string To { get; set; }

        /// <summary>
        /// 抄送邮箱地址列表
        /// </summary>
        public List<string>? Cc { get; set; }

        /// <summary>
        /// 密送邮箱地址列表
        /// </summary>
        public List<string>? Bcc { get; set; }

        /// <summary>
        /// 邮件主题
        /// </summary>
        [Required(ErrorMessage = "邮件主题不能为空")]
        public required string Subject { get; set; }

        /// <summary>
        /// 邮件内容
        /// </summary>
        [Required(ErrorMessage = "邮件内容不能为空")]
        public required string Body { get; set; }

        /// <summary>
        /// 是否为HTML格式
        /// </summary>
        public bool IsHtml { get; set; } = true;

        /// <summary>
        /// 附件文件路径列表
        /// </summary>
        public List<string>? Attachments { get; set; }
    }

    /// <summary>
    /// 模板邮件发送请求模型
    /// </summary>
    public class SendTemplateEmailRequest
    {
        /// <summary>
        /// 收件人邮箱地址
        /// </summary>
        [Required(ErrorMessage = "收件人邮箱地址不能为空")]
        [EmailAddress(ErrorMessage = "邮箱地址格式不正确")]
        public required string To { get; set; }

        /// <summary>
        /// 模板ID
        /// </summary>
        [Required(ErrorMessage = "模板ID不能为空")]
        public required string TemplateId { get; set; }

        /// <summary>
        /// 模板参数
        /// </summary>
        public Dictionary<string, object>? Parameters { get; set; }

        /// <summary>
        /// 抄送邮箱地址列表
        /// </summary>
        public List<string>? Cc { get; set; }

        /// <summary>
        /// 密送邮箱地址列表
        /// </summary>
        public List<string>? Bcc { get; set; }
    }

    /// <summary>
    /// 邮件模板模型
    /// </summary>
    public class EmailTemplate
    {
        /// <summary>
        /// 模板ID
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 邮件主题模板
        /// </summary>
        public required string Subject { get; set; }

        /// <summary>
        /// 邮件内容模板
        /// </summary>
        public required string Body { get; set; }

        /// <summary>
        /// 是否为HTML格式
        /// </summary>
        public bool IsHtml { get; set; } = true;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 邮件发送结果模型
    /// </summary>
    public class EmailSendResult
    {
        /// <summary>
        /// 是否发送成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息ID
        /// </summary>
        public string? MessageId { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SentAt { get; set; } = DateTime.Now;
    }



    /// <summary>
    /// 邮件连接测试结果模型
    /// </summary>
    public class EmailConnectionTestResult
    {
        /// <summary>
        /// 是否连接成功
        /// </summary>
        public bool Connected { get; set; }

        /// <summary>
        /// 测试时间
        /// </summary>
        public DateTime TestedAt { get; set; }
    }
}