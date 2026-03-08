<script setup lang="ts">
import type { ImageUploadItem } from '~/types/images/ImageUploadItem'

const item = defineModel<ImageUploadItem>('item', { required: true })

defineEmits<{
  (e: 'remove', clientId: string): void
}>()

const imageUrl = computed(() => {
  if (!item.value.thumbFile) return null
  return URL.createObjectURL(item.value.thumbFile)
})

const imageFileName = computed(() => {
  return item.value?.fileName ?? 'Unnamed Image'
})

</script>

<template>
  <v-container
    class="pa-0 project-image-upload-list-item hover-surface"
    fluid>
    <v-row>
      <v-col
        cols="auto"
        class="d-flex align-center justify-center">
        <v-img
          v-if="imageUrl"
          class="ma-2"
          :src="imageUrl"
          width="50"/>
      </v-col>
      <v-col cols="auto">
        <div class="image-details fs-12">
          <div>{{ imageFileName }}</div>
          <div>{{ item.contentType }}</div>
          <div>{{ (item.sizeFull / 1024).toFixed(2) }} KB</div>
        </div>
      </v-col>
      <v-col class="d-flex align-center">
        <v-text-field
          v-model="item.altText"
          variant="filled"
          label="Alt Text"
          hide-details />
      </v-col>
      <v-col
        cols="auto"
        class="d-flex align-center">
        <v-btn
          icon="mdi-close"
          class="ma-2"
          variant="text"
          color="error"
          @click="$emit('remove', item.clientId)" />
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>
.image-details {
  width: 10rem;
  overflow-wrap: break-word;
}
</style>
