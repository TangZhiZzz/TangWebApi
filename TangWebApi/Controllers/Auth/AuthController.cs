using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using TangWebApi.Entity;
using TangWebApi.Models;
using TangWebApi.Services;

namespace TangWebApi;

/// <summary>
/// 认证控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISqlSugarClient _db;
    private readonly JwtService _jwtService;

    public AuthController(ISqlSugarClient db, JwtService jwtService)
    {
        _db = db;
        _jwtService = jwtService;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="input">登录请求</param>
    /// <returns>登录结果</returns>
    [HttpPost("login")]
    public async Task<LoginOutput> Login([FromBody] LoginInput input)
    {
        if (string.IsNullOrEmpty(input.Email) || string.IsNullOrEmpty(input.Password))
        {
            throw new ArgumentException("邮箱和密码不能为空");
        }

        // 查找用户
        var user = await _db.Queryable<User>()
            .FirstAsync(u => u.Email == input.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException("用户不存在");
        }

        // 验证密码使用BCrypt哈希验证
        if (!BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("密码错误");
        }

        // 生成JWT令牌
        var token = _jwtService.GenerateToken(user.Id, user.Username, user.Email, new[] { "User" });

        return new LoginOutput
        {
            Token = token,
            User = new UserInfo
            {
                Id = user.Id,
                Name = user.Username,
                Email = user.Email
            }
        };
    }

    /// <summary>
    /// 验证令牌
    /// </summary>
    /// <returns>当前用户信息</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<UserInfo> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("无效的令牌");
        }

        var user = await _db.Queryable<User>()
            .FirstAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new KeyNotFoundException("用户不存在");
        }

        return new UserInfo
        {
            Id = user.Id,
            Name = user.Username,
            Email = user.Email
        };
    }

    /// <summary>
    /// 刷新令牌
    /// </summary>
    /// <returns>新的令牌</returns>
    [HttpPost("refresh")]
    [Authorize]
    public async Task<RefreshTokenOutput> RefreshToken()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("无效的令牌");
        }

        var user = await _db.Queryable<User>()
            .FirstAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new KeyNotFoundException("用户不存在");
        }

        // 生成新的JWT令牌
        var token = _jwtService.GenerateToken(user.Id, user.Username, user.Email, new[] { "User" });

        return new RefreshTokenOutput { Token = token };
    }
}




