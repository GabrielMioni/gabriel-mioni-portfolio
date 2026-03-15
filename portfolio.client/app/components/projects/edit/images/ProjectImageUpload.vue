<script setup lang="ts">
import { getOutputMimeType, resizeImageTo } from '~/utils/images'
import type { ImageUploadItem } from '~/types/images/ImageUploadItem'

const imageUploadItems = defineModel<ImageUploadItem[]>('modelValue', { required: true })

const filesList = ref<File[]>([])

const updateImageUploadItems = async (files: File[]) => {
  if (files.length === 0) {
    return
  }
  const items = await Promise.all(files.map(async file => {

    const mimeType = getOutputMimeType(file)

    const resizedThumb = await resizeImageTo(file, 200, 200, mimeType)
    const resizedFull = await resizeImageTo(file, 1600, 1600, mimeType)

    return {
      clientId: crypto.randomUUID(),
      contentType: file.type,
      fileName: file.name,
      sizeThumb: resizedThumb.blob.size,
      sizeFull: resizedFull.blob.size,
      altText: file.name,
      thumbFile: resizedThumb.blob,
      fullFile: resizedFull.blob,
      height: resizedFull.height,
      width: resizedFull.width
    }
  }))

  imageUploadItems.value.push(...items)
  filesList.value = []
}

watch(
  filesList,
  (files) => {
    updateImageUploadItems(files)
  },
  { deep: true }
)

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
        <ProjectImageUploadList :items="imageUploadItems" />
      </v-col>
    </v-row>
  </v-container>
</template>

<style lang="scss" scoped>

</style>
