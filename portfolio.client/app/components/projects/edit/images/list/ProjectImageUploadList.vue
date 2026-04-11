<script setup lang="ts">
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import ProjectImageUploadListItem from '~/components/projects/edit/images/list/ProjectImageUploadListItem.vue'
import { normalizeImageEditorItemSortOrder } from '~/utils/images/imageEditorItems'

const model = defineModel<ImageEditorItem[]>({ required: true })

defineEmits<{
  (e: 'remove', clientId: string): void
}>()

const itemsLocal = computed({
  get: () => model.value,
  set: (value: ImageEditorItem[]) => {
    model.value = normalizeImageEditorItemSortOrder(value)
  }
})
</script>

<template>
  <DraggableList v-model="itemsLocal">
    <template #default="{ element }">
      <ProjectImageUploadListItem
        :item="element"
        @update="$emit('remove', $event)" />
    </template>
  </DraggableList>
</template>
