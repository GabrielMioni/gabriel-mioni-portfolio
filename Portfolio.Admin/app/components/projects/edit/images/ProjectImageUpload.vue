<script setup lang="ts">
import { getOutputMimeType, resizeImageTo } from '~/utils/images/'
import {
  normalizeEditorItemsSortOrder,
  removeEditorItem,
  restoreEditorItem
} from '~/utils/editorItems'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import {
  MAX_PROJECT_IMAGES,
  getRemainingCapacity,
  takeItemsWithinCapacity
} from '~/utils/projects/limits'

const imageItems = defineModel<ImageEditorItem[]>('items', { required: true })

const filesList = ref<File[]>([])
const { showSnackbar } = useSnackbarStore()

// Persisted images marked for removal still exist on the server until Save.
const usedImageSlots = computed(() => imageItems.value.length)
const remainingImageSlots = computed(() =>
  getRemainingCapacity(usedImageSlots.value, MAX_PROJECT_IMAGES)
)
const hasRemovedImages = computed(() =>
  imageItems.value.some(item => item.isRemoved)
)

const updateImageUploadItems = async (files: File[]) => {
  if (files.length === 0) return

  const acceptedFiles = takeItemsWithinCapacity(
    files,
    usedImageSlots.value,
    MAX_PROJECT_IMAGES
  )
  const rejectedCount = files.length - acceptedFiles.length

  if (rejectedCount > 0) {
    const saveRemovalsMessage = hasRemovedImages.value
      ? ' Save removed images before adding replacements.'
      : ''

    showSnackbar(
      `${rejectedCount} image${rejectedCount === 1 ? ' was' : 's were'} not added. ` +
      `Projects can have up to ${MAX_PROJECT_IMAGES} images.${saveRemovalsMessage}`,
      'warning'
    )
  }

  if (acceptedFiles.length === 0) {
    filesList.value = []
    return
  }

  let sort = imageItems.value.length ?? 0

  const items = await Promise.all(
    acceptedFiles.map(async file => {
      const mimeType = getOutputMimeType(file)

      const resizedThumb = await resizeImageTo(file, 600, 600, mimeType)
      const resizedFull = await resizeImageTo(file, 2400, 2400, mimeType)

      return {
        id: null,
        clientId: crypto.randomUUID(),
        contentType: file.type,
        fileName: file.name,
        sizeThumb: resizedThumb.blob.size,
        sizeFull: resizedFull.blob.size,
        altText: file.name,
        thumbFile: resizedThumb.blob,
        fullFile: resizedFull.blob,
        height: resizedFull.height,
        width: resizedFull.width,
        isRemoved: false,
        sort: ++sort
      }
    })
  )

  imageItems.value = normalizeEditorItemsSortOrder([
    ...imageItems.value,
    ...items
  ])

  filesList.value = []
}

watch(
  filesList,
  (files) => {
    updateImageUploadItems(files)
  },
  { deep: true }
)

const removeImage = (clientId: string) => {
  imageItems.value = removeEditorItem(clientId, imageItems.value)
}

const restoreImage = (clientId: string) => {
  imageItems.value = restoreEditorItem(clientId, imageItems.value)
}

</script>

<template>
  <v-container
    fluid
    class="pa-0 project-image-upload">
    <v-row>
      <v-col>
        <ProjectImageDropzone
          v-model="filesList"
          :disabled="remainingImageSlots === 0"
          :maximum-count="MAX_PROJECT_IMAGES"
          :remaining-capacity="remainingImageSlots"
          :removed-images-pending="hasRemovedImages" />
      </v-col>
    </v-row>
    <v-divider class="my-6" />
    <v-row>
      <v-col>
        <ProjectImageUploadList
          v-model="imageItems"
          @remove="removeImage"
          @restore="restoreImage" />
      </v-col>
    </v-row>
  </v-container>
</template>

<style lang="scss" scoped>

</style>
