<script setup lang="ts">
import { useQuery } from '@urql/vue'
import {
  type EditProjectInput,
  GetProjectByIdDocument,
  ProjectFragmentDoc,
  ProjectImageFragmentDoc,
  ProjectStatus
} from '~/generated/graphql'
import { useFragment } from '~/generated'
import { imageFragmentToEditorItem } from '~/utils/imageUpload'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'

const {
  editing,
  editProject
} = useProjectMutations()

const {
  isProcessingImages,
  uploadImages
} = useProjectImageMutations()

const isSavingProject = computed(() => editing.value || isProcessingImages.value)

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
const imageItems = ref<ImageEditorItem[]>([])

const route = useRoute()
const id = route.params?.id ? route.params.id as string : ''

const {
  data,
  fetching
  // #TODO: handle error
  // error
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

const updateInput = computed<EditProjectInput>(() => ({
  id: id,
  title: form.title,
  summary: form.summary,
  body: form.body,
  status: form.status
}))

const submitEditProject = async () => {
  try {
    await editProject(updateInput.value)
    if (imageItems.value.length > 0) {
      await uploadImages({ uploadItems: imageItems.value, projectId: id })
    }
  } catch (error) {
    console.error('Failed to save project', error)
  }
}

watch(
  project,
  (project) => {
    if (!project) return

    form.title = project.title ?? ''
    form.summary = project.summary ?? ''
    form.body = project.body ?? ''
    form.status = project.status ?? ProjectStatus.Draft
    const projectImageFragments = useFragment(ProjectImageFragmentDoc, project.images)

    imageItems.value = projectImageFragments
      .map(projectImageFragment => imageFragmentToEditorItem(projectImageFragment))
      .filter(
        (item): item is ImageEditorItem => item !== null
      )
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
    <v-card
      v-else
      flat>
      <v-toolbar color="transparent">
        <v-tabs v-model="tab">
          <v-tab :value="tabValues.details">Details</v-tab>
          <v-tab :value="tabValues.images">Images</v-tab>
        </v-tabs>
        <v-spacer />
        <v-btn
          text
          class="mr-3"
          @click="$router.back()">
          Cancel
        </v-btn>
        <v-btn
          class="bg-primary"
          :loading="isSavingProject"
          @click="submitEditProject">
          Save
        </v-btn>
      </v-toolbar>
      <v-tabs-window v-model="tab">
        <div class="mt-3">
          <v-tabs-window-item :value="tabValues.details">
            <ProjectForm
              v-model:form="form"
              v-model:is-valid="isValid" />
          </v-tabs-window-item>
          <v-tabs-window-item :value="tabValues.images">
            <ProjectImageUpload
              v-model="imageItems"/>
          </v-tabs-window-item>
        </div>
      </v-tabs-window>
    </v-card>
  </v-container>
</template>

<style scoped>

</style>
