<script setup lang="ts" generic="T">
import {
  TABLE_ITEMS_PER_PAGE_OPTIONS,
  type Header,
  type TableOptions
} from '~/types/ui/datatable'

defineProps<{
  headers?: Header[] | undefined
  density?: 'default' | 'comfortable' | 'compact' | null
  items: T[]
  itemsLength: number
  itemValue?: string
  options: TableOptions
  emptyTitle?: string
  emptyMessage?: string
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
    :items-per-page-options="TABLE_ITEMS_PER_PAGE_OPTIONS"
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
    <template #no-data>
      <div class="base-table__empty">
        <v-icon
          aria-hidden="true"
          class="base-table__empty-icon"
          icon="mdi-file-search-outline"
          size="32" />
        <p class="base-table__empty-title">
          {{ emptyTitle ?? 'No records found' }}
        </p>
        <p class="base-table__empty-message">
          {{ emptyMessage ?? 'Try changing the current filters.' }}
        </p>
      </div>
    </template>
  </v-data-table-server>
</template>

<style lang="scss">
@use '~/assets/scss/components/datatable.scss';
</style>
