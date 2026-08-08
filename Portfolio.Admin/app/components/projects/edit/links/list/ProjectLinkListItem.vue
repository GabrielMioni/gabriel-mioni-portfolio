<script setup lang="ts">
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import type { EditorItemMoveDirection } from '~/utils/editorItems'
import {
  MAX_LINK_TEXT_LENGTH,
  MAX_LINK_URL_LENGTH
} from '~/utils/links/limits'
import { maxLength, required, validateUrl } from '~/utils/rules'

const item = defineModel<LinkEditorItem>('item', { required: true })
const layout = ref<{
  focusMoveButton: (direction: EditorItemMoveDirection) => void
    } | null>(null)
const urlInput = ref<{
  focus: () => void
    } | null>(null)

const props = defineProps<{
  position: number
  itemCount: number
  focusUrl?: boolean
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

watch(
  () => props.focusUrl,
  async (shouldFocus) => {
    if (!shouldFocus) return

    await nextTick()
    urlInput.value?.focus()
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
    :item-label="`link ${position + 1} of ${itemCount}`"
    @action="updateRemovalState"
    @move="emit('move', $event)">
    <v-row dense>
      <v-col
        cols="12"
        md="3">
        <v-text-field
          ref="urlInput"
          v-model="item.url"
          label="Url"
          variant="filled"
          :maxlength="MAX_LINK_URL_LENGTH"
          :counter="MAX_LINK_URL_LENGTH"
          persistent-counter
          :disabled="item.isRemoved"
          :rules="[
            required(),
            validateUrl(),
            maxLength(MAX_LINK_URL_LENGTH, 'URL')
          ]" />
      </v-col>
      <v-col
        cols="12"
        md="3">
        <v-text-field
          v-model="item.text"
          label="Link Text"
          variant="filled"
          :maxlength="MAX_LINK_TEXT_LENGTH"
          :counter="MAX_LINK_TEXT_LENGTH"
          persistent-counter
          :disabled="item.isRemoved"
          :rules="[
            required(),
            maxLength(MAX_LINK_TEXT_LENGTH, 'Link text')
          ]" />
      </v-col>
      <v-col
        cols="12"
        md="4">
        <ProjectLinkSelect
          v-model="item.type"
          :disabled="item.isRemoved" />
      </v-col>
      <v-col
        cols="12"
        md="1"
        class="d-flex align-center">
        <div class="link-details text-break fs-12">
          Created: <br>
          <template v-if="createdAtDate">
            {{ createdAtDate }}
          </template>
          <span
            v-else
            class="font-italic text-grey">
            (pending)
          </span>
        </div>
      </v-col>
    </v-row>
  </EditorItemLayout>
</template>
