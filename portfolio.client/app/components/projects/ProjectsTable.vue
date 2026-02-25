<script setup lang="ts">
import type { Project } from '~/generated/graphql'
import type { Header, TableOptions } from '~/types/ui/datatable'
import type { MenuItem } from '~/types/ui/MenuItem'

type ProjectKey = keyof Project
type ActionKey = 'action'
type ColumnKey = ProjectKey | ActionKey

type ProjectHeader = Header<ColumnKey>

const emits = defineEmits<{
  (e: 'create-project'): void
}>()

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
    title: 'Body',
    align: 'start',
    sortable: true,
    key: 'body'
  },
  {
    title: 'Created',
    align: 'start',
    sortable: true,
    key: 'createdAt'
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

const menuItems: MenuItem[] = [
  {
    title: 'Create Project',
    icon: 'mdi-pencil',
    action: () => {
      emits('create-project')
    }
  }
]

const options = defineModel<TableOptions>('options')
const search = defineModel<string>('search')

defineProps<{
  projects: Project[],
  totalCount: number
}>()

const expanded = ref<string[]>([])

</script>

<template>
  <BaseTable
    v-model:options="options"
    v-model:expanded="expanded"
    :headers="headers"
    :items="projects"
    :items-length="totalCount">
    <template #top>
      <v-row>
        <v-col>
          <v-text-field
            v-model="search"
            label="Search"
            append-inner-icon="mdi-magnify"
            clearable
            hide-details
            class="mx-4" />
        </v-col>
        <v-col
          cols="auto"
          align-self="center"
          justify="end">
          <BaseMenu :items="menuItems"/>
        </v-col>
      </v-row>
    </template>
    <template #item="{ item, toggleExpand, isExpanded }">
      <ProjectTableRow
        :project="item"
        :toggle-expand="toggleExpand"
        :is-expanded="isExpanded" />
    </template>
    <template #expanded-row="{ item }">
      <tr>
        <td :colspan="headers.length">
          {{ item.body }}
        </td>
      </tr>
    </template>
  </BaseTable>
</template>
