<script setup lang="ts">
import { useQuery } from '@urql/vue'
import {
  type EditProjectInput,
  type EditProjectImageInput,
  GetProjectByIdDocument,
  ProjectFragmentDoc,
  ProjectImageFragmentDoc,
  ProjectStatus
} from '~/generated/graphql'
import { useFragment } from '~/generated'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import {
  findImageEditorItemAndIndexByClientId,
  imageFragmentToEditorItem
} from '~/utils/images/imageEditorItems'

const {
  editing,
  editProject
} = useProjectMutations()

const {
  isProcessingImages,
  uploadImages
} = useProjectImageMutations()

const isSavingProject = computed(() =>
  editing.value || isProcessingImages.value
)

const tabValues = {
  details: 'details',
  images: 'images'
} as const

const tab = ref(tabValues.details)
const isValid = ref(false)
const removedDialog = ref(false)

const form = reactive({
  title: '',
  summary: '',
  body: '',
  status: ProjectStatus.Draft
})

const activeImageItems = ref<ImageEditorItem[]>([])
const removedImageItems = ref<ImageEditorItem[]>([])
const hasInitialized = ref(false)

const route = useRoute()
const id = route.params?.id ? route.params.id as string : ''

const {
  data,
  fetching,
  executeQuery
  // TODO: handle error
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
  id,
  title: form.title,
  summary: form.summary,
  body: form.body,
  status: form.status,
  images: activeImageItems.value
    .map((i) => ({
      projectImageId: i.id,
      altText: i.altText
    }))
    .filter((image): image is EditProjectImageInput => image.projectImageId != null)
}))

const isInitialLoading = computed(() => fetching.value && !project.value)

const uploadItems = computed(() =>
  activeImageItems.value.filter((image): image is ImageEditorItem => !image.id)
)

const syncFromProject = (
  currentProject: NonNullable<typeof project.value>
) => {
  form.title = currentProject.title ?? ''
  form.summary = currentProject.summary ?? ''
  form.body = currentProject.body ?? ''
  form.status = currentProject.status ?? ProjectStatus.Draft

  const projectImageFragments = useFragment(
    ProjectImageFragmentDoc,
    currentProject.images
  )

  activeImageItems.value = projectImageFragments
    .map(imageFragmentToEditorItem)
    .sort((a, b) => a.sort - b.sort)
}

const refreshProject = async () => {
  const result = await executeQuery({
    requestPolicy: 'network-only'
  })

  const refreshedProjectRef = result.data?.value?.projectById

  if (!refreshedProjectRef) return

  const refreshedProject = useFragment(ProjectFragmentDoc, refreshedProjectRef)
  syncFromProject(refreshedProject)
}

const submitEditProject = async () => {
  try {
    await editProject(updateInput.value)

    if (uploadItems.value.length > 0) {
      await uploadImages({
        uploadItems: uploadItems.value,
        projectId: id
      })
    }

    await refreshProject()
  } catch (error) {
    console.error('Failed to save project', error)
  }
}

const openRemoveImagesDialog = () => {
  if (removedImageItems.value.length <= 0) return
  removedDialog.value = true
}

const restoreImage = (clientId: string) => {
  const result = findImageEditorItemAndIndexByClientId(clientId, removedImageItems.value)
  if (!result) return

  const { item, index } = result

  const nextRemovedItems = [...removedImageItems.value]
  nextRemovedItems.splice(index, 1)
  removedImageItems.value = nextRemovedItems

  activeImageItems.value.push({
    ...item,
    sort: activeImageItems.value.length
  })
}

watch(
  project,
  (currentProject) => {
    if (!currentProject || hasInitialized.value) return

    syncFromProject(currentProject)
    hasInitialized.value = true
  },
  { immediate: true }
)
</script>

<template>
  <v-container>
    <div
      v-if="isInitialLoading"
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
          <v-tab :value="tabValues.images">Images ({{ activeImageItems.length }})</v-tab>
        </v-tabs>
        <v-spacer />
        <v-btn
          text
          class="mr-3"
          prepend-icon="mdi-trash-can-outline"
          :disabled="removedImageItems.length <= 0"
          @click="openRemoveImagesDialog">
          Removed Images ({{ removedImageItems.length > 0 ? removedImageItems.length : 'Empty' }})
        </v-btn>
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
              v-model:items="activeImageItems"
              v-model:removed="removedImageItems" />
          </v-tabs-window-item>
        </div>
      </v-tabs-window>
    </v-card>
    <RemovedImagesDialog
      v-model="removedDialog"
      :removed-image-items="removedImageItems"
      @add="restoreImage"/>
  </v-container>
</template>

<style scoped>

</style>
