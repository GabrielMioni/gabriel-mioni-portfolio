<script setup lang="ts">
import type { Items, TableOptions } from '~/types/ui/datatable'

const options = defineModel<TableOptions>('options', {
  default: () => ({
    page: 1,
    itemsPerPage: 10,
    sortBy: [],
    groupBy: [],
    search: ''
  })
})

defineProps<{
  items: NonNullable<Items>
  itemsLength: number
}>()

</script>

<template>
  <v-data-table-server
    v-model:options="options"
    :items="items"
    :items-length="itemsLength" />
</template>
