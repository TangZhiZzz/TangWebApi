<script setup lang="ts">
import ChatHall from './components/ChatHall.vue'
</script>

<template>
  <div id="app">
    <ChatHall />
  </div>
</template>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  height: 100vh;
  overflow: hidden;
}

#app {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
