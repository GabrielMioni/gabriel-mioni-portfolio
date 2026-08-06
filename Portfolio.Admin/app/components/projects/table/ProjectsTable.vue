<script setup lang="ts">
import type { Project } from '~/generated/graphql'
import type { Header, TableOptions } from '~/types/ui/datatable'

type ProjectKey = keyof Project
type ActionKey = 'action'
type ColumnKey = ProjectKey | ActionKey

type ProjectHeader = Header<ColumnKey>

const props = defineProps<{
  options: TableOptions
  projects: Project[]
  totalCount: number
}>()

const emit = defineEmits<{
  'update:options': [options: TableOptions]
}>()

const search = defineModel<string>('search', {
  required: true
})

const headers: ProjectHeader[] = [
  {
    title: 'Title',
    align: 'start',
    sortable: true,
    key: 'title'
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
    sortable: false,
    key: 'status'
  },
  {
    title: '',
    align: 'end',
    sortable: false,
    key: 'action'
  }
]

const expanded = ref<string[]>([])
</script>

<template>
  <BaseTable
    v-model:expanded="expanded"
    :options="props.options"
    :headers="headers"
    :items="projects"
    :items-length="totalCount"
    empty-title="No projects found"
    empty-message="Try changing the search, or add a new project."
    density="comfortable"
    @update:options="emit('update:options', $event)">
    <template #top>
      <v-container
        fluid
        class="admin-table-toolbar">
        <v-row no-gutters>
          <v-col>
            <v-text-field
              v-model="search"
              label="Search"
              append-inner-icon="mdi-magnify"
              clearable
              hide-details />
          </v-col>
        </v-row>
      </v-container>
    </template>
    <template #item="{ item, toggleExpand, isExpanded }">
      <ProjectTableRow
        :project="item"
        :toggle-expand="toggleExpand"
        :is-expanded="isExpanded" />
    </template>
    <template #expanded-row="{ item }">
      <tr class="expand-row">
        <td :colspan="headers.length">
          {{ item.body }}
        </td>
      </tr>
    </template>
  </BaseTable>
</template>
