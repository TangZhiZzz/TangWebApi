# TangWebApi 日志记录标准

本文档定义了 TangWebApi 项目的简化日志记录标准和最佳实践。

## 日志级别

### 标准日志级别
- **Information**: 一般信息，记录应用程序的正常运行状态
- **Warning**: 警告信息，表示潜在问题但不影响程序运行
- **Error**: 错误信息，表示发生了错误但程序可以继续运行
- **Critical**: 严重错误，可能导致程序崩溃或无法继续运行

### 使用场景
- **Information**: 应用启动/关闭、重要业务操作、API请求
- **Warning**: 配置问题、性能问题、不推荐的使用方式
- **Error**: 异常处理、业务逻辑错误、外部服务调用失败
- **Critical**: 数据库连接失败、关键服务不可用、安全问题

## 日志消息格式

### 基本格式
日志消息使用Serilog的默认格式，包含时间戳、级别、来源和消息内容。

### 示例
```
2024-01-15 10:30:45.123 [INF] 用户登录成功: user123
2024-01-15 10:30:46.456 [ERR] 数据库连接失败: Connection timeout
```

## 结构化日志

### 基本用法
使用 `LogStructured` 方法记录包含额外属性的日志：

```csharp
var properties = new Dictionary<string, object>
{
    { "UserId", "user123" },
    { "Action", "Login" }
};

_logger.LogStructured(LogLevel.Information, "用户登录", properties);
```

### 输出格式
结构化日志会自动包含提供的属性信息。

## API请求日志

### 使用方法
```csharp
_logger.LogApiRequest(
    method: "GET",
    path: "/api/users",
    statusCode: 200,
    duration: 150
);
```

### 输出格式
```
2024-01-15 10:30:45.123 [INF] API Request: GET /api/users - 200 (150ms)
```

### 自动记录
API请求日志通过 `RequestLoggingMiddleware` 自动记录基本的请求信息。

## 业务操作日志

### 使用方法
```csharp
_logger.LogBusinessOperation(
    operation: "CreateUser",
    success: true
);
```

### 输出格式
```
2024-01-15 10:30:45.123 [INF] Business Operation: CreateUser - Success
```

### 失败操作
```csharp
_logger.LogBusinessOperation(
    operation: "DeleteUser",
    success: false
);
```

输出：
```
2024-01-15 10:30:45.123 [WRN] Business Operation: DeleteUser - Failed
```

## 配置示例

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    },
    "FilePath": "logs/app-.log"
  }
}
```

## 日志文件结构

### 输出文件
1. **控制台输出** - 开发时查看日志
2. **logs/app-{date}.log** - 文件日志，包含所有日志信息

### 日志轮转策略
- **轮转周期**: 按天轮转（每天生成新文件）
- **保留策略**: 保留7天的日志文件
- **文件命名**: 使用日期后缀，如 `app-20240115.log`

## 最佳实践

### 1. 日志级别选择
- 生产环境建议使用 Information 级别
- 避免记录过多的调试信息

### 2. 消息内容
- 使用清晰、简洁的消息描述
- 包含必要的上下文信息
- 避免记录敏感信息（密码、令牌等）

### 3. 异常处理
- 记录异常时包含完整的异常信息
- 使用适当的日志级别（Error 或 Critical）
- 提供足够的上下文帮助问题诊断

### 4. 性能考虑
- 避免在高频调用的方法中记录过多日志
- 合理设置日志级别过滤不必要的日志

### 5. 简单有效
- 保持日志配置简单易懂
- 专注于记录重要的业务信息
- 避免过度复杂的日志结构