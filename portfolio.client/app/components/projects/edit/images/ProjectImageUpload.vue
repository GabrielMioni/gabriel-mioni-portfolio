<script setup lang="ts">
import { getOutputMimeType, resizeImageTo } from '~/utils/images'
import type { ImageUploadItem } from '~/types/images/ImageUploadItem'

const filesList = ref<File[]>([])
const imageUploadItems = ref<ImageUploadItem[]>([])

const updateImageUploadItems = async (files: File[]) => {
  if (files.length === 0) {
    return
  }
  const items = await Promise.all(files.map(async file => {

    const mimeType = getOutputMimeType(file)

    const thumbFile = await resizeImageTo(file, 200, 200, mimeType)
    const fullFile = await resizeImageTo(file, 200, 200, mimeType)

    return {
      clientId: crypto.randomUUID(),
      contentType: file.type,
      sizeThumb: thumbFile.size,
      sizeFull: fullFile.size,
      altText: file.name,
      thumbFile,
      fullFile
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
