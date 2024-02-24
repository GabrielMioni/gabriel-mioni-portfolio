<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useProjectsStore } from '@/store/projects/index.js'
import { useAddOrRemoveDialog } from '@/views/Admin/ProjectsTable/showAddOrRemoveDialog.js'
// import ProjectEditModal from '@/views/Admin/ProjectsTable/ProjectEditModal.vue'
import PopupMenu from '@/components/PopupMenu.vue'

const projectStore = useProjectsStore()

// Reactive
const currentPage = ref(1)
const itemsPerPage = ref(10)
const editActiveId = ref(null)
const showInactive = ref(true)

const columnFields = {
  active: 'active',
  action: 'action'
}

const columns = [
  {
    field: 'name',
    header: 'Name',
    sortable: false
  },
  {
    field: 'description',
    header: 'Description',
    sortable: false
  },
  {
    field: 'git',
    header: 'Git',
    sortable: false
  },
  {
    field: columnFields.active,
    header: 'Active',
    sortable: false
  },
  {
    field: columnFields.action,
    header: ' ',
    sortable: false
  }
]

// Computed
const projects = computed(() => projectStore.projectsFormatted)
const totalRecords = computed(() => projectStore.projectCount)

const displayColumns = computed(() => showInactive.value ? columns : columns.filter(col => col.field !== 'active'))

const displayProjects = computed(() => {
  return projects.value.filter(proj => {
    return showInactive.value ? true : proj.active
  })
})

// Methods
const editProject = (id) => {
  editActiveId.value = id
}

const { showAddOrRemoveDialog } = useAddOrRemoveDialog()

const findProjectById = (id) => projects.value.find(p => p.id === id)

const confirmAddOrRemove = async (id) => {
  // const project = projects.value.find(p => p.id === id)
  const project = findProjectById(id)
  if (!project) {
    console.warn(`Unable to find project with id of ${id}`)
    return
  }
  const setActive = !project.active
  try {
    const result = await showAddOrRemoveDialog(id, !project.active)
    if (!result) {
      return
    }
    projectStore.setProjectActive({ id, setActive })
    console.log('Updated')
  } catch (e) {
    console.error(e)
  }
}

const getMenuItemsForRow = (id) => {
  const project = findProjectById(id)
  const actionType = project.active ? 'Remove' : 'Add'
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
          label: actionType,
          icon: 'pi pi-trash',
          command: () => confirmAddOrRemove(id)
        }
      ]
    }
  ]
}

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
  <div>
    <h3>This is where stuff will go</h3>
    <confirm-dialog />

    <data-table
      :value="displayProjects"
      :rows="10"
      :rows-per-page-options="[10, 25, 50]"
      :total-records="totalRecords"
      paginator
      paginator-template="RowsPerPageDropdown FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink"
      current-page-report-template="{first} to {last} of {totalRecords}">
      <template #header>
        <div class="flex justify-content-end align-items-center gap-2">
          <input-switch
            v-model="showInactive"
            input-id="input-show-active" />
          <label for="input-show-active">Show Inactive</label>
        </div>
      </template>
      <column
        v-for="col of displayColumns"
        :key="col.field"
        :field="col.field"
        :header="col.header"
        :sortable="col.sortable">
        <template #body="{ data }">
          <popup-menu
            v-if="col.field === columnFields.action"
            :items="getMenuItemsForRow(data.id)" />
          <template v-if="col.field === columnFields.active">
            <tag
              :severity="data.active ? 'success' : 'warning'"
              :value="data.active ? 'Active' : 'Inactive'" />
          </template>
          <template v-else>
            {{ data[col.field] }}
          </template>
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
