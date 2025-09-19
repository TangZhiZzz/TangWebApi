import * as signalR from '@microsoft/signalr'

class SignalRService {
  private connection: signalR.HubConnection | null = null
  private currentUser: string | null = null
  private eventListenersSetup = false

  // 连接到SignalR Hub
  async connect(): Promise<void> {
    if (this.connection) {
      await this.disconnect()
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5238/chathub')
      .withAutomaticReconnect()
      .build()

    await this.connection.start()
    console.log('SignalR连接已建立')
  }

  // 断开连接
  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
      this.currentUser = null
      this.eventListenersSetup = false
      console.log('SignalR连接已断开')
    }
  }

  // 设置用户名并加入大厅
  async setUser(userName: string): Promise<void> {
    if (!this.connection) {
      throw new Error('未连接到SignalR Hub')
    }

    this.currentUser = userName
    await this.connection.invoke('JoinHall', userName)
    console.log(`用户 ${userName} 已加入大厅`)
  }

  // 发送消息到大厅
  async sendToHall(message: string): Promise<void> {
    if (!this.connection) {
      throw new Error('未连接到SignalR Hub')
    }

    if (!this.currentUser) {
      throw new Error('用户未设置')
    }

    await this.connection.invoke('SendToHall', this.currentUser, message)
  }

  // 设置事件监听器（只调用一次）
  setupEventListeners(
    onMessage: (user: string, message: string) => void,
    onUserJoined: (user: string) => void,
    onUserLeft: (user: string) => void,
    onUsersUpdated: (users: string[]) => void
  ): void {
    if (!this.connection || this.eventListenersSetup) return
    
    this.connection.on('ReceiveMessage', onMessage)
    this.connection.on('UserJoined', onUserJoined)
    this.connection.on('UserLeft', onUserLeft)
    this.connection.on('UsersUpdated', onUsersUpdated)
    
    this.eventListenersSetup = true
    console.log('SignalR事件监听器已设置')
  }

  // 监听消息接收（保持向后兼容）
  onMessageReceived(callback: (user: string, message: string) => void): void {
    if (!this.connection) return
    this.connection.on('ReceiveMessage', callback)
  }

  // 监听用户加入（保持向后兼容）
  onUserJoined(callback: (user: string) => void): void {
    if (!this.connection) return
    this.connection.on('UserJoined', callback)
  }

  // 监听用户离开（保持向后兼容）
  onUserLeft(callback: (user: string) => void): void {
    if (!this.connection) return
    this.connection.on('UserLeft', callback)
  }

  // 监听在线用户更新（保持向后兼容）
  onUsersUpdated(callback: (users: string[]) => void): void {
    if (!this.connection) return
    this.connection.on('UsersUpdated', callback)
  }

  // 获取连接状态
  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected
  }

  // 获取当前用户
  getCurrentUser(): string | null {
    return this.currentUser
  }

  // 移除所有事件监听器
  offAll(): void {
    if (this.connection) {
      this.connection.off('ReceiveMessage')
      this.connection.off('UserJoined')
      this.connection.off('UserLeft')
      this.connection.off('UsersUpdated')
    }
  }
}

export const signalRService = new SignalRService()