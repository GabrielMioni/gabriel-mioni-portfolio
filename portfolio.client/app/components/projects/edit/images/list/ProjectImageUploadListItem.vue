<script setup lang="ts">
import type { ImageUploadItem } from '~/types/images/ImageUploadItem'

const item = defineModel<ImageUploadItem>('item', { required: true })

const imageUrl = computed(() => {
  if (!item.value.thumbFile) return null
  return URL.createObjectURL(item.value.thumbFile)
})

</script>

<template>
  <v-container
    class="pa-0 project-image-upload-list-item"
    fluid>
    <v-row no-gutters>
      <v-col>
        <v-img
          v-if="imageUrl"
          :src="imageUrl"
          width="100"/>
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>

</style>
