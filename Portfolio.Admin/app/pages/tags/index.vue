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

const { tags, totalCount } = useTagsTableQueries(queryVars)

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
  <v-container
    fluid
    class="admin-page">
    <AdminPageHeader
      eyebrow="Classification index"
      title="Tags"
      description="Maintain the vocabulary used to classify projects."
      :count="totalCount">
      <template #actions>
        <v-btn
          color="primary"
          prepend-icon="mdi-plus"
          @click="createDialogOpen = true">
          New Tag(s)
        </v-btn>
      </template>
    </AdminPageHeader>
    <TagsTable
      v-model:search="search"
      v-model:show-orphaned="showOrphaned"
      class="admin-table-workspace"
      :tags="tags"
      :total-count="totalCount"
      :options="tableOptions"
      @update:options="updateTableOptions"
      @edit="onEdit"
      @delete="onDelete" />
    <TagDialog
      v-model="editDialogOpen"
      :tag="selectedTag" />
    <CreateTagsDialog
      v-model="createDialogOpen" />
    <DeleteTagDialog
      v-model="deleteDialogOpen"
      :tag="selectedTag" />
  </v-container>
</template>
