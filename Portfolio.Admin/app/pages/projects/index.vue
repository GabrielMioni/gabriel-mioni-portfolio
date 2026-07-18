<script setup lang="ts">
import type { GetProjectsQueryVariables } from '~/generated/graphql'
import { toGraphqlSort, toGraphqlFilterInput } from '~/utils/graphql'
import { useProjectQueries } from '~/composables/useProjectQueries'

const {
  tableOptions,
  search,
  updateTableOptions
} = useTableUrlSync({ defaultItemsPerPage: 10 })

const editDialogId = ref<string | null>(null)
const deleteDialogId = ref<string | null>(null)

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
    where: toGraphqlFilterInput(options.search) ?? undefined
  }
})

const {
  projects,
  pageInfo,
  refetchProjects,
  totalCount
} = useProjectQueries(queryVars)

const editDialog = computed({
  get: () => !!editDialogId.value,
  set: (val) => {
    if (!val) editDialogId.value = null
  }
})

const deleteDialog = computed({
  get: () => !!deleteDialogId.value,
  set: (val) => {
    if (!val) {
      deleteDialogId.value = null
    }
  }
})

const selectedEditProject = computed(() => {
  const id = editDialogId.value
  if (!id) return null
  return projects.value.find((project) => project.id === id) ?? null
})

const selectedDeleteProject = computed(() => {
  const id = deleteDialogId.value
  if (!id) return null
  return projects.value.find((project) => project.id === id) ?? null
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
</script>

<template>
  <v-container>
    <v-row>
      <v-col>
        <ProjectsTable
          v-model:search="search"
          :options="tableOptions"
          :projects="projects"
          :total-count="totalCount"
          :page-info="pageInfo"
          @update:options="updateTableOptions" />
      </v-col>
    </v-row>
    <QuickEditDialog
      v-model="editDialog"
      :project="selectedEditProject" />
    <DeleteProjectDialog
      v-if="selectedDeleteProject"
      v-model="deleteDialog"
      :title="selectedDeleteProject.title"
      :summary="selectedDeleteProject.summary"
      :project-id="selectedDeleteProject.id"
      @deleted="refetchProjects" />
  </v-container>
</template>
