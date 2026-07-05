<script setup lang="ts">
import type { GetTagSummariesQueryVariables } from '~/generated/graphql'
import type { Header, TableOptions } from '~/types/ui/datatable'
import type { MenuItem } from '~/types/ui/MenuItem'
import { toTagSortInput, toTagFilterInput, useTagsTableQueries } from '~/composables/useTagsTableQueries'

type TagHeader = Header<'name' | 'value' | 'projectsCount' | 'action'>

const headers: TagHeader[] = [
  { title: 'Name', key: 'name', sortable: true, align: 'start' },
  { title: 'Value', key: 'value', sortable: true, align: 'start' },
  { title: 'Project Count', key: 'projectsCount', sortable: true, align: 'start' },
  { title: '', key: 'action', sortable: false, align: 'end' }
]

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

const { tags, totalCount } = useTagsTableQueries(queryVars)

const getMenuItems = (tagId: string): MenuItem[] => [
  { title: 'Edit', icon: 'mdi-pencil-outline', action: () => onEdit(tagId) },
  { title: 'View Projects', icon: 'mdi-folder-multiple-outline', action: () => onViewProjects(tagId) },
  { title: 'Delete', icon: 'mdi-trash-can-outline', itemClass: 'text-error', action: () => onDelete(tagId) }
]

const onEdit = (tagId: string) => {
  console.log(tagId)
  // TODO: open edit dialog
}

const onViewProjects = (tagId: string) => {
  console.log(tagId)
  // TODO: navigate or open project list dialog
}

const onDelete = (tagId: string) => {
  console.log(tagId)
  // TODO: open delete confirmation dialog
}
</script>

<template>
  <v-container>
    <BaseTable
      :options="tableOptions"
      :headers="headers"
      :items="tags"
      :items-length="totalCount"
      density="comfortable"
      @update:options="updateTableOptions">
      <template #top>
        <v-container
          fluid
          class="px-0">
          <v-text-field
            v-model="search"
            label="Search"
            append-inner-icon="mdi-magnify"
            clearable
            hide-details />
        </v-container>
      </template>
      <template #item="{ item }">
        <tr>
          <td>
            <v-chip
              color="primary"
              variant="flat"
              class="ma-0"
              label
              small>
              {{ item.name }}
            </v-chip>
          </td>
          <td v-text="item.value" />
          <td
            v-text="item.projectsCount" />
          <td
            class="text-end"
            @click.stop>
            <BaseMenu :items="getMenuItems(item.id)" />
          </td>
        </tr>
      </template>
    </BaseTable>
  </v-container>
</template>
