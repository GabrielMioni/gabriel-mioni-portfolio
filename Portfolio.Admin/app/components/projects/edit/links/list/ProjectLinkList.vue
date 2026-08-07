<script setup lang="ts">
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import {
  moveEditorItem,
  normalizeEditorItemsSortOrder,
  type EditorItemMoveDirection
} from '~/utils/editorItems'

const model = defineModel<LinkEditorItem[]>({ required: true })

defineProps<{
  focusClientId?: string | null
}>()

defineEmits<{
  (e: 'remove' | 'restore', clientId: string): void
}>()

const itemsLocal = computed({
  get: () => model.value,
  set: (value: LinkEditorItem[]) => {
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
  moveStatus.value = `Link moved to position ${targetIndex + 1} of ${activeItems.value.length}.`
}
</script>

<template>
  <DraggableList v-model="itemsLocal">
    <template #default="{ element }">
      <ProjectLinkListItem
        :item="element"
        :position="getActiveItemIndex(element.clientId)"
        :item-count="activeItems.length"
        :focus-url="focusClientId === element.clientId"
        :focus-request="(moveFocusRequest?.clientId) === element.clientId
          ? moveFocusRequest ?? undefined
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

<style scoped>

</style>
