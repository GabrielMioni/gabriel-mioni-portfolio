<script setup lang="ts">
import type { Items, Header, TableOptions } from '~/types/ui/datatable'

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
    appendIcon?: string | undefined,
    headers?: Header[] | undefined,
    items: NonNullable<Items>
    itemsLength: number,
    searchLabel?: string,
    useSearch?: boolean
  }>(),
  {
    appendIcon: undefined,
    headers: undefined,
    searchLabel: 'Search',
    useSearch: false
  }
)

const search = defineModel<string>('search')

</script>

<template>
  <v-data-table-server
    v-model:options="options"
    :headers
    :items
    :items-length>
    <template
      v-if="useSearch"
      #top>
      <v-text-field
        v-model="search"
        :label="searchLabel"
        :append-inner-icon="appendIcon"
        clearable
        hide-details
        class="mx-4" />
    </template>
  </v-data-table-server>
</template>
