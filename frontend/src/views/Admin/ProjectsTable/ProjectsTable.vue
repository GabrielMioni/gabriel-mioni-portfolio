<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useProjectsStore } from '@/store/projects/index.js'
import apolloClient from '@/apollo/apolloClient.js'
import ProjectEditModal from '@/views/Admin/ProjectsTable/ProjectEditModal.vue'
import PopupMenu from '@/components/PopupMenu.vue'
import { useConfirm } from 'primevue/useconfirm'
import RemoveProject from '@/views/Admin/ProjectsTable/graphql/RemoveProject.gql'

const confirm = useConfirm()

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
const show = ref(false)

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
  },
  {
    field: 'action',
    header: ' ',
    sortable: false
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

const editActiveId = ref(null)

const editProject = (id) => {
  editActiveId.value = id
}

const confirmRemoveProject = (id) => {
  confirm.require({
    message: `You are about to remove project ${id} you sure you want to proceed?`,
    header: 'Remove Project',
    icon: 'pi pi-exclamation-triangle',
    rejectClass: 'p-button-secondary p-button-outlined',
    rejectLabel: 'Cancel',
    acceptLabel: 'Remove',
    accept: async () => {
      await removeProjectAsync(id)
    }
  })
}

const removeProjectAsync = async (id) => {
  try {
    const { data } = await apolloClient.mutate({
      mutation: RemoveProject,
      variables: {
        id
      },
      errorPolicy: 'all'
    })
    console.log(data)
  } catch (e) {
    console.error(e)
  }
}

const removeProject = (id) => {
  confirmRemoveProject(id)
}

const getMenuItemsForRow = (id) => {
  return [
    {
      label: 'Options',
      items: [
        {
          label: 'Edit',
          icon: 'pi pi-pencil',
          command: () => editProject(id)
        },
        {
          label: 'Remove',
          icon: 'pi pi-trash',
          command: () => removeProject(id)
        }
      ]
    }
  ]
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
    <Button
      label="Show"
      @click="confirmRemoveProject" />
    <project-edit-modal v-model="show" />
    <confirm-dialog />

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
        :sortable="col.sortable">
        <template
          v-if="col.field === 'action'"
          #body="{ data }">
          <popup-menu :items="getMenuItemsForRow(data.id)" />
        </template>
      </column>
    </data-table>
    <!--    <pre>{{ columns }}</pre>-->
    <!--    <pre> {{ projects }}</pre>-->
  </div>
</template>

<script>
export default {
  name: 'ProjectsTable'
}
</script>

<style scoped>

</style>
