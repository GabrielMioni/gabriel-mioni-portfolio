<script setup lang="ts" generic="T">
import type { Header, TableOptions } from '~/types/ui/datatable'

defineProps<{
  headers?: Header[] | undefined
  density?: 'default' | 'comfortable' | 'compact' | null
  items: T[]
  itemsLength: number
  itemValue?: string
  options: TableOptions
}>()

const emit = defineEmits<{
  (e: 'update:options', options: TableOptions): void
}>()

const expanded = defineModel<string[]>('expanded', {
  default: () => []
})
</script>

<template>
  <v-data-table-server
    v-model:expanded="expanded"
    :density="density ?? 'default'"
    :headers="headers"
    :items="items"
    :items-length="itemsLength"
    :item-value="itemValue ?? 'id'"
    :page="options.page"
    :items-per-page="options.itemsPerPage"
    :sort-by="options.sortBy"
    :group-by="options.groupBy"
    :search="options.search"
    class="base-table"
    @update:options="emit('update:options', $event)">
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

<style lang="scss">
@use '~/assets/scss/components/datatable.scss';
</style>
