<script setup lang="ts">
import type { ImageUploadItem } from '~/types/images/ImageUploadItem'
import ProjectImageUploadListItem from '~/components/projects/edit/images/list/ProjectImageUploadListItem.vue'

const items = defineModel<ImageUploadItem[]>('items', { required: true })

</script>

<template>
  <div class="project-image-upload-list">
    <ProjectImageUploadListItem
      v-for="item in items"
      :key="item.clientId"
      :item="item" />
  </div>
</template>

<style scoped>

</style>
