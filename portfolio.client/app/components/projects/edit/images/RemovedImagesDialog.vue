<script setup lang="ts">
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'

const dialog = defineModel<boolean>()

defineProps<{
  removedImageItems: ImageEditorItem[]
}>()

defineEmits<{
  (e: 'add', clientId: string): void
}>()

</script>

<template>
  <RemovedItemsDialog
    v-model="dialog"
    title="Removed Images"
    :items="removedImageItems"
    @add="$emit('add', $event)">
    <template #item="{ item, restore }">
      <ProjectImageUploadListItem
        :item="item"
        :is-removing="false"
        @update="restore" />
    </template>
  </RemovedItemsDialog>
</template>
