<script setup lang="ts">
import { getOutputMimeType, resizeImageTo } from '~/utils/images/'
import {
  normalizeEditorItemsSortOrder,
  removeEditorItem
} from '~/utils/editorItems'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'

const imageItems = defineModel<ImageEditorItem[]>('items', { required: true })

const filesList = ref<File[]>([])

const updateImageUploadItems = async (files: File[]) => {
  if (files.length === 0) return

  let sort = imageItems.value.length ?? 0

  const items = await Promise.all(
    files.map(async file => {
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

</script>

<template>
  <v-container
    fluid
    class="pa-0 project-image-upload">
    <v-row>
      <v-col>
        <ProjectImageDropzone v-model="filesList" />
      </v-col>
    </v-row>
    <v-divider class="my-6" />
    <v-row>
      <v-col>
        <ProjectImageUploadList
          v-model="imageItems"
          @remove="removeImage" />
      </v-col>
    </v-row>
  </v-container>
</template>

<style lang="scss" scoped>

</style>
