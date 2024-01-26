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

const editProject = (id) => {
  console.log(`edit project - ID: ${id}`)
}

const removeProject = (id) => {
  console.log(`remove project - ID: ${id}`)
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
  <div>
    <div>This is where stuff will go</div>
    <pre>{{ headers }}</pre>
    <pre> {{ projects }}</pre>
  </div>
</template>

<script>
export default {
  name: 'ProjectsTable'
}
</script>

<style scoped>

</style>
