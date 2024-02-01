<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useProjectsStore } from '@/store/projects/index.js'

const projectStore = useProjectsStore()

// Data
const currentPage = ref(1)
const itemsPerPage = ref(10)
const tableParams = ref({
  first: 0,
  rows: 10,
  sortField: null,
  filters: []
})

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
const totalRecords = computed(() => projectStore.projectCount)

// Methods
const loadProjectsForCurrentPage = () => {
  const skip = (currentPage.value - 1) * itemsPerPage.value
  projectStore.loadProjects({ skip, take: itemsPerPage.value })
}

const handleSort = (val) => {
  console.log(val)
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
    <data-table
      :value="projects"
      :rows="10"
      :rows-per-page-options="[10, 25, 50]"
      :total-records="totalRecords"
      paginator
      paginator-template="RowsPerPageDropdown FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink"
      current-page-report-template="{first} to {last} of {totalRecords}"
      @sort="handleSort">
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
