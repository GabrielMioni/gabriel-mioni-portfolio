<script setup>
import { useProjectsStore } from '@/store/projects/index.js'

const projectStore = useProjectsStore()

console.log(projectStore.projects) // Accessing the 'projects' getter directly
</script>

<template>
  <div class="projects-list">
    Hello.
    <ul>
      <li
        v-for="project in projectStore.projects"
        :key="project.id">
        {{ project.name }}
      </li>
    </ul>
  </div>
</template>
