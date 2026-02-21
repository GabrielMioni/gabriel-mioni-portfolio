<script setup lang="ts">
import type { Project } from '~/generated/graphql'

defineProps<{
  project: Project
}>()

</script>

<template>
  <tr>
    <td>{{ project.title }}</td>
    <td>{{ project.body }}</td>
    <td>{{ project.publishedAt }}</td>
    <td>{{ project.createdAt }}</td>
    <td>{{ project.updatedAt }}</td>
    <td>{{ project.summary }}</td>
    <td>{{ project.status }}</td>
    <td class="text-end">
      <ProjectMenu :project-id="project.id"/>
    </td>
  </tr>
</template>

<style scoped>

</style>
