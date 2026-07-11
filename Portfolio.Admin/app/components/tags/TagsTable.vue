<script setup lang="ts">
import type { ProjectTagSummary } from '~/generated/graphql'
import type { Header, TableOptions } from '~/types/ui/datatable'
import type { MenuItem } from '~/types/ui/MenuItem'

type TagHeader = Header<'name' | 'value' | 'projectsCount' | 'action'>

const props = defineProps<{
  tags: ProjectTagSummary[]
  totalCount: number
  options: TableOptions
}>()

const emit = defineEmits<{
  'update:options': [options: TableOptions]
  edit: [tagId: string]
  delete: [tagId: string]
}>()

const search = defineModel<string>('search', { required: true })

const headers: TagHeader[] = [
  { title: 'Name', key: 'name', sortable: true, align: 'start' },
  { title: 'Value', key: 'value', sortable: true, align: 'start' },
  { title: 'Project Count', key: 'projectsCount', sortable: true, align: 'start' },
  { title: '', key: 'action', sortable: false, align: 'end' }
]

const getMenuItems = (tag: ProjectTagSummary): MenuItem[] => [
  { title: 'Edit', icon: 'mdi-pencil-outline', action: () => emit('edit', tag.id) },
  { title: 'Delete', icon: 'mdi-trash-can-outline', itemClass: 'text-error', action: () => emit('delete', tag.id) }
]
</script>

<template>
  <BaseTable
    :options="props.options"
    :headers="headers"
    :items="props.tags"
    :items-length="props.totalCount"
    density="comfortable"
    @update:options="emit('update:options', $event)">
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
        <td v-text="item.projectsCount" />
        <td
          class="text-end"
          @click.stop>
          <BaseMenu :items="getMenuItems(item)" />
        </td>
      </tr>
    </template>
  </BaseTable>
</template>
