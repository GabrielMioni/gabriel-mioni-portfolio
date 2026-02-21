<script setup lang="ts">
import type { GetProjectsQueryVariables } from '~/generated/graphql'
import type { TableOptions } from '~/types/ui/datatable'
import { toGraphqlSort, toGraphqlFilterInput } from '~/utils/graphql'

const tableOptions = ref<TableOptions>({
  page: 1,
  itemsPerPage: 10,
  sortBy: [],
  groupBy: [],
  search: ''
})
const search = ref<string>('')

const queryVars = computed<GetProjectsQueryVariables>(() => {
  const options = tableOptions.value
  const skip = (options.page - 1) * options.itemsPerPage
  const take = options.itemsPerPage

  return {
    skip,
    take,
    includeUnpublished: true,
    order: options.sortBy?.length
      ? toGraphqlSort(options.sortBy)
      : undefined,
    where: toGraphqlFilterInput(tableOptions.value.search) ?? undefined
  }
})

watchDebounced(
  search,
  (val) => {
    const next = val.trim()
    if (next === tableOptions.value.search) return
    tableOptions.value.search = next
    tableOptions.value.page = 1
  },
  { debounce: 350, maxWait: 1000 }
)

const onEdit = (id: string) => {
  console.log('edit', id)
}
const onDelete = (id: string) => {
  console.log('delete', id)
}

provide('projectActions', {
  edit: onEdit,
  delete: onDelete
})


const {
  projects,
  pageInfo,
  totalCount
} = useProjects(queryVars)

</script>

<template>
  <v-container>
    <v-row>
      <v-col>
        <ProjectsTable
          v-model:options="tableOptions"
          v-model:search="search"
          :projects="projects"
          :total-count="totalCount"
          :page-info="pageInfo" />
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>

</style>
