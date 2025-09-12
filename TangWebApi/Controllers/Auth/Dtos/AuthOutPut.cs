namespace TangWebApi;


/// <summary>
/// 登录响应模型
/// </summary>
public class LoginOutput
{
    /// <summary>
    /// JWT令牌
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 用户信息
    /// </summary>
    public UserInfo User { get; set; } = new();
}

/// <summary>
/// 用户信息模型
/// </summary>
public class UserInfo
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
/// <summary>
/// 刷新令牌响应模型
/// </summary>
public class RefreshTokenOutput
{
    /// <summary>
    /// JWT令牌
    /// </summary>
    public string Token { get; set; } = string.Empty;
}