# TangChat SignalR 集成指南

## 概述

TangChat 是一个基于 Vue 3 和 SignalR 的实时聊天应用，提供了完整的实时通信功能，包括公开消息、私聊、群组聊天和系统通知。

## 功能特性

### 🚀 核心功能
- **实时连接管理** - 自动连接、重连和状态监控
- **多种消息类型** - 公开消息、私聊消息、群组消息
- **用户管理** - 在线用户列表、用户状态跟踪
- **群组功能** - 创建/加入/离开群组
- **输入状态** - 实时显示用户输入状态
- **系统通知** - 连接状态、用户活动等系统消息

### 🎨 用户界面
- **响应式设计** - 支持桌面和移动设备
- **现代化UI** - 美观的卡片式布局
- **实时状态指示** - 连接状态、在线用户、消息类型等
- **交互式组件** - 消息输入、用户列表、通知面板

## 项目结构

```
src/
├── components/
│   └── ChatRoom.vue          # 聊天室主组件
├── composables/
│   └── useSignalR.ts         # SignalR Vue 组合式函数
├── services/
│   └── signalrService.ts     # SignalR 连接服务
└── types/
    └── signalr.ts            # TypeScript 类型定义
```

## 功能特性

### 1. 实时消息传递
- 发送和接收实时消息
- 支持群组聊天
- 私人消息功能

### 2. 用户管理
- 在线用户列表
- 用户加入/离开通知
- 用户状态跟踪

### 3. 连接管理
- 自动重连机制
- 连接状态监控
- 错误处理和恢复

### 4. 群组功能
- 加入/离开聊天群组
- 群组消息广播
- 多群组支持

## 使用方法

### 1. 基本使用

```typescript
import { useSignalR } from '@/composables/useSignalR'

const {
  isConnected,
  messages,
  onlineUsers,
  connect,
  disconnect,
  sendMessage
} = useSignalR()

// 连接到 SignalR Hub
await connect()

// 发送消息
await sendMessage('用户名', '消息内容')
```

### 2. 在 Vue 组件中使用

```vue
<template>
  <div>
    <div v-if="isConnected">已连接</div>
    <div v-for="message in messages" :key="message.timestamp">
      {{ message.user }}: {{ message.message }}
    </div>
  </div>
</template>

<script setup>
import { useSignalR } from '@/composables/useSignalR'

const { isConnected, messages, connect } = useSignalR()

// 组件挂载时连接
onMounted(() => {
  connect()
})
</script>
```

### 3. 群组功能

```typescript
// 加入群组
await joinGroup('技术讨论')

// 离开群组
await leaveGroup('技术讨论')
```

## 配置选项

### SignalR 服务配置

在 `signalrService.ts` 中可以配置：

```typescript
const signalRService = new SignalRService('http://localhost:5000/chathub')
```

### 连接选项

- **hubUrl**: SignalR Hub 的 URL 地址
- **automaticReconnect**: 自动重连（默认启用）
- **logLevel**: 日志级别（Information）

## 服务端要求

为了使客户端正常工作，服务端需要实现以下 Hub 方法：

### 必需的 Hub 方法

```csharp
public class ChatHub : Hub
{
    // 发送消息
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    // 加入群组
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserJoined", Context.User.Identity.Name);
    }

    // 离开群组
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserLeft", Context.User.Identity.Name);
    }
}
```

### 客户端事件

客户端监听以下事件：

- `ReceiveMessage`: 接收消息
- `UserJoined`: 用户加入
- `UserLeft`: 用户离开

## 错误处理

项目包含完整的错误处理机制：

```typescript
try {
  await sendMessage('用户', '消息')
} catch (error) {
  if (error instanceof ConnectionError) {
    // 处理连接错误
  } else if (error instanceof MessageError) {
    // 处理消息错误
  }
}
```

## 开发建议

### 1. 环境配置

确保后端 SignalR Hub 正在运行在正确的端口（默认：5000）

### 2. CORS 配置

如果前后端在不同域名，需要配置 CORS：

```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .WithOrigins("http://localhost:5173") // Vite 默认端口
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});
```

### 3. 生产环境

在生产环境中，请更新 `signalrService.ts` 中的 Hub URL：

```typescript
const signalRService = new SignalRService('https://your-api-domain.com/chathub')
```

## 故障排除

### 常见问题

1. **连接失败**
   - 检查后端 SignalR Hub 是否正在运行
   - 验证 URL 是否正确
   - 检查 CORS 配置

2. **消息不显示**
   - 确认事件监听器已正确设置
   - 检查服务端 Hub 方法实现

3. **自动重连失败**
   - 检查网络连接
   - 验证服务端是否支持重连

### 调试技巧

启用详细日志：

```typescript
const connection = new HubConnectionBuilder()
  .withUrl(hubUrl)
  .configureLogging(LogLevel.Debug) // 启用调试日志
  .build()
```

## 扩展功能

项目架构支持轻松添加新功能：

- 文件传输
- 语音/视频通话
- 消息历史记录
- 用户认证
- 消息加密

## 依赖项

- `@microsoft/signalr`: ^8.0.0
- `vue`: ^3.5.21
- `typescript`: ~5.8.3