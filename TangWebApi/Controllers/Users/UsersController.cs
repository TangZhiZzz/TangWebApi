using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using TangWebApi.Entity;
using TangWebApi.Models;
using TangWebApi.Services;
using TangWebApi.Controllers.Users.Dtos;

namespace TangWebApi.Controllers.Users
{
    /// <summary>
    /// 用户控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly SqlSugarService _sqlSugarService;
        private readonly ISqlSugarClient _db;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sqlSugarService">SqlSugar服务</param>
        /// <param name="db">数据库客户端</param>
        public UsersController(SqlSugarService sqlSugarService, ISqlSugarClient db)
        {
            _sqlSugarService = sqlSugarService;
            _db = db;
        }

        /// <summary>
        /// 获取所有用户
        /// </summary>
        /// <param name="search">搜索参数</param>
        /// <returns>用户列表</returns>
        /// <response code="200">成功返回用户列表</response>
        [HttpGet]
        public async Task<PagedUserOutput> GetUsers([FromQuery] SearchUserInput search)
        {
            var query = _db.Queryable<User>();

            // 应用搜索条件
            if (!string.IsNullOrEmpty(search.Keyword))
            {
                query = query.Where(u => u.Username.Contains(search.Keyword) || u.Email.Contains(search.Keyword));
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == search.IsActive.Value);
            }

            // 获取总数
            var total = await query.CountAsync();

