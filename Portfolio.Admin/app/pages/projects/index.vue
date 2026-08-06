<script setup lang="ts">
import type { GetProjectsQueryVariables } from '~/generated/graphql'
import { toGraphqlSort, toGraphqlFilterInput } from '~/utils/graphql'
import { useProjectQueries } from '~/composables/useProjectQueries'

const {
  tableOptions,
  search,
  updateTableOptions
} = useTableUrlSync({ defaultItemsPerPage: 10 })

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

const deleteDialog = computed({
  get: () => !!deleteDialogId.value,
  set: (val) => {
    if (!val) {
      deleteDialogId.value = null
    }
  }
})

const selectedDeleteProject = computed(() => {
  const id = deleteDialogId.value
  if (!id) return null
  return projects.value.find((project) => project.id === id) ?? null
})

const onDelete = (id: string) => {
  deleteDialogId.value = id
}

provide('projectActions', {
  delete: onDelete
})
</script>

<template>
  <v-container
    fluid
    class="admin-page">
    <AdminPageHeader
      eyebrow="Content index"
      title="Projects"
      description="Create, revise, and publish project studies."
      :count="totalCount">
      <template #actions>
        <v-btn
          color="primary"
          prepend-icon="mdi-plus"
          variant="flat"
          to="/projects/create">
          Add Project
        </v-btn>
      </template>
    </AdminPageHeader>
    <ProjectsTable
      v-model:search="search"
      class="admin-table-workspace"
      :options="tableOptions"
      :projects="projects"
      :total-count="totalCount"
      :page-info="pageInfo"
      @update:options="updateTableOptions" />
    <DeleteProjectDialog
      v-if="selectedDeleteProject"
      v-model="deleteDialog"
      :title="selectedDeleteProject.title"
      :summary="selectedDeleteProject.summary"
      :project-id="selectedDeleteProject.id"
      @deleted="refetchProjects" />
  </v-container>
</template>
