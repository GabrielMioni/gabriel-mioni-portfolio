<script setup>
import ProjectsList from '@/components/ProjectsList.vue'
import heroImage from '@/assets/nature-forest-trees-fog.jpeg'
</script>

<template>
  <v-app>
    <v-main>
      <v-parallax
        :src="heroImage"
        height="300">
        <div class="d-flex flex-column fill-height justify-center text-white text-h4 text-center">
          Web developer.<br>
          Available for hire.
        </div>
      </v-parallax>
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
