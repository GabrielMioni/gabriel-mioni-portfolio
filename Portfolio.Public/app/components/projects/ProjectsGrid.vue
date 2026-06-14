<script setup lang="ts">
import { useProjectQueries } from '~/composables/useProjectQueries'
import ProjectItem from '~/components/projects/ProjectItem.vue'

const {
  projects,
  fetchingProjects
} = useProjectQueries()
</script>

<template>
  <UContainer>
    <div class="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
      <ProjectItem
        v-for="item in projects"
        :key="item.id"
        :project="item" />
    </div>
  </UContainer>
</template>

<style scoped>

</style>
