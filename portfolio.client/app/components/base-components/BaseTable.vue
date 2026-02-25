<script setup lang="ts" generic="T">
import type { Header, TableOptions } from '~/types/ui/datatable'

const options = defineModel<TableOptions>('options', {
  default: () => ({
    page: 1,
    itemsPerPage: 10,
    sortBy: [],
    groupBy: [],
    search: ''
  })
})

const expanded = defineModel<string[]>('expanded', {
  default: () => []
})

withDefaults(defineProps<{
  headers?: Header[] | undefined
  items: T[]
  itemsLength: number
  itemValue?: string
}>(), {
  headers: undefined,
  itemValue: 'id'
})
</script>

<template>
  <v-data-table-server
    v-model:options="options"
    v-model:expanded="expanded"
    :headers="headers"
    :items="items"
    :items-length="itemsLength"
    :item-value="itemValue">
    <template
      v-if="$slots.top"
      #top>
      <slot name="top" />
    </template>
    <template #item="{ item, internalItem, isExpanded, toggleExpand }">
      <slot
        name="item"
        :item="item as T"
        :is-expanded="isExpanded(internalItem)"
        :toggle-expand="() => toggleExpand(internalItem)" />
    </template>
    <template #expanded-row="{ item }">
      <slot
        name="expanded-row"
        :item="item as T" />
    </template>
  </v-data-table-server>
</template>
