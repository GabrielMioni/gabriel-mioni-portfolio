<script setup lang="ts">
import type { GetTagSummariesQueryVariables } from '~/generated/graphql'
import { toTagSortInput, toTagFilterInput, useTagsTableQueries } from '~/composables/useTagsTableQueries'

enum TagAction {
  Edit = 'edit',
  Delete = 'delete'
}

const route = useRoute()

const activeAction = ref<TagAction | null>(route.query.tagId ? TagAction.Edit : null)
const selectedTagId = ref<string | null>(
  typeof route.query.tagId === 'string' ? route.query.tagId : null
)
const showOrphaned = ref(route.query.showOrphaned !== 'false')

const extra = computed((): Record<string, string> => {
  const result: Record<string, string> = {}

  if (activeAction.value === TagAction.Edit && selectedTagId.value)
    result.tagId = selectedTagId.value
  if (!showOrphaned.value)
    result.showOrphaned = 'false'

  return result
})

const {
  tableOptions,
  search,
  updateTableOptions
} = useTableUrlSync({
  defaultItemsPerPage: 10,
  defaultSort: { key: 'name', order: 'asc' },
  extra
})

const queryVars = computed<GetTagSummariesQueryVariables>(() => {
  const { page, itemsPerPage, sortBy } = tableOptions.value
  return {
    skip: (page - 1) * itemsPerPage,
    take: itemsPerPage,
    order: sortBy?.length ? toTagSortInput(sortBy) : undefined,
    where: toTagFilterInput(tableOptions.value.search),
    showOrphaned: showOrphaned.value
  }
})

const { tags, totalCount, refetchTags } = useTagsTableQueries(queryVars)

const createDialogOpen = ref(false)

const selectedTag = computed(() =>
  tags.value.find(t => t.id === selectedTagId.value) ?? null
)

const editDialogOpen = computed({
  get: () => activeAction.value === TagAction.Edit,
  set: (val) => { if (!val) activeAction.value = null }
})

const deleteDialogOpen = computed({
  get: () => activeAction.value === TagAction.Delete,
  set: (val) => { if (!val) activeAction.value = null }
})

const onEdit = (tagId: string) => {
  selectedTagId.value = tagId
  activeAction.value = TagAction.Edit
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
      v-model:show-orphaned="showOrphaned"
      :tags="tags"
      :total-count="totalCount"
      :options="tableOptions"
      @update:options="updateTableOptions"
      @new-tag="createDialogOpen = true"
      @edit="onEdit"
      @delete="onDelete" />
    <TagDialog
      v-model="editDialogOpen"
      :tag="selectedTag"
      @save="refetchTags"
      @deleted="refetchTags" />
    <CreateTagsDialog
      v-model="createDialogOpen"
      @created="refetchTags" />
    <DeleteTagDialog
      v-model="deleteDialogOpen"
      :tag="selectedTag"
      @deleted="refetchTags" />
  </v-container>
</template>
