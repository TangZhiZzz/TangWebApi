using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using TangWebApi.Entity;
using TangWebApi.Models;
using TangWebApi.Services;

namespace TangWebApi.Controllers
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
        /// <returns>用户列表</returns>
        /// <response code="200">成功返回用户列表</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<User>), 200)]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _db.Queryable<User>().ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户信息</returns>
        /// <response code="200">成功返回用户信息</response>
        /// <response code="404">用户不存在</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _db.Queryable<User>().Where(u => u.Id == id).FirstAsync();
                if (user == null)
                {
                    return NotFound(new { Error = $"用户不存在: {id}" });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns>创建的用户信息</returns>
        /// <response code="201">成功创建用户</response>
        /// <response code="400">请求参数错误</response>
        [HttpPost]
        [ProducesResponseType(typeof(User), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email))
                {
                    return BadRequest(new { Error = "用户名和邮箱不能为空" });
                }

                // 检查用户名是否已存在
                var existingUser = await _db.Queryable<User>()
                    .Where(u => u.Username == user.Username || u.Email == user.Email)
                    .FirstAsync();
                
                if (existingUser != null)
                {
                    return BadRequest(new { Error = "用户名或邮箱已存在" });
                }

                user.CreatedAt = DateTime.Now;
                user.IsActive = true;
                
                var result = await _db.Insertable(user).ExecuteReturnEntityAsync();
                return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="user">更新的用户信息</param>
        /// <returns>更新结果</returns>
        /// <response code="200">成功更新用户</response>
        /// <response code="404">用户不存在</response>
        /// <response code="400">请求参数错误</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            try
            {
                var existingUser = await _db.Queryable<User>().Where(u => u.Id == id).FirstAsync();
                if (existingUser == null)
                {
                    return NotFound(new { Error = $"用户不存在: {id}" });
                }

                user.Id = id;
                user.UpdatedAt = DateTime.Now;
                user.CreatedAt = existingUser.CreatedAt; // 保持原创建时间
                
                await _db.Updateable(user).ExecuteCommandAsync();
                
                var updatedUser = await _db.Queryable<User>().Where(u => u.Id == id).FirstAsync();
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>删除结果</returns>
        /// <response code="204">成功删除用户</response>
        /// <response code="404">用户不存在</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _db.Queryable<User>().Where(u => u.Id == id).FirstAsync();
                if (user == null)
                {
                    return NotFound(new { Error = $"用户不存在: {id}" });
                }

                await _db.Deleteable<User>().Where(u => u.Id == id).ExecuteCommandAsync();
                return Ok(new { Message = "删除用户成功", Id = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// 搜索用户
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns>匹配的用户列表</returns>
        /// <response code="200">成功返回搜索结果</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<User>), 200)]
        public async Task<IActionResult> SearchUsers([FromQuery] string keyword)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    var allUsers = await _db.Queryable<User>().ToListAsync();
                    return Ok(allUsers);
                }

                var users = await _db.Queryable<User>()
                    .Where(u => u.Username.Contains(keyword) || u.Email.Contains(keyword))
                    .ToListAsync();
                
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}