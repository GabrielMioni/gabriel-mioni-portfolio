<script setup lang="ts">
import { useQuery } from '@urql/vue'
import {
  ProjectStatus,
  GetProjectByIdDocument,
  ProjectFragmentDoc
} from '~/generated/graphql'
import { useFragment } from '~/generated'

const isValid = ref(false)
const form = reactive({
  title: '',
  summary: '',
  body: '',
  status: ProjectStatus.Draft
})

const route = useRoute()
const id = route.params?.id ? route.params.id as string : ''

const {
  data,
  fetching,
  error
} = useQuery({
  query: GetProjectByIdDocument,
  variables: {
    id
  }
})

const project = computed(() => {
  const ref = data.value?.projectById
  return ref ? useFragment(ProjectFragmentDoc, ref) : null
})

watch(
  project,
  (project) => {
    if (!project) return

    form.title = project.title ?? ''
    form.summary = project.summary ?? ''
    form.body = project.body ?? ''
    form.status = project.status ?? ProjectStatus.Draft
  },
  { immediate: true }
)

</script>

<template>
  <v-container>
    <v-col
      v-if="fetching"
      class="d-flex justify-center">
      <v-progress-circular
        size="60"
        indeterminate />
    </v-col>
    <v-col v-else>
      <ProjectForm
        v-model:form="form"
        v-model:is-valid="isValid" />
    </v-col>
  </v-container>
</template>

<style scoped>

</style>
