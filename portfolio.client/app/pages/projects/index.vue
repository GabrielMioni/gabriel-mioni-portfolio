<script setup lang="ts">
import {
  type ProjectQueryProjectsArgs,
  SortEnumType
} from '~/generated/graphql'

const input = ref<ProjectQueryProjectsArgs>({
  includeUnpublished: true,
  first: 5,
  after: null,
  before: null,
  order: [{ createdAt: SortEnumType.Desc }]
})

const { projects, pageInfo, fetchingProjects, projectError } = useProjects(input.value)

</script>

<template>
  <div v-if="fetchingProjects">
    Loading...
  </div>
  <div v-else-if="projectError">
    Error: {{ projectError }}
  </div>
  <div v-else>
    <pre>{{ projects }}</pre>
    <pre>{{ pageInfo }}</pre>
  </div>
</template>

<style scoped>

</style>
