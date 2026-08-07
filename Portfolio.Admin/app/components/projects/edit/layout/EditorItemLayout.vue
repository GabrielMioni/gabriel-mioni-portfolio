<script setup lang="ts">
import type { EditorItemMoveDirection } from '~/utils/editorItems'

const moveActions = useTemplateRef<HTMLElement>('move-actions')
withDefaults(
  defineProps<{
      draggable?: boolean
      isRemoved?: boolean
      isPending?: boolean
      compact?: boolean
      canMoveUp?: boolean
      canMoveDown?: boolean
      itemLabel?: string
    }>(),
  {
    draggable: false,
    isRemoved: false,
    isPending: false,
    compact: false,
    canMoveUp: false,
    canMoveDown: false,
    itemLabel: 'item'
  }
)

defineEmits<{
  (e: 'action'): void
  (e: 'move', direction: 'up' | 'down'): void
}>()

const focusMoveButton = (direction: EditorItemMoveDirection) => {
  moveActions.value
    ?.querySelector<HTMLButtonElement>(`[data-editor-item-move="${direction}"]:not(:disabled)`)
    ?.focus()
}

defineExpose({ focusMoveButton })
</script>

<template>
  <v-container
    fluid
    class="py-0 editor-list-item-layout hover-surface"
    :class="{
      'editor-list-item-layout--pending': isPending,
      'editor-list-item-layout--removed': isRemoved
    }">
    <v-row
      :class="{ 'editor-list-item-layout--compact': compact }">
      <v-col
        v-if="draggable || $slots.leading"
        cols="auto"
        class="d-flex align-center justify-center order-1 editor-list-item-layout__editable">
        <div class="d-flex align-center">
          <v-icon
            v-if="draggable"
            class="drag-handle cursor-grab"
            icon="mdi-drag" />
          <slot name="leading" />
        </div>
      </v-col>
      <v-col class="order-2 editor-list-item-layout__editable">
        <slot />
      </v-col>
      <v-col
        cols="12"
        md="auto"
        class="d-flex align-center justify-end order-3 ga-2 editor-list-item-layout__actions">
        <slot name="actions">
          <div
            v-if="!isRemoved && (canMoveUp || canMoveDown)"
            ref="move-actions">
            <v-btn
              :aria-label="`Move ${itemLabel} up`"
              data-editor-item-move="up"
              :disabled="!canMoveUp"
              icon="mdi-arrow-up"
              size="small"
              variant="text"
              @click="$emit('move', 'up')" />
            <v-btn
              :aria-label="`Move ${itemLabel} down`"
              data-editor-item-move="down"
              :disabled="!canMoveDown"
              icon="mdi-arrow-down"
              size="small"
              variant="text"
              @click="$emit('move', 'down')" />
          </div>
          <span
            v-if="isRemoved"
            class="text-caption text-medium-emphasis"
            aria-live="polite">
            Will be removed
          </span>
          <v-btn
            variant="text"
            size="small"
            :color="isRemoved ? 'primary' : 'error'"
            @click="$emit('action')">
            {{ isRemoved ? 'Undo' : 'Remove' }}
          </v-btn>
        </slot>
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>
.editor-list-item-layout--compact {
  min-height: 72px;
}

.editor-list-item-layout {
  border-inline-start: 3px solid transparent;
  transition: background-color 150ms ease, border-color 150ms ease;
}

.editor-list-item-layout--pending {
  /*noinspection CssUnresolvedCustomProperty*/
  background-color: rgba(var(--v-theme-warning), 0.08);
  /*noinspection CssUnresolvedCustomProperty*/
  border-inline-start-color: rgba(var(--v-theme-warning), 0.65);
}

.editor-list-item-layout--removed {
  /*noinspection CssUnresolvedCustomProperty*/
  background-color: rgba(var(--v-theme-error), 0.06);
  /*noinspection CssUnresolvedCustomProperty*/
  border-inline-start-color: rgba(var(--v-theme-error), 0.55);
}

.editor-list-item-layout--removed .editor-list-item-layout__editable {
  opacity: 0.55;
}

@media (max-width: 959px) {
  .editor-list-item-layout__actions {
    padding-top: 0;
  }
}
</style>
