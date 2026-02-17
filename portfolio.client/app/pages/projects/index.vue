<script setup lang="ts">
import type { ProjectQueryProjectsArgs } from '~/generated/graphql'
import type { TableOptions } from '~/types/ui/datatable'
import { toGraphqlSort } from '~/utils/graphql'

const tableOptions = ref<TableOptions>({
  page: 1,
  itemsPerPage: 10,
  sortBy: [],
  groupBy: [],
  search: ''
})

const queryArgs = computed<ProjectQueryProjectsArgs>(() => {
  const skip = (tableOptions.value.page - 1) * tableOptions.value.itemsPerPage
  const take = tableOptions.value.itemsPerPage
  const includeUnpublished = true
  const order = tableOptions.value.sortBy
    ? toGraphqlSort(tableOptions.value.sortBy)
    : []

  return {
    includeUnpublished,
    skip,
    take,
    order
  }
})

const {
  projects,
  pageInfo,
  totalCount
} = useProjects(queryArgs)

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
