<script setup lang="ts">
import type { GetProjectsQueryVariables } from '~/generated/graphql'
import type { TableOptions } from '~/types/ui/datatable'
import { toGraphqlSort, toGraphqlFilterInput } from '~/utils/graphql'
import { useProjectQueries } from '~/composables/useProjectQueries'

const tableOptions = ref<TableOptions>({
  page: 1,
  itemsPerPage: 10,
  sortBy: [],
  groupBy: [],
  search: ''
})
const search = ref<string>('')

const queryVars = computed<GetProjectsQueryVariables>(() => {
  const options = tableOptions.value
  const skip = (options.page - 1) * options.itemsPerPage
  const take = options.itemsPerPage

  return {
    skip,
    take,
    includeUnpublished: true,
    order: options.sortBy?.length
      ? toGraphqlSort(options.sortBy)
      : undefined,
    where: toGraphqlFilterInput(tableOptions.value.search) ?? undefined
  }
})

watchDebounced(
  search,
  (val) => {
    const next = val.trim()
    if (next === tableOptions.value.search) return
    tableOptions.value.search = next
    tableOptions.value.page = 1
  },
  { debounce: 350, maxWait: 1000 }
)

const editDialogId = ref<string | null>(null)
const deleteDialogId = ref<string | null>(null)
const createDialog = ref(false)

const editDialog = computed({
  get: () => !!editDialogId.value || createDialog.value,
  set: (val) => {
    if (!val) {
      editDialogId.value = null
      createDialog.value = false
    }
  }
})

const selectedEditProject = computed(() => {
  const id = editDialogId.value
  if (!id) return null
  return projects.value.find((p) => p.id === id) ?? null
})

const onEdit = (id: string) => {
  editDialogId.value = id
}
const onDelete = (id: string) => {
  deleteDialogId.value = id
}

provide('projectActions', {
  edit: onEdit,
  delete: onDelete
})


const {
  projects,
  pageInfo,
  totalCount
} = useProjectQueries(queryVars)

</script>

<template>
  <v-container>
    <v-row>
      <v-col class="px-0">
        <ProjectsTable
          v-model:options="tableOptions"
          v-model:search="search"
          :projects="projects"
          :total-count="totalCount"
          :page-info="pageInfo"
          @create-project="createDialog = true"/>
      </v-col>
    </v-row>
    <ProjectDialog
      v-model="editDialog"
      :project="selectedEditProject" />
  </v-container>
</template>

<style scoped>

</style>
