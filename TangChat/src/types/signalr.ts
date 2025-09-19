// SignalR 相关类型定义

export interface ChatMessage {
  user: string
  message: string
  timestamp: Date
}

export interface ChatUser {
  id: string
  name: string
  isOnline: boolean
  lastSeen?: Date
}

export interface ChatGroup {
  id: string
  name: string
  members: ChatUser[]
  createdAt: Date
}

export interface SignalRConnectionState {
  isConnected: boolean
  connectionId?: string
  lastConnected?: Date
  reconnectAttempts: number
}

export interface SignalREvents {
  // 消息相关事件
  ReceiveMessage: (user: string, message: string) => void
  ReceivePrivateMessage: (fromUser: string, toUser: string, message: string) => void
  
  // 用户相关事件
  UserJoined: (user: string) => void
  UserLeft: (user: string) => void
  UserTyping: (user: string, isTyping: boolean) => void
  
  // 群组相关事件
  GroupJoined: (user: string, groupName: string) => void
  GroupLeft: (user: string, groupName: string) => void
  GroupMessage: (groupName: string, user: string, message: string) => void
  
  // 系统事件
  SystemNotification: (message: string, type: 'info' | 'warning' | 'error') => void
  ConnectionStatusChanged: (isConnected: boolean) => void
}

export interface SignalRMethods {
  // 发送消息
  SendMessage: (user: string, message: string) => Promise<void>
  SendPrivateMessage: (toUser: string, message: string) => Promise<void>
  
  // 群组操作
  JoinGroup: (groupName: string) => Promise<void>
  LeaveGroup: (groupName: string) => Promise<void>
  SendGroupMessage: (groupName: string, message: string) => Promise<void>
  
  // 用户操作
  SetUserTyping: (isTyping: boolean) => Promise<void>
  GetOnlineUsers: () => Promise<string[]>
  
  // 系统操作
  Ping: () => Promise<string>
}

export interface SignalRConfig {
  hubUrl: string
  automaticReconnect?: boolean
  reconnectIntervals?: number[]
  logLevel?: 'trace' | 'debug' | 'info' | 'warn' | 'error' | 'critical' | 'none'
  accessTokenFactory?: () => string | Promise<string>
}

export interface ChatRoomState {
  messages: ChatMessage[]
  onlineUsers: ChatUser[]
  currentUser?: ChatUser
  currentGroup?: string
  isTyping: boolean
  typingUsers: string[]
}

// 错误类型
export class SignalRError extends Error {
  constructor(
    message: string,
    code?: string,
    details?: any
  ) {
    super(message)
    this.name = 'SignalRError'
  }
}

export class ConnectionError extends SignalRError {
  constructor(message: string, details?: any) {
    super(message, 'CONNECTION_ERROR', details)
    this.name = 'ConnectionError'
  }
}

export class MessageError extends SignalRError {
  constructor(message: string, details?: any) {
    super(message, 'MESSAGE_ERROR', details)
    this.name = 'MessageError'
  }
}