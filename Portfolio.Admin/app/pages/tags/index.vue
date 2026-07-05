<script setup lang="ts">
import type { GetTagSummariesQueryVariables } from '~/generated/graphql'
import type { TableOptions } from '~/types/ui/datatable'
import { toTagSortInput, toTagFilterInput, useTagsTableQueries } from '~/composables/useTagsTableQueries'

enum TagAction {
  Edit = 'edit',
  ViewProjects = 'viewProjects',
  Delete = 'delete'
}

const tableOptions = ref<TableOptions>({
  page: 1,
  itemsPerPage: 25,
  sortBy: [{ key: 'name', order: 'asc' }],
  groupBy: [],
  search: ''
})

const search = ref<string>('')

const queryVars = computed<GetTagSummariesQueryVariables>(() => {
  const { page, itemsPerPage, sortBy } = tableOptions.value
  return {
    skip: (page - 1) * itemsPerPage,
    take: itemsPerPage,
    order: sortBy?.length ? toTagSortInput(sortBy) : undefined,
    where: toTagFilterInput(tableOptions.value.search) ?? undefined
  }
})

const updateTableOptions = (options: TableOptions) => {
  tableOptions.value = { ...tableOptions.value, ...options, search: tableOptions.value.search }
}

watchDebounced(
  search,
  (val) => {
    tableOptions.value = { ...tableOptions.value, search: val.trim(), page: 1 }
  },
  { debounce: 350, maxWait: 1000 }
)

const { tags, totalCount, refetchTags } = useTagsTableQueries(queryVars)

const activeAction = ref<TagAction | null>(null)
const selectedTagId = ref<string | null>(null)

const selectedTag = computed(() =>
  tags.value.find(t => t.id === selectedTagId.value) ?? null
)

const editDialogOpen = computed({
  get: () => activeAction.value === TagAction.Edit,
  set: (val) => { if (!val) activeAction.value = null }
})

const onEdit = (tagId: string) => {
  selectedTagId.value = tagId
  activeAction.value = TagAction.Edit
}

const onViewProjects = (tagId: string) => {
  selectedTagId.value = tagId
  activeAction.value = TagAction.ViewProjects
}

const onDelete = (tagId: string) => {
  selectedTagId.value = tagId
  activeAction.value = TagAction.Delete
}
</script>

<template>
  <v-container>
    <TagsTable
      v-model:search="search"
      :tags="tags"
      :total-count="totalCount"
      :options="tableOptions"
      @update:options="updateTableOptions"
      @edit="onEdit"
      @view-projects="onViewProjects"
      @delete="onDelete" />
    <TagEditDialog
      v-model="editDialogOpen"
      :tag="selectedTag" />
  </v-container>
</template>
