<script setup lang="ts">
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import ProjectImageUploadListItem from '~/components/projects/edit/images/list/ProjectImageUploadListItem.vue'
import { normalizeEditorItemsSortOrder } from '~/utils/editorItems'

const model = defineModel<ImageEditorItem[]>({ required: true })

defineEmits<{
  (e: 'remove', clientId: string): void
}>()

const itemsLocal = computed({
  get: () => model.value.filter(item => !item.isRemoved),
  set: (value: ImageEditorItem[]) => {
    model.value = normalizeEditorItemsSortOrder(value)
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
