# TangWebApi

一个功能丰富的 ASP.NET Core Web API 项目，集成了身份验证、文件管理、缓存、消息队列、健康检查和系统信息服务等功能。

## 🚀 主要特性

### 🔐 身份验证与授权
- JWT Token 身份验证
- 用户注册、登录、刷新令牌
- 基于角色的访问控制

### 📁 文件管理
- 文件上传、下载、删除
- 支持多种文件格式
- 文件大小和类型验证
- 安全的文件存储
- **分片上传与断点续传**
  - 大文件分片上传
  - 断点续传支持
  - 上传进度跟踪
  - 文件完整性验证
  - 自动清理过期分片

### 🗄️ 缓存服务
- Redis 分布式缓存
- 内存缓存
- 缓存过期策略
- 缓存统计信息

### 📧 邮件服务
- SMTP 邮件发送
- HTML 和纯文本邮件
- 邮件模板支持
- 异步邮件发送

### 📊 消息队列
- RabbitMQ 集成
- 消息发布和订阅
- 队列管理
- 消息持久化

### 🔍 健康检查
- 应用程序健康状态监控
- 数据库连接检查
- 外部服务依赖检查
- 详细的健康报告

### 📈 系统信息
- CPU 使用率监控
- 内存使用情况
- 磁盘空间信息
- 系统运行时间

### 📝 日志记录
- Serilog 结构化日志
- 多种日志输出目标
- 日志级别配置
- 请求/响应日志记录

## 🛠️ 技术栈

- **框架**: ASP.NET Core 8.0
- **数据库**: SqlSugar ORM
- **缓存**: Redis / 内存缓存
- **消息队列**: RabbitMQ
- **日志**: Serilog
- **文档**: Swagger/OpenAPI
- **身份验证**: JWT Bearer Token

## 📦 项目结构

```
TangWebApi/
├── Controllers/          # API 控制器
│   ├── FileController.cs        # 文件管理控制器
│   ├── ChunkUploadController.cs # 分片上传控制器
│   └── ...
├── Services/            # 业务服务层
│   ├── IChunkUploadService.cs   # 分片上传服务接口
│   ├── ChunkUploadService.cs    # 分片上传服务实现
│   ├── ChunkCleanupService.cs   # 分片清理后台服务
│   └── ...
├── Models/              # 数据模型
├── Extensions/          # 扩展方法
├── Middleware/          # 中间件
├── Filter/              # 过滤器
└── wwwroot/            # 静态文件
    └── chunk-upload-demo.html   # 分片上传演示页面
```

## 🚀 快速开始

### 环境要求
- .NET 8.0 SDK
- Redis (可选)
- RabbitMQ (可选)

### 安装和运行

1. 克隆项目
```bash
git clone https://github.com/TangZhiZzz/TangWebApi.git
cd TangWebApi
```

2. 还原依赖
```bash
dotnet restore
```

3. 配置设置
编辑 `appsettings.json` 文件，配置数据库连接字符串、Redis、RabbitMQ 等服务。

4. 运行项目
```bash
dotnet run
```

5. 访问 API 文档
打开浏览器访问: `https://localhost:5001/swagger`

## ⚙️ 配置说明

### JWT 配置
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "TangWebApi",
    "Audience": "TangWebApi-Users",
    "ExpirationInMinutes": 60
  }
}
```

### Redis 配置
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

### RabbitMQ 配置
```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  }
}
```

### 文件上传配置
```json
{
  "FileSettings": {
    "UploadPath": "uploads",
    "MaxFileSize": 104857600,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx"],
    "ChunkSize": 1048576,
    "ChunkTempPath": "temp/chunks",
    "ChunkRetentionHours": 24
  }
}
```

## 📚 API 文档

项目集成了 Swagger UI，启动项目后可以通过以下地址访问：
- 开发环境: `https://localhost:5001/swagger`
- 生产环境: `https://your-domain/swagger`

### 分片上传演示
启动项目后可以通过以下地址访问分片上传演示页面：
- 开发环境: `https://localhost:5001/chunk-upload-demo.html`
- 生产环境: `https://your-domain/chunk-upload-demo.html`

## 🧪 测试

```bash
# 运行单元测试
dotnet test

# 运行集成测试
dotnet test --filter Category=Integration
```

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🤝 贡献

欢迎提交 Pull Request 和 Issue！

## 📞 联系方式

如有问题或建议，请通过以下方式联系：
- GitHub Issues: [https://github.com/TangZhiZzz/TangWebApi/issues](https://github.com/TangZhiZzz/TangWebApi/issues)

---

⭐ 如果这个项目对你有帮助，请给它一个星标！