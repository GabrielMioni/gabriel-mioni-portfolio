<script setup lang="ts">
import { useQuery } from '@urql/vue'
import {
  ProjectStatus,
  GetProjectByIdDocument,
  ProjectFragmentDoc
} from '~/generated/graphql'
import { useFragment } from '~/generated'
import ProjectImageUpload from '~/components/projects/edit/ProjectImageUpload.vue'

const tabValues = {
  details: 'details',
  images: 'images'
}

const tab = ref(tabValues.details)
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
    <div
      v-if="fetching"
      class="d-flex justify-center">
      <v-progress-circular
        size="60"
        indeterminate />
    </div>
    <div v-else>
      <v-tabs v-model="tab">
        <v-tab :value="tabValues.details">Details</v-tab>
        <v-tab :value="tabValues.images">Images</v-tab>
      </v-tabs>

      <v-tabs-window v-model="tab">
        <div class="mt-3">
          <v-tabs-window-item :value="tabValues.details">
            <ProjectForm
              v-model:form="form"
              v-model:is-valid="isValid" />
          </v-tabs-window-item>
          <v-tabs-window-item :value="tabValues.images">
            <ProjectImageUpload />
          </v-tabs-window-item>
        </div>
      </v-tabs-window>
    </div>
  </v-container>
</template>

<style scoped>

</style>
