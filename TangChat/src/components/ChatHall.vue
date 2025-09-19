<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick } from 'vue'
import { useSignalR } from '../composables/useSignalR'

// 用户信息
const userName = ref('')
const isLoggedIn = ref(false)

// 聊天相关
const message = ref('')
const messages = ref<Array<{
  id: string
  user: string
  content: string
  timestamp: Date
  type: 'user' | 'system'
}>>([])

// 在线用户
const onlineUsers = ref<string[]>([])

// 引用
const messagesContainer = ref<HTMLElement>()
const messageInput = ref<HTMLInputElement>()

// SignalR
const { 
  connectionState, 
  connect, 
  disconnect, 
  sendMessage,
  onMessageReceived,
  onUserJoined,
  onUserLeft,
  onUsersUpdated
} = useSignalR()

// 登录
const login = async () => {
  if (!userName.value.trim()) {
    alert('请输入用户名')
    return
  }
  
  try {
    await connect(userName.value.trim())
    isLoggedIn.value = true
    addSystemMessage(`欢迎来到聊天大厅，${userName.value}！`)
  } catch (error) {
    console.error('连接失败:', error)
    alert('连接失败，请重试')
  }
}

// 登出
const logout = async () => {
  await disconnect()
  isLoggedIn.value = false
  messages.value = []
  onlineUsers.value = []
  addSystemMessage('已断开连接')
}

// 发送消息
const sendChatMessage = async () => {
  if (!message.value.trim()) return
  
  try {
    await sendMessage(message.value.trim())
    message.value = ''
    await nextTick()
    messageInput.value?.focus()
  } catch (error) {
    console.error('发送消息失败:', error)
    alert('发送消息失败')
  }
}

// 添加系统消息
const addSystemMessage = (content: string) => {
  messages.value.push({
    id: Date.now().toString(),
    user: 'System',
    content,
    timestamp: new Date(),
    type: 'system'
  })
  scrollToBottom()
}

// 添加用户消息
const addUserMessage = (user: string, content: string) => {
  messages.value.push({
    id: Date.now().toString(),
    user,
    content,
    timestamp: new Date(),
    type: 'user'
  })
  scrollToBottom()
}

// 滚动到底部
const scrollToBottom = async () => {
  await nextTick()
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
  }
}

// 格式化时间
const formatTime = (date: Date) => {
  return date.toLocaleTimeString('zh-CN', { 
    hour: '2-digit', 
    minute: '2-digit' 
  })
}

// 处理回车发送
const handleKeyPress = (event: KeyboardEvent) => {
  if (event.key === 'Enter' && !event.shiftKey) {
    event.preventDefault()
    sendChatMessage()
  }
}

// 生命周期
onMounted(() => {
  // 监听消息
  onMessageReceived((user: string, message: string) => {
    addUserMessage(user, message)
  })
  
  // 监听用户加入
  onUserJoined((user: string) => {
    addSystemMessage(`${user} 加入了聊天室`)
  })
  
  // 监听用户离开
  onUserLeft((user: string) => {
    addSystemMessage(`${user} 离开了聊天室`)
  })
  
  // 监听在线用户更新
  onUsersUpdated((users: string[]) => {
    onlineUsers.value = users
  })
})

onUnmounted(() => {
  disconnect()
})
</script>

