<script setup lang="ts">
import type { ImageUploadItem } from '~/types/images/ImageUploadItem'
import ProjectImageUploadListItem from '~/components/projects/edit/images/list/ProjectImageUploadListItem.vue'

const items = defineModel<ImageUploadItem[]>('items', { required: true })

defineEmits<{
  (e: 'remove', clientId: string): void
}>()

const removeItem = (clientId: string) => {
  const index = items.value.findIndex(item => item.clientId === clientId)
  if (index !== -1) {
    items.value.splice(index, 1)
  }
}

</script>

<template>
  <div class="project-image-upload-list">
    <template
      v-for="(item, index) in items"
      :key="item.clientId">
      <ProjectImageUploadListItem
        :item="item"
        @remove="removeItem"/>
      <v-divider
        v-if="index !== items.length - 1"
        class="my-3"/>
    </template>
  </div>
</template>

<style scoped>

</style>
