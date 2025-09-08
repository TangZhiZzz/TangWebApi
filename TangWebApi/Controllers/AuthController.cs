using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using TangWebApi.Models;
using TangWebApi.Services;

namespace TangWebApi.Controllers
{
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
        /// <param name="request">登录请求</param>
        /// <returns>登录结果</returns>
        [HttpPost("login")]
        public async Task<LoginResponse> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentException("邮箱和密码不能为空");
            }

            // 查找用户
            var user = await _db.Queryable<User>()
                .FirstAsync(u => u.Email == request.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException("用户不存在");
            }

            // 验证密码使用BCrypt哈希验证
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("密码错误");
            }

            // 生成JWT令牌
            var token = _jwtService.GenerateToken(user.Id, user.Username, user.Email, new[] { "User" });

            return new LoginResponse
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
        public async Task<RefreshTokenResponse> RefreshToken()
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

            return new RefreshTokenResponse { Token = token };
        }
    }

    /// <summary>
    /// 登录请求模型
    /// </summary>
    public class LoginRequest
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

    /// <summary>
    /// 登录响应模型
    /// </summary>
    public class LoginResponse
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
    public class RefreshTokenResponse
    {
        /// <summary>
        /// JWT令牌
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
}