<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useProjectsStore } from '@/store/projects/index.js'
import { useAddOrRemoveDialog } from '@/views/Admin/ProjectsTable/showAddOrRemoveDialog.js'
import PopupMenu from '@/components/PopupMenu.vue'
import ProjectEditModal from '@/views/Admin/ProjectsTable/ProjectEditModal.vue'
import BaseButton from '@/components/BaseButton.vue'
import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'
import FlexRow from '@/components/flex/FlexRow.vue'
const projectStore = useProjectsStore()
import moment from 'moment'

// Reactive
const currentPage = ref(1)
const itemsPerPage = ref(10)
const editActiveId = ref(null)
const showInactive = ref(true)
const createNewProjectActive = ref(false)

const columnFields = {
  active: 'active',
  action: 'action',
  createdAt: 'createdAt'
}

const columns = [
  {
    field: 'name',
    header: 'Name',
    sortable: false
  },
  {
    field: 'createdAt',
    header: 'Created At',
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

const emptyProject = {
  active: true,
  description: '',
  id: null,
  git: '',
  name: ''
}

const projectBeingEdited = computed(() => {
  if (createNewProjectActive.value) {
    return emptyProject
  }
  const projectId = editActiveId.value
  if (projectId === null) {
    return null
  }
  return findProjectById(projectId) || null
})

const editActive = computed({
  get: () => editActiveId.value !== null || createNewProjectActive.value,
  set: (val) => {
    if (!val) {
      editActiveId.value = null
      createNewProjectActive.value = false
    }
  }
})

// Methods
const setEditActiveId = (id) => {
  editActiveId.value = id
}

const findProjectById = (id) => projects.value.find(p => p.id === id)

const { showAddOrRemoveDialog } = useAddOrRemoveDialog()

const confirmAddOrRemove = async (id, isActive) => {
  try {
    const confirmAddOrRemove = await showAddOrRemoveDialog(id, !isActive)
    if (!confirmAddOrRemove) {
      return
    }
    projectStore.setProjectActive({ id, setActive: !isActive })
    console.log('Updated')
  } catch (e) {
    console.error(e)
  }
}

const getMenuItemsForRow = (id) => {
  const project = findProjectById(id)
  const isActive = project.active
  const actionType = isActive ? 'Remove' : 'Add'
  return [
    {
      label: 'Options',
      items: [
        {
          label: 'Edit',
          icon: 'pi pi-pencil',
          command: () => setEditActiveId(id)
        },
        {
          label: actionType,
          icon: 'pi pi-trash',
          command: () => confirmAddOrRemove(id, isActive)
        }
      ]
    }
  ]
}

const formatDisplayDate = (date) => {
  return moment(date).format('MM/DD/YYYY HH:mm')
}

const loadProjectsForCurrentPage = () => {
  const skip = (currentPage.value - 1) * itemsPerPage.value
  projectStore.loadProjects({ skip, take: itemsPerPage.value })
}

const createNewProject = () => {
  createNewProjectActive.value = true
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
    <toast-message />
    <project-edit-modal
      v-model="editActive"
      :project="projectBeingEdited" />
    <data-table
      :value="displayProjects"
      :rows="10"
      :rows-per-page-options="[10, 25, 50]"
      :total-records="totalRecords"
      paginator
      paginator-template="RowsPerPageDropdown FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink"
      current-page-report-template="{first} to {last} of {totalRecords}">
      <template #header>
        <flex-container
          fluid
          class="p-0">
          <flex-row>
            <flex-column
              :cols="6"
              class="flex justify-content-center">
              <div class="flex gap-2">
                <input-switch
                  v-model="showInactive"
                  input-id="input-show-active" />
                <label for="input-show-active">Show Inactive</label>
              </div>
            </flex-column>
            <flex-column
              :cols="6"
              class="flex justify-content-center"
              style="text-align: end">
              <div>
                <base-button @click="createNewProject">
                  Create Project
                </base-button>
              </div>
            </flex-column>
          </flex-row>
        </flex-container>
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
          <template v-else-if="col.field === columnFields.active">
            <tag
              :severity="data.active ? 'success' : 'warning'"
              :value="data.active ? 'Active' : 'Inactive'" />
          </template>
          <template v-else-if="col.field === columnFields.createdAt">
            {{ formatDisplayDate(data[col.field]) }}
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
