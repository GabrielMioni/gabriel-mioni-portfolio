<script setup lang="ts">
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import ProjectImageUploadListItem from '~/components/projects/edit/images/list/ProjectImageUploadListItem.vue'
import {
  moveEditorItem,
  normalizeEditorItemsSortOrder,
  type EditorItemMoveDirection
} from '~/utils/editorItems'

const model = defineModel<ImageEditorItem[]>({ required: true })

defineEmits<{
  (e: 'remove' | 'restore', clientId: string): void
}>()

const itemsLocal = computed({
  get: () => model.value,
  set: (value: ImageEditorItem[]) => {
    model.value = normalizeEditorItemsSortOrder(value)
  }
})

const activeItems = computed(() => model.value.filter(item => !item.isRemoved))
const moveStatus = ref('')
const moveFocusRequest = ref<{
  clientId: string
  direction: EditorItemMoveDirection
  sequence: number
} | null>(null)

const getActiveItemIndex = (clientId: string) =>
  activeItems.value.findIndex(item => item.clientId === clientId)

const moveItem = (clientId: string, direction: EditorItemMoveDirection) => {
  const currentIndex = getActiveItemIndex(clientId)
  const targetIndex = currentIndex + (direction === 'up' ? -1 : 1)

  if (targetIndex < 0 || targetIndex >= activeItems.value.length) return

  model.value = moveEditorItem(clientId, model.value, direction)
  moveFocusRequest.value = {
    clientId,
    direction: targetIndex === 0
      ? 'down'
      : targetIndex === activeItems.value.length - 1
        ? 'up'
        : direction,
    sequence: (moveFocusRequest.value?.sequence ?? 0) + 1
  }
  moveStatus.value = `Image moved to position ${targetIndex + 1} of ${activeItems.value.length}.`
}
</script>

<template>
  <DraggableList v-model="itemsLocal">
    <template #default="{ element }">
      <ProjectImageUploadListItem
        :item="element"
        :position="getActiveItemIndex(element.clientId)"
        :item-count="activeItems.length"
        :focus-request="moveFocusRequest?.clientId === element.clientId
          ? moveFocusRequest
          : undefined"
        @remove="$emit('remove', $event)"
        @restore="$emit('restore', $event)"
        @move="moveItem(element.clientId, $event)" />
    </template>
  </DraggableList>
  <p
    class="d-sr-only"
    aria-live="polite">
    {{ moveStatus }}
  </p>
</template>
