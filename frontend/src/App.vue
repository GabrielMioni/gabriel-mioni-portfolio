<script setup>
import { onMounted } from 'vue'
import { fetchWeatherForecast } from '@/api/index.js'
import ToastMessage from '@/components/ToastMessage.vue'

onMounted(async () => {
  const result = await fetchWeatherForecast()
    .catch(error => console.error(error))
  console.log(result)
})

</script>

<template>
  <div
    id="main"
    class="p-component">
    <ToastMessage />
    <router-view v-slot="{ Component }">
      <transition name="fade">
        <component :is="Component" />
      </transition>
    </router-view>
  </div>
</template>

<style lang="scss" scoped>
#main {
  flex: 1 0 auto;
  max-width: 100%;
  transition: 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.15s;
}
.fade-enter, .fade-leave-to {
  opacity: 0;
}
</style>
