using System.ComponentModel.DataAnnotations;

namespace TangWebApi.Models
{
    /// <summary>
    /// JWT配置设置
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// JWT密钥
        /// </summary>
        [Required]
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// 发行者
        /// </summary>
        [Required]
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// 受众
        /// </summary>
        [Required]
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// 过期时间（分钟）
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "过期时间必须大于0")]
        public int ExpiryInMinutes { get; set; } = 60;
    }
}