<template>
  <div class="chat-hall">
    <!-- 登录界面 -->
    <div v-if="!isLoggedIn" class="login-container">
      <div class="login-card">
        <h1 class="login-title">TangChat 聊天大厅</h1>
        <p class="login-subtitle">请输入您的用户名加入聊天</p>
        
        <div class="login-form">
          <input
            v-model="userName"
            type="text"
            placeholder="输入用户名"
            class="login-input"
            @keypress.enter="login"
            maxlength="20"
          />
          <button 
            @click="login" 
            class="login-btn"
            :disabled="!userName.trim()"
          >
            加入聊天
          </button>
        </div>
      </div>
    </div>

    <!-- 聊天界面 -->
    <div v-else class="chat-container">
      <!-- 头部 -->
      <div class="chat-header">
        <div class="header-left">
          <h2 class="chat-title">聊天大厅</h2>
          <span class="connection-status" :class="{ connected: connectionState.isConnected }">
            {{ connectionState.isConnected ? '已连接' : '连接中...' }}
          </span>
        </div>
        <div class="header-right">
          <span class="user-info">{{ userName }}</span>
          <button @click="logout" class="logout-btn">退出</button>
        </div>
      </div>

      <div class="chat-body">
        <!-- 消息区域 -->
        <div class="messages-section">
          <div ref="messagesContainer" class="messages-container">
            <div
              v-for="msg in messages"
              :key="msg.id"
              :class="['message', msg.type, { own: msg.user === userName }]"
            >
              <div v-if="msg.type === 'system'" class="system-message">
                {{ msg.content }}
              </div>
              <div v-else class="user-message">
                <div class="message-header">
                  <span class="message-user">{{ msg.user }}</span>
                  <span class="message-time">{{ formatTime(msg.timestamp) }}</span>
                </div>
                <div class="message-content">{{ msg.content }}</div>
              </div>
            </div>
          </div>

          <!-- 输入区域 -->
          <div class="input-section">
            <div class="input-container">
              <input
                ref="messageInput"
                v-model="message"
                type="text"
                placeholder="输入消息..."
                class="message-input"
                @keypress="handleKeyPress"
                :disabled="!connectionState.isConnected"
              />
              <button 
                @click="sendChatMessage" 
                class="send-btn"
                :disabled="!message.trim() || !connectionState.isConnected"
              >
                发送
              </button>
            </div>
          </div>
        </div>

        <!-- 在线用户列表 -->
        <div class="users-section">
          <div class="users-header">
            <h3>在线用户 ({{ onlineUsers.length }})</h3>
          </div>
          <div class="users-list">
            <div
              v-for="user in onlineUsers"
              :key="user"
              :class="['user-item', { current: user === userName }]"
            >
              <div class="user-avatar">{{ user.charAt(0).toUpperCase() }}</div>
              <span class="user-name">{{ user }}</span>
              <span v-if="user === userName" class="user-label">(我)</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.chat-hall {
  width: 100%;
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* 登录界面 */
.login-container {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
}

.login-card {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  border-radius: 20px;
  padding: 40px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  text-align: center;
  max-width: 400px;
  width: 90%;
}

.login-title {
  font-size: 28px;
  font-weight: bold;
  color: #333;
  margin-bottom: 10px;
}

.login-subtitle {
  color: #666;
  margin-bottom: 30px;
  font-size: 16px;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.login-input {
  padding: 15px 20px;
  border: 2px solid #e1e5e9;
  border-radius: 12px;
  font-size: 16px;
  outline: none;
  transition: all 0.3s ease;
}

.login-input:focus {
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.login-btn {
  padding: 15px 30px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.login-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 10px 25px rgba(102, 126, 234, 0.3);
}

.login-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 聊天界面 */
.chat-container {
  width: 95%;
  max-width: 1200px;
  height: 90vh;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  border-radius: 20px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.chat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 30px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 15px;
}

.chat-title {
  font-size: 24px;
  font-weight: bold;
  margin: 0;
}

.connection-status {
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 12px;
  background: rgba(255, 255, 255, 0.2);
}

.connection-status.connected {
  background: rgba(76, 175, 80, 0.8);
}

.header-right {
  display: flex;
  align-items: center;
  gap: 15px;
}

.user-info {
  font-weight: 600;
}

.logout-btn {
  padding: 8px 16px;
  background: rgba(255, 255, 255, 0.2);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.logout-btn:hover {
  background: rgba(255, 255, 255, 0.3);
}

.chat-body {
  flex: 1;
  display: flex;
  overflow: hidden;
}

/* 消息区域 */
.messages-section {
  flex: 1;
  display: flex;
  flex-direction: column;
  border-right: 1px solid #e1e5e9;
}

.messages-container {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.message.system {
  text-align: center;
}

.system-message {
  display: inline-block;
  padding: 8px 16px;
  background: #f0f0f0;
  color: #666;
  border-radius: 20px;
  font-size: 14px;
}

.user-message {
  max-width: 70%;
  align-self: flex-start;
}

.message.own .user-message {
  align-self: flex-end;
}

.message-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 5px;
  gap: 10px;
}

.message-user {
  font-weight: 600;
  color: #667eea;
  font-size: 14px;
}

.message.own .message-user {
  color: #764ba2;
}

.message-time {
  font-size: 12px;
  color: #999;
}

.message-content {
  padding: 12px 16px;
  background: #f8f9fa;
  border-radius: 18px;
  word-wrap: break-word;
  line-height: 1.4;
}

.message.own .message-content {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

/* 输入区域 */
.input-section {
  padding: 20px;
  border-top: 1px solid #e1e5e9;
  background: #fafafa;
}

.input-container {
  display: flex;
  gap: 10px;
}

.message-input {
  flex: 1;
  padding: 12px 16px;
  border: 2px solid #e1e5e9;
  border-radius: 25px;
  outline: none;
  font-size: 14px;
  transition: all 0.3s ease;
}

.message-input:focus {
  border-color: #667eea;
}

.send-btn {
  padding: 12px 24px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 25px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.3s ease;
}

.send-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 5px 15px rgba(102, 126, 234, 0.3);
}

.send-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 用户列表 */
.users-section {
  width: 280px;
  background: #fafafa;
  display: flex;
  flex-direction: column;
}

.users-header {
  padding: 20px;
  border-bottom: 1px solid #e1e5e9;
}

.users-header h3 {
  margin: 0;
  color: #333;
  font-size: 16px;
}

.users-list {
  flex: 1;
  overflow-y: auto;
  padding: 10px;
}

.user-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  border-radius: 12px;
  transition: all 0.3s ease;
  margin-bottom: 5px;
}

.user-item:hover {
  background: rgba(102, 126, 234, 0.1);
}

.user-item.current {
  background: rgba(102, 126, 234, 0.15);
}

.user-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 14px;
}

.user-name {
  flex: 1;
  font-weight: 500;
  color: #333;
}

.user-label {
  font-size: 12px;
  color: #667eea;
  font-weight: 600;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .chat-container {
    width: 100%;
    height: 100vh;
    border-radius: 0;
  }
  
  .chat-body {
    flex-direction: column;
  }
  
  .users-section {
    width: 100%;
    max-height: 200px;
    border-right: none;
    border-top: 1px solid #e1e5e9;
  }
  
  .users-list {
    display: flex;
    flex-direction: row;
    overflow-x: auto;
    padding: 10px;
  }
  
  .user-item {
    flex-shrink: 0;
    margin-right: 10px;
    margin-bottom: 0;
  }
  
  .chat-header {
    padding: 15px 20px;
  }
  
  .chat-title {
    font-size: 20px;
  }
  
  .header-right {
    gap: 10px;
  }
}
</style>