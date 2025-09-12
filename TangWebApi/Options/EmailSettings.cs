namespace TangWebApi.Options
{
    /// <summary>
    /// 邮件配置模型
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// SMTP服务器地址
        /// </summary>
        public required string SmtpServer { get; set; }

        /// <summary>
        /// SMTP端口
        /// </summary>
        public int SmtpPort { get; set; } = 587;

        /// <summary>
        /// 是否启用SSL
        /// </summary>
        public bool EnableSsl { get; set; } = true;

        /// <summary>
        /// 发件人邮箱
        /// </summary>
        public required string FromEmail { get; set; }

        /// <summary>
        /// 发件人名称
        /// </summary>
        public required string FromName { get; set; }

        /// <summary>
        /// 邮箱用户名
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// 邮箱密码
        /// </summary>
        public required string Password { get; set; }
    }
}