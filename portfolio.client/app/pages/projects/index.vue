<script setup lang="ts">
import type { GetProjectsQueryVariables } from '~/generated/graphql'
import type { TableOptions } from '~/types/ui/datatable'
import { toGraphqlSort, toGraphqlFilterInput } from '~/utils/graphql'
import { useProjectQueries } from '~/composables/useProjectQueries'

const route = useRoute()
const router = useRouter()

const getPositiveNumberFromQuery = (
  value: unknown,
  fallback: number
): number => {
  if (typeof value !== 'string') return fallback

  const parsed = Number(value)

  return Number.isFinite(parsed) && parsed > 0
    ? parsed
    : fallback
}

const getStringFromQuery = (
  value: unknown,
  fallback = ''
): string => {
  return typeof value === 'string'
    ? value
    : fallback
}

const getInitialTableOptions = (): TableOptions => {
  const page = getPositiveNumberFromQuery(route.query.page, 1)
  const itemsPerPage = getPositiveNumberFromQuery(route.query.itemsPerPage, 10)
  const search = getStringFromQuery(route.query.search).trim()

  return {
    page,
    itemsPerPage,
    sortBy: [],
    groupBy: [],
    search
  }
}

const tableOptions = ref<TableOptions>(getInitialTableOptions())
const search = ref<string>((tableOptions.value?.search ?? '').trim())

const editDialogId = ref<string | null>(null)
const deleteDialogId = ref<string | null>(null)
const newDraft = ref(false)

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

const replaceRouteQueryFromOptions = async (options: TableOptions) => {
  const nextQuery: Record<string, string> = {
    page: String(options.page),
    itemsPerPage: String(options.itemsPerPage)
  }

  const trimmedSearch = options.search?.trim() ?? ''

  if (trimmedSearch.length > 0) {
    nextQuery.search = trimmedSearch
  }

  const currentPage = String(route.query.page ?? '1')
  const currentItemsPerPage = String(route.query.itemsPerPage ?? '10')
  const currentSearch = String(route.query.search ?? '')

  const queryIsAlreadyCurrent =
      currentPage === nextQuery.page &&
      currentItemsPerPage === nextQuery.itemsPerPage &&
      currentSearch === (nextQuery.search ?? '')

  if (queryIsAlreadyCurrent) return

  await router.replace({
    query: nextQuery
  })
}

const updateTableOptions = async (options: TableOptions) => {
  tableOptions.value = {
    ...tableOptions.value,
    ...options,
    search: tableOptions.value.search
  }

  await replaceRouteQueryFromOptions(tableOptions.value)
}

watchDebounced(
  search,
  async (val) => {
    const next = val.trim()

    if (next === tableOptions.value.search) return

    tableOptions.value = {
      ...tableOptions.value,
      search: next,
      page: 1
    }

    await replaceRouteQueryFromOptions(tableOptions.value)
  },
  { debounce: 350, maxWait: 1000 }
)

const {
  projects,
  pageInfo,
  refetchProjects,
  totalCount
} = useProjectQueries(queryVars)

const editDialog = computed({
  get: () => !!editDialogId.value || newDraft.value,
  set: (val) => {
    if (!val) {
      editDialogId.value = null
      newDraft.value = false
    }
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
      <v-col class="px-0">
        <ProjectsTable
          v-model:search="search"
          :options="tableOptions"
          :projects="projects"
          :total-count="totalCount"
          :page-info="pageInfo"
          @update:options="updateTableOptions"
          @new-draft="newDraft = true" />
      </v-col>
    </v-row>
    <ProjectDialog
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

<style scoped>

</style>
