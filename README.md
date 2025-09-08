# TangWebApi

一个功能丰富的 ASP.NET Core Web API 项目，提供了完整的企业级应用开发基础设施。

## 🚀 项目特性

### 核心功能
- **用户认证与授权** - JWT Token 认证系统
- **文件管理** - 文件上传、下载、管理服务
- **邮件服务** - 支持邮件发送功能
- **缓存系统** - 支持 Redis 和内存缓存
- **消息队列** - RabbitMQ 消息队列集成
- **健康检查** - 应用程序健康状态监控
- **系统信息** - 系统运行状态和信息获取
- **日志记录** - Serilog 结构化日志

### 技术栈
- **.NET 8** - 最新的 .NET 框架
- **ASP.NET Core** - Web API 框架
- **SqlSugar** - ORM 数据访问层
- **SQLite** - 轻量级数据库
- **Redis** - 分布式缓存
- **RabbitMQ** - 消息队列
- **Serilog** - 结构化日志
- **Swagger** - API 文档
- **JWT** - 身份验证

## 📁 项目结构

```
TangWebApi/
├── Controllers/          # API 控制器
│   ├── AuthController.cs        # 认证控制器
│   ├── CacheController.cs       # 缓存控制器
│   ├── EmailController.cs       # 邮件控制器
│   ├── FileController.cs        # 文件控制器
│   ├── HealthCheckController.cs # 健康检查控制器
│   ├── LoggingController.cs     # 日志控制器
│   ├── MessageQueueController.cs# 消息队列控制器
│   ├── SystemInfoController.cs  # 系统信息控制器
│   └── UsersController.cs       # 用户控制器
├── Services/             # 业务服务层
├── Models/              # 数据模型
├── Extensions/          # 扩展方法
├── Filter/              # 过滤器
├── Middleware/          # 中间件
└── wwwroot/            # 静态文件
```

## 🛠️ 安装和配置

### 环境要求
- .NET 8 SDK
- Redis (可选，用于分布式缓存)
- RabbitMQ (可选，用于消息队列)

### 1. 克隆项目
```bash
git clone <repository-url>
cd TangWebApi
```

### 2. 还原依赖包
```bash
cd TangWebApi
dotnet restore
```

### 3. 配置文件

编辑 `appsettings.json` 配置文件：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=TangWebApi.db"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "TangWebApi",
    "Audience": "TangWebApi-Users",
    "ExpiryMinutes": 60
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  },
  "Email": {
    "SmtpServer": "smtp.example.com",
    "Port": 587,
    "Username": "your-email@example.com",
    "Password": "your-password"
  }
}
```

### 4. 数据库初始化
项目使用 SQLite 数据库，首次运行时会自动创建数据库和种子数据。

### 5. 运行项目
```bash
dotnet run
```

项目将在 `http://localhost:5238` 启动。

## 📖 使用说明

### API 文档
启动项目后，访问 Swagger 文档：
- **开发环境**: `http://localhost:5238/swagger`

### 主要 API 端点

#### 认证相关
- `POST /api/auth/login` - 用户登录
- `POST /api/auth/register` - 用户注册
- `POST /api/auth/refresh` - 刷新 Token

#### 文件管理
- `POST /api/file/upload` - 文件上传
- `GET /api/file/download/{id}` - 文件下载
- `GET /api/file/list` - 文件列表
- `DELETE /api/file/{id}` - 删除文件

#### 缓存操作
- `GET /api/cache/{key}` - 获取缓存
- `POST /api/cache` - 设置缓存
- `DELETE /api/cache/{key}` - 删除缓存

#### 系统信息
- `GET /api/systeminfo` - 获取系统信息
- `GET /api/systeminfo/performance` - 获取性能信息

#### 健康检查
- `GET /api/health` - 健康检查
- `GET /api/health/detailed` - 详细健康检查

### 认证使用

1. **获取 Token**:
```bash
curl -X POST "http://localhost:5238/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password"}'
```

2. **使用 Token 访问受保护的 API**:
```bash
curl -X GET "http://localhost:5238/api/users" \
  -H "Authorization: Bearer your-jwt-token"
```

## 🔧 开发指南

### 添加新的控制器
1. 在 `Controllers` 文件夹中创建新的控制器
2. 继承 `ControllerBase` 并添加 `[ApiController]` 特性
3. 使用依赖注入获取所需服务

### 添加新的服务
1. 在 `Services` 文件夹中创建接口和实现
2. 在 `ServiceCollectionExtensions.cs` 中注册服务
3. 在控制器中通过构造函数注入使用

### 配置缓存
项目支持两种缓存方式：
- **内存缓存**: 默认启用
- **Redis 缓存**: 需要配置 Redis 连接字符串

### 日志配置
使用 Serilog 进行日志记录，日志文件保存在 `logs` 目录中。

## 🧹 项目清理

项目提供了清理脚本来清理编译产物和临时文件：

### Windows
```bash
# PowerShell 版本
.\clean.ps1

# 批处理版本
.\clean.bat
```

清理脚本会删除：
- 编译产物 (bin, obj 目录)
- 临时文件
- 日志文件
- 可选：数据库文件、上传文件、NuGet 缓存

## 🚀 部署

### 发布应用
```bash
dotnet publish -c Release -o ./publish
```

### Docker 部署
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY ./publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "TangWebApi.dll"]
```

## 🤝 贡献

1. Fork 项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 📞 联系方式

如有问题或建议，请通过以下方式联系：
- 创建 Issue
- 发送邮件到 [tjfzeishuai@163.com]

## 🙏 致谢

感谢所有为这个项目做出贡献的开发者！