# TangWebApi 扩展方法说明

本项目通过扩展方法将服务配置和中间件配置进行了模块化封装，提高了代码的可维护性和可读性。

## 扩展方法结构

### ServiceCollectionExtensions.cs
服务集合扩展方法，用于配置依赖注入服务：

- `AddSqlSugarService(IConfiguration)` - 配置SqlSugar ORM服务
- `AddSwaggerService()` - 配置Swagger API文档服务
- `AddCorsService()` - 配置CORS跨域服务
- `AddJwtAuthenticationService(IConfiguration)` - 配置JWT认证服务（预留）

### ApplicationBuilderExtensions.cs
应用程序构建器扩展方法，用于配置中间件管道：

- `UseSwaggerService()` - 配置Swagger中间件
- `UseKnife4jService()` - 配置Knife4j中间件
- `UseCorsService()` - 配置CORS中间件
- `InitializeDatabaseAsync()` - 异步初始化数据库
- `UseDevelopmentEnvironment(IWebHostEnvironment)` - 配置开发环境中间件

## 使用示例

### Program.cs 中的使用

```csharp
using TangWebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// 添加自定义服务
builder.Services.AddSqlSugarService(builder.Configuration);
builder.Services.AddSwaggerService();
builder.Services.AddCorsService();
builder.Services.AddJwtAuthenticationService(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseDevelopmentEnvironment(app.Environment);

// 配置静态文件服务（为Knife4j提供支持）
app.UseStaticFiles();

// 配置CORS
app.UseCorsService();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// 初始化数据库
await app.InitializeDatabaseAsync();

app.Run();
```

## 优势

1. **模块化**: 将相关的配置逻辑封装在一起
2. **可重用**: 扩展方法可以在多个项目中重复使用
3. **可维护**: 配置逻辑集中管理，便于维护和修改
4. **可读性**: Program.cs 文件更加简洁清晰
5. **可测试**: 每个扩展方法都可以独立测试

## 扩展建议

可以根据项目需要继续添加其他服务的扩展方法，如：
- Redis缓存服务
- 日志服务
- 消息队列服务
- 数据库迁移服务
- 健康检查服务

每个扩展方法都应该遵循单一职责原则，只负责配置特定的服务或中间件。