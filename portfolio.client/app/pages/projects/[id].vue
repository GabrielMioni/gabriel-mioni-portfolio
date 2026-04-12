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
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import { imageFragmentToEditorItem } from '~/utils/images/imageEditorItems'
import {
  findEditorItemAndIndexByClientId,
  normalizeEditorItemsSortOrder
} from '~/utils/editorItems'

const {
  editing,
  editProject
} = useProjectMutations()

const {
  isProcessingImages,
  deleteImageUploads,
  uploadImages
} = useProjectImageMutations()

const isSavingProject = computed(() =>
  editing.value || isProcessingImages.value
)

const enum tabValues {
  details = 'details',
  images = 'images',
  links = 'links'
}

const isValid = ref(false)
const originalProject = ref<typeof project.value>(null)
const originalImageItems = ref<ImageEditorItem[]>([])
const removedDialog = ref(false)
const tab = ref<string>(tabValues.details)

const form = reactive({
  title: '',
  summary: '',
  body: '',
  status: ProjectStatus.Draft
})

const activeImageItems = ref<ImageEditorItem[]>([])
const removedImageItems = ref<ImageEditorItem[]>([])

const activeLinkItems = ref<LinkEditorItem[]>([])
const removedLinkItems = ref<LinkEditorItem[]>([])

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
      altText: i.altText,
      sortOrder: i.sort
    }))
    .filter((image): image is EditProjectImageInput => image.projectImageId != null)
}))

const imageCount = computed(() => activeImageItems.value.length)

const pendingImagesLength = computed(() => {
  return activeImageItems.value.filter(image => !image.id).length
})

const isInitialLoading = computed(() => fetching.value && !project.value)

const uploadItems = computed(() =>
  activeImageItems.value.filter((image): image is ImageEditorItem => !image.id)
)

const syncFromProject = (
  currentProject: NonNullable<typeof project.value>
) => {
  originalProject.value = currentProject

  form.title = currentProject.title ?? ''
  form.summary = currentProject.summary ?? ''
  form.body = currentProject.body ?? ''
  form.status = currentProject.status ?? ProjectStatus.Draft

  const projectImageFragments = useFragment(
    ProjectImageFragmentDoc,
    currentProject.images
  )

  const mappedImageItems = normalizeEditorItemsSortOrder(
    projectImageFragments
      .map(imageFragmentToEditorItem)
      .sort((a, b) => a.sort - b.sort)
  )

  activeImageItems.value = mappedImageItems
  originalImageItems.value = mappedImageItems.map(item => ({ ...item }))
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

const hasExistingImageUpdates = computed(() => {
  const currentExisting = activeImageItems.value
    .filter((item): item is ImageEditorItem & { id: string } => Boolean(item.id))
    .map(item => ({
      id: item.id,
      altText: item.altText,
      sort: item.sort
    }))
    .sort((a, b) => a.sort - b.sort)

  const originalExisting = originalImageItems.value
    .filter((item): item is ImageEditorItem & { id: string } => Boolean(item.id))
    .map(item => ({
      id: item.id,
      altText: item.altText,
      sort: item.sort
    }))
    .sort((a, b) => a.sort - b.sort)

  if (currentExisting.length !== originalExisting.length) {
    return true
  }
  console.log('Comparing existing images', { currentExisting, originalExisting })

  return currentExisting.some((item, index) => {
    const original = originalExisting[index]
    if (!original) return true

    return (
      item.id !== original.id ||
        item.altText !== original.altText ||
        item.sort !== original.sort
    )
  })
})

const hasUpdates = computed(() => {
  if (!project.value || !originalProject.value) return false

  const hasFieldUpdates =
    form.title !== originalProject.value.title ||
    form.summary !== originalProject.value.summary ||
    form.body !== originalProject.value.body ||
    form.status !== originalProject.value.status

  const hasImageUploads = uploadItems.value.length > 0
  const hasDeleteItems = removedImageItems.value.length > 0

  return hasFieldUpdates || hasImageUploads || hasDeleteItems || hasExistingImageUpdates.value
})

const deleteImageIds = computed(() => {
  return removedImageItems.value
    .map(item => item.id)
    .filter((id): id is string => Boolean(id))
})

const handleDeleteItems = async () => {
  await deleteImageUploads({
    projectId: id,
    projectImageIds: deleteImageIds.value
  })
}

const submitEditProject = async () => {
  if (!hasUpdates.value) return
  try {
    await editProject(updateInput.value)

    if (uploadItems.value.length > 0) {
      await uploadImages({
        uploadItems: uploadItems.value,
        projectId: id
      })
    }

    if (deleteImageIds.value.length > 0) {
      await handleDeleteItems()
      removedImageItems.value = []
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
  const result = findEditorItemAndIndexByClientId(clientId, removedImageItems.value)
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
    <template v-else>
      <v-toolbar
        color="background"
        class="position-sticky top-0"
        style="z-index: 999">
        <v-tabs v-model="tab">
          <v-tab :value="tabValues.details">Details</v-tab>
          <v-tab :value="tabValues.images">
            <!-- Eliminate whitespace in parentheses -->
            Images (
            <span>{{ imageCount }}</span
            ><template v-if="pendingImagesLength > 0"
            ><span> • </span
            ><span>{{ pendingImagesLength }} pending</span></template
            >)
          </v-tab>
          <v-tab :value="tabValues.links">Links</v-tab>
        </v-tabs>
        <v-spacer />
        <v-btn
          v-if="tab === tabValues.images"
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
          :disabled="isSavingProject || !isValid || !hasUpdates"
          :loading="isSavingProject"
          @click="submitEditProject">
          Save
        </v-btn>
      </v-toolbar>
      <v-card
        flat>
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
            <v-tabs-window-item :value="tabValues.links">
              <ProjectLinks
                v-model:items="activeLinkItems"
                v-model:removed="removedLinkItems" />
            </v-tabs-window-item>
          </div>
        </v-tabs-window>
      </v-card>
    </template>
    <RemovedImagesDialog
      v-model="removedDialog"
      :removed-image-items="removedImageItems"
      @add="restoreImage"/>
  </v-container>
</template>

<style scoped>

</style>
