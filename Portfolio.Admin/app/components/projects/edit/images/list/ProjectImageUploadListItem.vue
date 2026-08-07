<script setup lang="ts">
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import type { EditorItemMoveDirection } from '~/utils/editorItems'

const config = useRuntimeConfig()

const item = defineModel<ImageEditorItem>('item', { required: true })
const layout = ref<{
  focusMoveButton: (direction: EditorItemMoveDirection) => void
} | null>(null)

const props = defineProps<{
  position: number
  itemCount: number
  focusRequest?: {
    direction: EditorItemMoveDirection
    sequence: number
  }
}>()

const emit = defineEmits<{
  (e: 'remove' | 'restore', clientId: string): void
  (e: 'move', direction: EditorItemMoveDirection): void
}>()

const updateRemovalState = () => {
  const event = item.value.isRemoved ? 'restore' : 'remove'
  emit(event, item.value.clientId)
}

const imageUrl = computed(() => {
  const current = item.value

  if (!current.id) {
    if (!current.thumbFile) return null
    return URL.createObjectURL(current.thumbFile)
  }

  const base = config.public.storageBase
  const key = current.thumbKey

  if (!base || !key) return null

  return `${base}/${key}`
})

const imageFileName = computed(() => {
  return item.value?.fileName ?? 'Unnamed Image'
})

const createdAtDate = computed(() => {
  if (!item.value?.createdAt) return null
  return new Date(item.value.createdAt).toLocaleDateString()
})

watch(
  () => props.focusRequest,
  async (request) => {
    if (!request) return

    await nextTick()
    layout.value?.focusMoveButton(request.direction)
  }
)
</script>

<template>
  <EditorItemLayout
    ref="layout"
    :draggable="!item.isRemoved"
    :is-pending="!item.id"
    :is-removed="item.isRemoved"
    :can-move-up="position > 0"
    :can-move-down="position >= 0 && position < itemCount - 1"
    :item-label="`image ${position + 1} of ${itemCount}`"
    @action="updateRemovalState"
    @move="emit('move', $event)">
    <template #leading>
      <v-img
        v-if="imageUrl"
        class="ma-2"
        :src="imageUrl"
        width="50" />
    </template>

    <v-row>
      <v-col cols="12">
        <v-text-field
          v-model="item.altText"
          :disabled="item.isRemoved"
          variant="filled"
          label="Alt Text"
          hide-details />
        <div class="image-details d-flex flex-wrap ga-3 mt-2 text-medium-emphasis text-break fs-12">
          <span v-if="createdAtDate">
            {{ createdAtDate }}
          </span>
          <span
            v-else
            class="font-italic">
            {{ imageFileName }} (pending)
          </span>
          <span>{{ item.contentType }}</span>
          <span>{{ (item.sizeFull / 1024).toFixed(2) }} KB</span>
        </div>
      </v-col>
    </v-row>
  </EditorItemLayout>
</template>
