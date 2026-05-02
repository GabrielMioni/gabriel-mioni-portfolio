<script setup lang="ts">
import { useQuery } from '@urql/vue'
import {
  type EditProjectInput,
  type EditProjectImageInput,
  type EditProjectLinkInput,
  GetProjectByIdDocument,
  ProjectFragmentDoc,
  ProjectImageFragmentDoc,
  ProjectLinkFragmentDoc,
  ProjectStatus
} from '~/generated/graphql'
import { useFragment } from '~/generated'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import { imageFragmentToEditorItem } from '~/utils/images/imageEditorItems'
import { linkFragmentToEditorItem } from '~/utils/links/linkEditorItems'
import {
  checkIfEditorItemsUpdated,
  restoreEditorItem,
  normalizeEditorItemsSortOrder
} from '~/utils/editorItems'
import { isLikelyValidHttpUrl } from '~/utils/links'
import EditorItemTabDisplay from '~/components/projects/edit/EditorItemTabDisplay.vue'

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

const projectLinksIsValid = ref(false)
const originalProject = ref<typeof project.value>(null)
const originalImageItems = ref<ImageEditorItem[]>([])
const originalLinkItems = ref<LinkEditorItem[]>([])
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
    .filter((image): image is EditProjectImageInput => image.projectImageId != null),
  links: activeLinkItems.value
    .filter((i): i is LinkEditorItem => i.text.trim().length > 0 && isLikelyValidHttpUrl(i.url))
    .map((i): EditProjectLinkInput => ({
      id: i?.id ?? null,
      linkText: i.text,
      linkType: i.type,
      sortOrder: i.sort,
      url: i.url
    }))
}))

const imageCount = computed(() => activeImageItems.value.length)

const pendingImagesLength = computed(() => {
  return activeImageItems.value.filter(image => !image.id).length
})

const isInitialLoading = computed(() => fetching.value && !project.value)

const uploadItems = computed(() =>
  activeImageItems.value.filter((image): image is ImageEditorItem => !image.id)
)

const newLinkItems = computed(() =>
  activeLinkItems.value.filter((link): link is LinkEditorItem => !link.id && link.text.trim().length > 0 && isLikelyValidHttpUrl(link.url))
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

  const projectLinkFragments = useFragment(
    ProjectLinkFragmentDoc,
    currentProject.links
  )

  const mappedLinkItems = normalizeEditorItemsSortOrder(
    projectLinkFragments
      .map(linkFragmentToEditorItem)
      .sort((a, b) => a.sort - b.sort)
  )

  activeLinkItems.value = mappedLinkItems
  originalLinkItems.value = mappedLinkItems.map(item => ({ ...item }))
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

const hasExistingImageUpdates = computed(() =>
  checkIfEditorItemsUpdated(
    originalImageItems.value,
    activeImageItems.value,
    item => ({
      id: item.id!,
      altText: item.altText,
      sort: item.sort
    })
  )
)

const hasExistingLinkUpdates = computed(() =>
  checkIfEditorItemsUpdated(
    originalLinkItems.value,
    activeLinkItems.value,
    item => ({
      id: item.id!,
      text: item.text,
      url: item.url,
      type: item.type,
      sort: item.sort
    })
  )
)

const hasUpdates = computed(() => {
  if (!project.value || !originalProject.value) return false

  const hasFieldUpdates =
    form.title !== originalProject.value.title ||
    form.summary !== originalProject.value.summary ||
    form.body !== originalProject.value.body ||
    form.status !== originalProject.value.status

  const hasImageUploads = uploadItems.value.length > 0
  const hasImageDeleteItems = removedImageItems.value.length > 0

  const hasNewLinks = newLinkItems.value.length > 0

  return hasFieldUpdates ||
    hasImageUploads ||
    hasImageDeleteItems ||
    hasExistingImageUpdates.value ||
    hasExistingLinkUpdates.value ||
    hasNewLinks
})

const deleteImageIds = computed(() => {
  return removedImageItems.value
    .map(item => item.id)
    .filter((id): id is string => Boolean(id))
})

const hasRemovedItems = computed(() => {
  if (tab.value === tabValues.images) {
    return removedImageItems.value.length > 0
  }
  if (tab.value === tabValues.links) {
    return removedLinkItems.value.length > 0
  }
  return true
})

const removedItemText = computed(() => {
  if (!hasRemovedItems.value) {
    return 'Empty'
  }
  if (tab.value === tabValues.images) {
    return `Removed Images (${removedImageItems.value.length})`
  }
  if (tab.value === tabValues.links) {
    return `Removed Links (${removedLinkItems.value.length})`
  }
  return ''
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
    if (removedLinkItems.value.length > 0) {
      removedLinkItems.value = []
    }
  } catch (error) {
    console.error('Failed to save project', error)
  }
}

const openRemovedItemsDialog = () => {
  if (!hasRemovedItems.value) return
  removedDialog.value = true
}

const restoreImageItem = (clientId: string) => {
  restoreEditorItem(clientId, removedImageItems, activeImageItems)
}

const restoreLinkItem = (clientId: string) => {
  restoreEditorItem(clientId, removedLinkItems, activeLinkItems)
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
            <EditorItemTabDisplay
              label="Images"
              :items="activeImageItems" />
          </v-tab>
          <v-tab :value="tabValues.links">
            <EditorItemTabDisplay
              label="Links"
              :items="activeLinkItems" />
          </v-tab>
        </v-tabs>
        <v-spacer />
        <v-btn
          v-if="tab === tabValues.images || tab === tabValues.links"
          text
          class="mr-3"
          prepend-icon="mdi-trash-can-outline"
          :disabled="!hasRemovedItems"
          @click="openRemovedItemsDialog">
          {{ removedItemText }}
        </v-btn>
        <v-btn
          text
          class="mr-3"
          @click="$router.back()">
          Cancel
        </v-btn>
        <v-btn
          class="bg-primary"
          :disabled="isSavingProject || !projectLinksIsValid || !hasUpdates"
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
                v-model:is-valid="projectLinksIsValid" />
            </v-tabs-window-item>
            <v-tabs-window-item :value="tabValues.images">
              <ProjectImageUpload
                v-model:items="activeImageItems"
                v-model:removed="removedImageItems" />
            </v-tabs-window-item>
            <v-tabs-window-item :value="tabValues.links">
              <ProjectLinks
                v-model:is-valid="projectLinksIsValid"
                v-model:items="activeLinkItems"
                v-model:removed="removedLinkItems" />
            </v-tabs-window-item>
          </div>
        </v-tabs-window>
      </v-card>
    </template>
    <RemovedImagesDialog
      v-if="tab === tabValues.images"
      v-model="removedDialog"
      :removed-image-items="removedImageItems"
      @add="restoreImageItem" />
    <RemovedLinksDialog
      v-if="tab === tabValues.links"
      v-model="removedDialog"
      :removed-link-items="removedLinkItems"
      @add="restoreLinkItem" />
  </v-container>
</template>

<style scoped>

</style>
