<script setup lang="ts">
import type { GetProjectsQueryVariables } from '~/generated/graphql'
import type { TableOptions } from '~/types/ui/datatable'
import { toGraphqlSort } from '~/utils/graphql'

const tableOptions = ref<TableOptions>({
  page: 1,
  itemsPerPage: 10,
  sortBy: [],
  groupBy: [],
  search: ''
})

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
      : undefined
    // where: toGraphqlFilterInput(tableOptions.value.search) ?? undefined
  }
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
          :projects="projects"
          :total-count="totalCount"
          :page-info="pageInfo" />
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>

</style>
