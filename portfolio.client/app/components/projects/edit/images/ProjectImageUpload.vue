<script setup lang="ts">
import { getOutputMimeType, resizeImageTo } from '~/utils/images'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'

const activeUploadItems = defineModel<ImageEditorItem[]>('items', { required: true })
const removedUploadItems = defineModel<ImageEditorItem[]>('removed', { required: true })

const filesList = ref<File[]>([])

const normalizeSortOrder = (items: ImageEditorItem[]) =>
  items.map((item, index) => ({
    ...item,
    sort: index
  }))

const updateActiveUploadItems = (items: ImageEditorItem[]) => {
  activeUploadItems.value = normalizeSortOrder(items)
}

const updateImageUploadItems = async (files: File[]) => {
  if (files.length === 0) {
    return
  }

  let sort = activeUploadItems.value.length ?? 0
  const items = await Promise.all(files.map(async file => {

    const mimeType = getOutputMimeType(file)

    const resizedThumb = await resizeImageTo(file, 200, 200, mimeType)
    const resizedFull = await resizeImageTo(file, 1600, 1600, mimeType)

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
      sort: ++sort
    }
  }))

  activeUploadItems.value.push(...items)
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
  const index = activeUploadItems.value.findIndex(
    activeItem => activeItem.clientId === clientId
  )
  if (index === -1) return

  const item = activeUploadItems.value[index]
  if (!item) return

  const nextActiveItems = [...activeUploadItems.value]
  nextActiveItems.splice(index, 1)

  removedUploadItems.value.push(item)
  updateActiveUploadItems(nextActiveItems)
}

</script>

<template>
  <v-container
    fluid
    class="pa-0 project-image-upload">
    <v-row>
      <v-col>
        <ProjectImageDropzone v-model="filesList"/>
      </v-col>
    </v-row>
    <v-divider class="my-6" />
    <v-row>
      <v-col>
        <ProjectImageUploadList
          :items="activeUploadItems"
          @update:items="updateActiveUploadItems"
          @remove="removeImage"/>
      </v-col>
    </v-row>
  </v-container>
</template>

<style lang="scss" scoped>

</style>
