namespace TangWebApi;

/// <summary>
/// 登录请求模型
/// </summary>
public class LoginInput
{
    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
