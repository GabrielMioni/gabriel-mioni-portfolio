<script setup>
import { useProjectsStore } from '@/store/projects/index.js'

const projectStore = useProjectsStore()
const projects = projectStore.projects || []

</script>

<template>
  <v-card
    v-for="project in projects"
    :key="project.id">
    <v-card-text>
      {{ project.name }}
    </v-card-text>
  </v-card>
</template>
