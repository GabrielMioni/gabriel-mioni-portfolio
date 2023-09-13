<script setup>
import { onMounted } from 'vue'
import { fetchWeatherForecast } from '@/api/index.js'
import HeroImage from '@/components/HeroImage.vue'

onMounted(async () => {
  const result = await fetchWeatherForecast()
    .catch(error => console.error(error))
  console.log(result)
})

</script>

<template>
  <v-app>
    <v-toolbar
      app
      color="#47c6c2"
      class="text-white">
      <v-toolbar-title>
        Gabriel Mioni
      </v-toolbar-title>
      <v-spacer />
      <v-btn
        text
        to="/">
        Projects
      </v-btn>
      <v-btn
        text
        to="/about">
        Contact
      </v-btn>
    </v-toolbar>
    <v-main>
      <hero-image />
      <router-view />
    </v-main>
  </v-app>
</template>

<style lang="scss" scoped>

</style>
