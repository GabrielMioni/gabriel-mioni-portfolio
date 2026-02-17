<script setup lang="ts">
import type { Project } from '~/generated/graphql'
import type { TableOptions } from '~/types/ui/datatable'
import BaseTable from '~/components/base-components/BaseTable.vue'

const options = defineModel<TableOptions>('options')

defineProps<{
  projects: Project[],
  totalCount: number
}>()

</script>

<template>
  <BaseTable
    v-model:options="options"
    :items="projects"
    :items-length="totalCount" />
</template>
