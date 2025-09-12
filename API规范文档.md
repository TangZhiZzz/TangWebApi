# TangWebApi 接口规范文档

## 概述

本文档描述了 TangWebApi 项目中认证（Auth）和用户管理（Users）模块的接口规范，包括接口设计原则、数据模型、错误处理等内容。

## 设计原则

### 1. RESTful API 设计
- 使用标准的 HTTP 方法（GET、POST、PUT、DELETE）
- 资源导向的 URL 设计
- 统一的响应格式

### 2. 错误处理
- 使用异常抛出机制，而非返回错误状态码
- 统一的异常类型：`ArgumentException`、`UnauthorizedAccessException`、`KeyNotFoundException`
- 中文错误消息，便于理解

### 3. 数据传输对象（DTO）
- 输入和输出分离，使用专门的 Input 和 Output 模型
- 移除不必要的 Message 字段，保持数据结构简洁
- 强类型返回，避免使用 `IActionResult`

## 认证模块（Auth）

### 基础路径
```
/api/Auth
```

### 接口列表

#### 1. 用户登录

**接口地址：** `POST /api/Auth/login`

**请求模型：** `LoginInput`
```csharp
public class LoginInput
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
```

**响应模型：** `LoginOutput`
```csharp
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
```

**业务逻辑：**
- 验证邮箱和密码不能为空
- 使用 BCrypt 验证密码
- 生成 JWT 令牌
- 返回用户基本信息

**异常处理：**
- `ArgumentException`: 邮箱和密码不能为空
- `UnauthorizedAccessException`: 用户不存在或密码错误

#### 2. 获取当前用户信息

**接口地址：** `GET /api/Auth/me`

**认证要求：** 需要 JWT 令牌

**响应模型：** `UserInfo`

**业务逻辑：**
- 从 JWT 令牌中提取用户ID
- 查询用户信息
- 返回用户基本信息

**异常处理：**
- `UnauthorizedAccessException`: 无效的令牌
- `KeyNotFoundException`: 用户不存在

#### 3. 刷新令牌

**接口地址：** `POST /api/Auth/refresh`

**认证要求：** 需要 JWT 令牌

**响应模型：** `RefreshTokenOutput`
```csharp
public class RefreshTokenOutput
{
    /// <summary>
    /// JWT令牌
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
```

**业务逻辑：**
- 验证当前令牌有效性
- 生成新的 JWT 令牌
- 返回新令牌

## 用户管理模块（Users）

### 基础路径
```
/api/Users
```

### 认证要求
所有接口都需要 JWT 令牌认证（除注册接口）

### 接口列表

#### 1. 获取用户列表

**接口地址：** `GET /api/Users`

**查询参数：** `SearchUserInput`
```csharp
public class SearchUserInput
{
    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 是否只显示激活用户
    /// </summary>
    public bool? IsActive { get; set; }
}
```

**响应模型：** `PagedUserOutput`
```csharp
public class PagedUserOutput
{
    /// <summary>
    /// 用户列表
    /// </summary>
    public List<UserOutput> Users { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }
}
```

#### 2. 获取用户详情

**接口地址：** `GET /api/Users/{id}`

**路径参数：**
- `id`: 用户ID

**响应模型：** `UserDetailOutput`
```csharp
public class UserDetailOutput : UserOutput
{
    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// 登录次数
    /// </summary>
    public int LoginCount { get; set; }
}
```

#### 3. 创建用户

**接口地址：** `POST /api/Users`

**请求模型：** `CreateUserInput`
```csharp
public class CreateUserInput
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 确认密码
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

**响应模型：** `CreateUserOutput`
```csharp
public class CreateUserOutput
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
```

#### 4. 更新用户信息

**接口地址：** `PUT /api/Users/{id}`

**请求模型：** `UpdateUserInput`
```csharp
public class UpdateUserInput
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool? IsActive { get; set; }
}
```

**返回类型：** `void`

#### 5. 修改密码

**接口地址：** `PUT /api/Users/{id}/password`

**请求模型：** `ChangePasswordInput`
```csharp
public class ChangePasswordInput
{
    /// <summary>
    /// 当前密码
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新密码
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// 确认新密码
    /// </summary>
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
```

**返回类型：** `void`

#### 6. 删除用户

**接口地址：** `DELETE /api/Users/{id}`

**返回类型：** `void`

#### 7. 用户注册

**接口地址：** `POST /api/Users/register`

**认证要求：** 无需认证

**请求模型：** `CreateUserInput`

**响应模型：** `CreateUserOutput`

## 数据模型

### 用户实体（User）

```csharp
[SugarTable("Users")]
public class User
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [SugarColumn(Length = 50, IsNullable = false)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [SugarColumn(Length = 100, IsNullable = false)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希
    /// </summary>
    [SugarColumn(Length = 255, IsNullable = false)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public bool IsActive { get; set; } = true;
}
```

## 安全规范

### 1. 密码处理
- 使用 BCrypt 进行密码哈希
- 密码不以明文形式存储或传输
- 密码修改需要验证当前密码

### 2. JWT 令牌
- 所有需要认证的接口都需要在 Header 中携带 JWT 令牌
- 令牌格式：`Authorization: Bearer {token}`
- 令牌包含用户ID、用户名、邮箱等基本信息

### 3. 数据验证
- 所有输入数据都进行严格验证
- 用户名和邮箱唯一性检查
- 密码确认验证

## 错误处理规范

### 异常类型

1. **ArgumentException**
   - 用于参数验证失败
   - 示例："用户名、邮箱和密码不能为空"

2. **UnauthorizedAccessException**
   - 用于认证失败
   - 示例："用户不存在"、"密码错误"

3. **KeyNotFoundException**
   - 用于资源不存在
   - 示例："用户不存在"

### 错误消息
- 使用中文错误消息
- 错误消息应该清晰、具体
- 不暴露敏感的系统信息

## 版本控制

- API 版本：v1
- 向后兼容原则
- 重大变更需要版本升级

## 性能优化

### 分页查询
- 默认每页10条记录
- 支持自定义页面大小
- 返回总数和总页数信息

### 数据库查询
- 使用 SqlSugar ORM
- 支持条件查询和模糊搜索
- 按创建时间倒序排列

## 总结

本规范文档定义了 TangWebApi 项目中认证和用户管理模块的接口标准，包括：

1. **统一的接口设计**：RESTful 风格，强类型返回
2. **完善的错误处理**：异常驱动，中文错误消息
3. **安全的认证机制**：JWT 令牌，BCrypt 密码哈希
4. **清晰的数据模型**：输入输出分离，结构简洁
5. **良好的性能设计**：分页查询，条件搜索

遵循本规范可以确保 API 的一致性、安全性和可维护性。