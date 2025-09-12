using SqlSugar;
using TangWebApi.Entity;
using TangWebApi.Models;

namespace TangWebApi.Services
{
    /// <summary>
    /// SqlSugar数据库服务
    /// </summary>
    public class SqlSugarService
    {
        private readonly ISqlSugarClient _db;

        public SqlSugarService(ISqlSugarClient db)
        {
            _db = db;
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            try
            {
                // 创建数据库（如果不存在）
                _db.DbMaintenance.CreateDatabase();

                // 创建表（如果不存在）
                _db.CodeFirst.InitTables<User>();
                _db.CodeFirst.InitTables<TangWebApi.Entity.FileInfo>();

                // 插入测试数据
                await SeedDataAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库初始化失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 插入种子数据
        /// </summary>
        private async Task SeedDataAsync()
        {
            var userCount = await _db.Queryable<User>().CountAsync();
            if (userCount == 0)
            {
                var users = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        Email = "admin@example.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), // 默认密码123456
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    },
                    new User
                    {
                        Username = "testuser",
                        Email = "test@example.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), // 默认密码123456
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    }
                };

                await _db.Insertable(users).ExecuteCommandAsync();
                Console.WriteLine("种子数据插入成功");
            }
        }

        /// <summary>
        /// 获取数据库实例
        /// </summary>
        public ISqlSugarClient GetDatabase() => _db;
    }
}
