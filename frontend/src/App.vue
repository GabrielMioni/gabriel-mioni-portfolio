<script setup>
import { onMounted } from 'vue'
import { fetchWeatherForecast } from '@/api/index.js'
import HeroImage from '@/components/HeroImage/HeroImage.vue'
import ToolBar from '@/components/ToolBar.vue'
import FooterSection from '@/components/FooterSection.vue'

onMounted(async () => {
  const result = await fetchWeatherForecast()
    .catch(error => console.error(error))
  console.log(result)
})

</script>

<template>
  <v-app>
    <v-main>
      <tool-bar />
      <hero-image />
      <router-view v-slot="{ Component }">
        <transition name="fade">
          <component :is="Component" />
        </transition>
      </router-view>
      <footer-section />
    </v-main>
  </v-app>
</template>

<style lang="scss" scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.15s;
}
.fade-enter, .fade-leave-to {
  opacity: 0;
}
</style>
