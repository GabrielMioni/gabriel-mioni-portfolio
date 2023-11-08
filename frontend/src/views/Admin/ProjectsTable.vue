<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useProjectsStore } from '@/store/projects/index.js'

const projectStore = useProjectsStore()

// Data
const currentPage = ref(1)
const itemsPerPage = ref(10)
const headers = ref([
  { text: 'Name', value: 'name' },
  { text: 'Description', value: 'description' },
  { text: 'Git', value: 'git' }
])

// Computed
const projects = computed(() => projectStore.projectsFormatted)
const projectCount = computed(() => projectStore.projectCount)

// Methods
const loadProjectsForCurrentPage = () => {
  const skip = (currentPage.value - 1) * itemsPerPage.value
  projectStore.loadProjects({ skip, take: itemsPerPage.value })
}

// Watchers
watch(currentPage, () => {
  loadProjectsForCurrentPage()
})

// Lifecycle Hooks
onMounted(() => {
  loadProjectsForCurrentPage()
})

</script>

<template>
  <v-data-table-server
    :items-per-page="itemsPerPage"
    :items="projects"
    :items-length="projectCount">
    <thead>
      <tr>
        <th
          v-for="header in headers"
          :key="header">
          {{ header.text }}
        </th>
      </tr>
    </thead>
    <tbody />
    <tbody>
      <tr
        v-for="project in projects"
        :key="project.id">
        <td
          v-for="(header, index) in headers"
          :key="index">
          {{ project[header.value] ? project[header.value] : '' }}
        </td>
      </tr>
    </tbody>
  </v-data-table-server>
</template>

<script>
export default {
  name: 'ProjectsTable'
}
</script>

<style scoped>

</style>
