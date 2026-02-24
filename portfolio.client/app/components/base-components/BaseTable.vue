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

withDefaults(
  defineProps<{
    headers?: Header[] | undefined
    items: T[]
    itemsLength: number
  }>(),
  {
    headers: undefined
  }
)

</script>

<template>
  <v-data-table-server
    v-model:options="options"
    :headers="headers"
    :items="items"
    :items-length="itemsLength">
    <template
      v-if="$slots.top"
      #top>
      <slot name="top" />
    </template>
    <template #item="{ item }">
      <slot
        name="item"
        :item="item as T" />
    </template>
  </v-data-table-server>
</template>
