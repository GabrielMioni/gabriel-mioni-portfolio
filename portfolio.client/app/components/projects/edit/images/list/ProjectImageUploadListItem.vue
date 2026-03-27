<script setup lang="ts">
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'

const config = useRuntimeConfig()

const item = defineModel<ImageEditorItem>('item', { required: true })

withDefaults(
  defineProps<{
    isRemoving?: boolean
  }>(),
  {
    isRemoving: true
  }
)

defineEmits<{
  (e: 'update', clientId: string): void
}>()

const imageUrl = computed(() => {
  const current = item.value

  // New (not yet persisted) image
  if (!current.id) {
    if (!current.thumbFile) return null
    return URL.createObjectURL(current.thumbFile)
  }

  // Existing image
  const base = config.public.storageBase
  const key = current.thumbKey

  if (!base || !key) return null

  return `${base}/${key}`
})

const imageFileName = computed(() => {
  return item.value?.fileName ?? 'Unnamed Image'
})

</script>

<template>
  <v-container
    class="pa-0 project-image-upload-list-item hover-surface"
    fluid>
    <v-row align="center">
      <v-col
        cols="auto"
        class="d-flex align-center justify-center order-1">
        <div class="d-flex align-center">
          <v-icon
            v-if="isRemoving"
            class="drag-handle cursor-grab"
            icon="mdi-drag"/>
          <v-img
            v-if="imageUrl"
            class="ma-2"
            :src="imageUrl"
            width="50" />
        </div>
      </v-col>
      <v-col
        class="order-2"
        sm="2">
        <div class="image-details text-break fs-12">
          <div>{{ imageFileName }}</div>
          <div>{{ item.contentType }}</div>
          <div>{{ (item.sizeFull / 1024).toFixed(2) }} KB</div>
        </div>
      </v-col>
      <v-col
        cols="auto"
        class="d-flex align-center justify-end order-3 order-sm-4">
        <v-btn
          :icon="isRemoving ? 'mdi-close' : 'mdi-plus'"
          class="ma-2"
          variant="text"
          :color="isRemoving ? 'error' : 'success'"
          @click="$emit('update', item.clientId)"/>
      </v-col>
      <v-col
        cols="12"
        sm
        class="order-4 order-sm-3">
        <v-text-field
          v-model="item.altText"
          :disabled="!isRemoving"
          density="compact"
          variant="filled"
          label="Alt Text"
          hide-details />
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>

</style>
