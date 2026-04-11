<script setup lang="ts">
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import ProjectImageUploadListItem from '~/components/projects/edit/images/list/ProjectImageUploadListItem.vue'

const items = defineModel<ImageEditorItem[]>('items', { required: true })

defineEmits<{
  (e: 'remove', clientId: string): void
}>()

</script>

<template>
  <DraggableList v-model="items">
    <template #default="{ element }">
      <ProjectImageUploadListItem
        :item="element"
        @update="$emit('remove', $event)" />
    </template>
  </DraggableList>
</template>
