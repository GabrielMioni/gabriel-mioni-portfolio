<script setup lang="ts">
import type { Project } from '~/generated/graphql'
import type { Header, TableOptions } from '~/types/ui/datatable'

type ProjectKey = keyof Project
type ActionKey = 'action'
type ColumnKey = ProjectKey | ActionKey

type ProjectHeader = Header<ColumnKey>

const headers: ProjectHeader[] = [
  {
    title: 'Title',
    align: 'start',
    sortable: true,
    key: 'title'
  },
  {
    title: 'Body',
    align: 'start',
    sortable: true,
    key: 'body'
  },
  {
    title: 'Published Date',
    align: 'start',
    sortable: true,
    key: 'publishedAt'
  },
  {
    title: 'Created Date',
    align: 'start',
    sortable: true,
    key: 'createdAt'
  },
  {
    title: 'Last Updated Date',
    align: 'start',
    sortable: true,
    key: 'updatedAt'
  },
  {
    title: 'Summary',
    align: 'start',
    sortable: true,
    key: 'summary'
  },
  {
    title: 'Status',
    align: 'start',
    sortable: true,
    key: 'status'
  },
  {
    title: '',
    align: 'end',
    sortable: false,
    key: 'action'
  }
]

const options = defineModel<TableOptions>('options')
const search = defineModel<string>('search')

defineProps<{
  projects: Project[],
  totalCount: number
}>()

</script>

<template>
  <BaseTable
    v-model:options="options"
    v-model:search="search"
    :headers="headers"
    :items="projects"
    :items-length="totalCount"
    append-icon="mdi-magnify"
    use-search>
    <template #item="{ item }">
      <ProjectTableRow
        :project="item" />
    </template>
  </BaseTable>
</template>
