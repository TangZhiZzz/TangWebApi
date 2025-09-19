import { ref, computed } from 'vue'
import { signalRService } from '../services/signalrService'

// 连接状态
const connectionState = ref({
  isConnected: false,
  isConnecting: false,
  error: null as string | null
})

// 计算属性
const isConnected = computed(() => connectionState.value.isConnected)
const isConnecting = computed(() => connectionState.value.isConnecting)

// 事件回调
const messageCallbacks = new Set<(user: string, message: string) => void>()
const userJoinedCallbacks = new Set<(user: string) => void>()
const userLeftCallbacks = new Set<(user: string) => void>()
const usersUpdatedCallbacks = new Set<(users: string[]) => void>()

// 标记是否已经设置了事件监听器
let eventListenersSetup = false

export function useSignalR() {
  // 设置事件监听器（只设置一次）
  const setupEventListeners = () => {
    if (eventListenersSetup) return
    
    // 使用新的setupEventListeners方法
    signalRService.setupEventListeners(
      (user: string, message: string) => {
        console.log('收到消息:', user, message)
        messageCallbacks.forEach(cb => cb(user, message))
      },
      (user: string) => {
        console.log('用户加入:', user)
        userJoinedCallbacks.forEach(cb => cb(user))
      },
      (user: string) => {
        console.log('用户离开:', user)
        userLeftCallbacks.forEach(cb => cb(user))
      },
      (users: string[]) => {
        console.log('用户列表更新:', users)
        usersUpdatedCallbacks.forEach(cb => cb(users))
      }
    )
    
    eventListenersSetup = true
  }

  // 连接到SignalR
  const connect = async (userName: string) => {
    try {
      connectionState.value.isConnecting = true
      connectionState.value.error = null
      
      await signalRService.connect()
      
      // 设置事件监听器
      setupEventListeners()
      
      await signalRService.setUser(userName)
      
      connectionState.value.isConnected = true
      connectionState.value.isConnecting = false
      
      console.log('SignalR连接成功')
    } catch (error) {
      connectionState.value.isConnecting = false
      connectionState.value.error = error instanceof Error ? error.message : '连接失败'
      console.error('SignalR连接失败:', error)
      throw error
    }
  }

  // 断开连接
  const disconnect = async () => {
    try {
      await signalRService.disconnect()
      connectionState.value.isConnected = false
      connectionState.value.isConnecting = false
      connectionState.value.error = null
      console.log('SignalR连接已断开')
    } catch (error) {
      console.error('断开连接失败:', error)
    }
  }

  // 发送消息到大厅
  const sendMessage = async (message: string) => {
    if (!connectionState.value.isConnected) {
      throw new Error('未连接到服务器')
    }
    
    try {
      await signalRService.sendToHall(message)
    } catch (error) {
      console.error('发送消息失败:', error)
      throw error
    }
  }

  // 监听消息接收
  const onMessageReceived = (callback: (user: string, message: string) => void) => {
    messageCallbacks.add(callback)
    return () => messageCallbacks.delete(callback)
  }

  // 监听用户加入
  const onUserJoined = (callback: (user: string) => void) => {
    userJoinedCallbacks.add(callback)
    return () => userJoinedCallbacks.delete(callback)
  }

  // 监听用户离开
  const onUserLeft = (callback: (user: string) => void) => {
    userLeftCallbacks.add(callback)
    return () => userLeftCallbacks.delete(callback)
  }

  // 监听在线用户更新
  const onUsersUpdated = (callback: (users: string[]) => void) => {
    usersUpdatedCallbacks.add(callback)
    return () => usersUpdatedCallbacks.delete(callback)
  }

  return {
    // 状态
    connectionState: computed(() => connectionState.value),
    isConnected,
    isConnecting,
    
    // 方法
    connect,
    disconnect,
    sendMessage,
    
    // 事件监听
    onMessageReceived,
    onUserJoined,
    onUserLeft,
    onUsersUpdated,
    
    // 服务实例（用于获取URL等）
    signalRService
  }
}