            // 分页查询
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((search.Page - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();

            var userOutputs = users.Select(u => new UserOutput
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }).ToList();

            return new PagedUserOutput
            {
                Users = userOutputs,
                Total = total,
                Page = search.Page,
                PageSize = search.PageSize,
                TotalPages = (int)Math.Ceiling((double)total / search.PageSize)
            };
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户信息</returns>
        /// <response code="200">成功返回用户信息</response>
        /// <response code="404">用户不存在</response>
        [HttpGet("{id}")]
        public async Task<UserDetailOutput> GetUser(int id)
        {
            var user = await _db.Queryable<User>().Where(u => u.Id == id).FirstAsync();
            if (user == null)
            {
                throw new ArgumentException($"用户不存在: {id}");
            }

            return new UserDetailOutput
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,
                LoginCount = user.LoginCount
            };
        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="input">用户信息</param>
        /// <returns>创建的用户信息</returns>
        /// <response code="201">成功创建用户</response>
        /// <response code="400">请求参数错误</response>
        [HttpPost]
        public async Task<CreateUserOutput> CreateUser([FromBody] CreateUserInput input)
        {
            // 验证输入
            if (string.IsNullOrEmpty(input.Username) || string.IsNullOrEmpty(input.Email) || string.IsNullOrEmpty(input.Password))
            {
                throw new ArgumentException("用户名、邮箱和密码不能为空");
            }

            if (input.Password != input.ConfirmPassword)
            {
                throw new ArgumentException("密码和确认密码不匹配");
            }

            // 检查用户名或邮箱是否已存在
            var existingUser = await _db.Queryable<User>()
                .Where(u => u.Username == input.Username || u.Email == input.Email)
                .FirstAsync();
            
            if (existingUser != null)
            {
                throw new ArgumentException("用户名或邮箱已存在");
            }

            // 创建新用户
            var user = new User
            {
                Username = input.Username,
                Email = input.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password),
                CreatedAt = DateTime.Now,
                IsActive = true,
                LoginCount = 0
            };
            
            var result = await _db.Insertable(user).ExecuteReturnEntityAsync();
            
            var userOutput = new UserOutput
            {
                Id = result.Id,
                Username = result.Username,
                Email = result.Email,
                IsActive = result.IsActive,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt
            };

            return new CreateUserOutput
            {
                Id = result.Id,
                Username = result.Username,
                Email = result.Email,
                IsActive = result.IsActive,
                CreatedAt = result.CreatedAt
            };
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="input">更新的用户信息</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task UpdateUser(int id, [FromBody] UpdateUserInput input)
        {
            var existingUser = await _db.Queryable<User>().Where(u => u.Id == id).FirstAsync();
            if (existingUser == null)
            {
                throw new ArgumentException($"用户不存在: {id}");
            }

            // 检查邮箱是否被其他用户使用
            if (!string.IsNullOrEmpty(input.Email) && input.Email != existingUser.Email)
            {
                var emailExists = await _db.Queryable<User>()
                    .Where(u => u.Email == input.Email && u.Id != id)
                    .AnyAsync();
                
                if (emailExists)
                {
                    throw new ArgumentException("邮箱已被其他用户使用");
                }
            }

            // 检查用户名是否被其他用户使用
            if (!string.IsNullOrEmpty(input.Username) && input.Username != existingUser.Username)
            {
                var usernameExists = await _db.Queryable<User>()
                    .Where(u => u.Username == input.Username && u.Id != id)
                    .AnyAsync();
                
                if (usernameExists)
                {
                    throw new ArgumentException("用户名已被其他用户使用");
                }
            }

            // 更新用户信息
            if (!string.IsNullOrEmpty(input.Username))
                existingUser.Username = input.Username;
            
            if (!string.IsNullOrEmpty(input.Email))
                existingUser.Email = input.Email;
            
            if (input.IsActive.HasValue)
                existingUser.IsActive = input.IsActive.Value;

            existingUser.UpdatedAt = DateTime.Now;
            
            await _db.Updateable(existingUser).ExecuteCommandAsync();
            
          
        }

        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="input">密码修改信息</param>
        [HttpPut("{id}/password")]
        public async Task ChangePassword(int id, [FromBody] ChangePasswordInput input)
        {
            var user = await _db.Queryable<User>().Where(u => u.Id == id).FirstAsync();
            if (user == null)
            {
                throw new ArgumentException($"用户不存在: {id}");
            }

            // 验证当前密码
            if (!BCrypt.Net.BCrypt.Verify(input.CurrentPassword, user.PasswordHash))
            {
                throw new ArgumentException("当前密码错误");
            }

            // 验证新密码
            if (input.NewPassword != input.ConfirmNewPassword)
            {
                throw new ArgumentException("新密码和确认密码不匹配");
            }

            // 更新密码
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);
            user.UpdatedAt = DateTime.Now;
            
            await _db.Updateable(user).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        [HttpDelete("{id}")]
        public async Task DeleteUser(int id)
        {
            var user = await _db.Queryable<User>().Where(u => u.Id == id).FirstAsync();
            if (user == null)
            {
                throw new ArgumentException($"用户不存在: {id}");
            }

            await _db.Deleteable<User>().Where(u => u.Id == id).ExecuteCommandAsync();
        }


        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="input">注册信息</param>
        /// <returns>注册结果</returns>
        /// <response code="200">注册成功</response>
        /// <response code="400">注册失败</response>
        [HttpPost("register")]
        public async Task<CreateUserOutput> Register([FromBody] CreateUserInput input)
        {
            // 验证输入
            if (string.IsNullOrEmpty(input.Username) || string.IsNullOrEmpty(input.Email) || string.IsNullOrEmpty(input.Password))
            {
                throw new ArgumentException("用户名、邮箱和密码不能为空");
            }

            // 验证密码确认
            if (input.Password != input.ConfirmPassword)
            {
                throw new ArgumentException("密码和确认密码不匹配");
            }

            // 检查用户名是否已存在
            var usernameExists = await _db.Queryable<User>()
                .Where(u => u.Username == input.Username)
                .AnyAsync();
            
            if (usernameExists)
            {
                throw new ArgumentException("用户名已存在");
            }

            // 检查邮箱是否已存在
            var emailExists = await _db.Queryable<User>()
                .Where(u => u.Email == input.Email)
                .AnyAsync();
            
            if (emailExists)
            {
                throw new ArgumentException("邮箱已被注册");
            }

            // 创建新用户
            var user = new User
            {
                Username = input.Username,
                Email = input.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password),
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var result = await _db.Insertable(user).ExecuteReturnEntityAsync();
            
            return new CreateUserOutput
            {
                Id = result.Id,
                Username = result.Username,
                Email = result.Email,
                IsActive = result.IsActive,
                CreatedAt = result.CreatedAt
            };
        }
    }
}