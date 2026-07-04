<script setup lang="ts">
import type { ProjectTagFragment } from '~/generated/graphql'
import type { Header, TableOptions } from '~/types/ui/datatable'

type TagKey = keyof ProjectTagFragment
type TagHeader = Header<TagKey>

const { allTags, fetchingTags } = useProjectTagQueries()

const options = ref<TableOptions>({
  page: 1,
  itemsPerPage: -1,
  sortBy: [{ key: 'name', order: 'asc' }],
  groupBy: []
})

const headers: TagHeader[] = [
  { title: 'Name', key: 'name', sortable: true, align: 'start' },
  { title: 'Value', key: 'value', sortable: true, align: 'start' }
]
</script>

<template>
  <v-container>
    <BaseTable
      :options="options"
      :headers="headers"
      :items="allTags"
      :items-length="allTags.length"
      density="comfortable"
      @update:options="options = $event">
      <template #item="{ item }">
        <tr style="cursor: pointer">
          <td v-text="item.name" />
          <td v-text="item.value" />
        </tr>
      </template>
    </BaseTable>
  </v-container>
</template>
