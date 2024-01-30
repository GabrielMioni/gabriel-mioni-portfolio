<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useProjectsStore } from '@/store/projects/index.js'

const projectStore = useProjectsStore()

// Data
const currentPage = ref(1)
const itemsPerPage = ref(10)

const columns = [
  {
    field: 'name',
    header: 'Name',
    sortable: true
  },
  {
    field: 'description',
    header: 'Description',
    sortable: true
  },
  {
    field: 'git',
    header: 'Git',
    sortable: true
  }
]

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
    <h3>This is where stuff will go</h3>
    <data-table :value="projects">
      <column
        v-for="col of columns"
        :key="col.field"
        :field="col.field"
        :header="col.header"
        :sortable="col.sortable" />
    </data-table>
    <pre>{{ columns }}</pre>
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
