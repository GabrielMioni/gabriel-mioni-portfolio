<script setup>
import ProjectsList from '@/components/ProjectsList.vue'
import HeroImage from '@/components/HeroImage.vue'
</script>

<template>
  <v-app>
    <v-main>
      <hero-image />
      <v-container>
        <v-row>
          <v-col>
            <projects-list />
          </v-col>
        </v-row>
      </v-container>
    </v-main>
  </v-app>
</template>

<style lang="scss" scoped>

</style>
