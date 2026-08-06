<script setup lang="ts">
import type { Directive } from 'vue'

const dialog = defineModel<boolean>()

const vFocusFirstInput: Directive<HTMLElement, boolean> = {
  mounted(el, binding) {
    if (!binding.value) return
    requestAnimationFrame(() => {
      el.querySelector<HTMLElement>('input:not([type="hidden"]), textarea')?.focus()
    })
  }
}

withDefaults(
  defineProps<{
    title?: string
    divider?: boolean
    fullscreen?: boolean
    persistent?: boolean
    toolbarColor?: string
    hideToolbar?: boolean
    width?: string | number
    focusFirstInput?: boolean
  }>(),
  {
    title: undefined,
    divider: false,
    fullscreen: false,
    persistent: false,
    hideToolbar: false,
    toolbarColor: 'paper-raised',
    width: 600,
    focusFirstInput: false
  }
)

</script>

<template>
  <v-dialog
    v-model="dialog"
    :width="width"
    :persistent="persistent"
    :fullscreen="fullscreen">
    <v-card
      v-focus-first-input="focusFirstInput"
      class="admin-dialog">
      <v-toolbar
        v-if="!hideToolbar"
        :color="toolbarColor"
        class="admin-dialog__header"
        density="comfortable"
        flat>
        <v-toolbar-title
          v-if="title"
          class="admin-dialog__title">
          {{ title }}
        </v-toolbar-title>
        <v-spacer />
        <v-btn
          aria-label="Close dialog"
          class="admin-dialog__close"
          :disabled="persistent"
          icon="mdi-close"
          variant="text"
          @click="dialog = false" />
      </v-toolbar>
      <v-card-title v-if="$slots['card-title']">
        <slot name="card-title" />
      </v-card-title>
      <v-card-text
        v-if="$slots.default"
        class="admin-dialog__body">
        <slot name="default" />
      </v-card-text>
      <v-divider v-if="divider" />
      <v-card-actions
        v-if="$slots.actions"
        class="admin-dialog__actions">
        <slot name="actions" />
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.admin-dialog {
  background: rgb(var(--v-theme-paper-raised));
  border: 1px solid rgb(var(--v-theme-rule));
  border-radius: 0;
  box-shadow: 12px 12px 0 rgba(var(--v-theme-ink), 0.18);
}

.admin-dialog__header {
  border-bottom: 1px solid rgb(var(--v-theme-rule));
  border-top: 5px solid rgb(var(--v-theme-cyan));
}

.admin-dialog__title {
  color: rgb(var(--v-theme-ink));
  font-family: var(--admin-font-display);
  font-size: 1.65rem;
  font-weight: 850;
  letter-spacing: -0.02em;
}

.admin-dialog__close {
  color: rgb(var(--v-theme-ink));
}

.admin-dialog__close:hover {
  background: rgb(var(--v-theme-amber));
  color: rgb(var(--v-theme-ink));
}

.admin-dialog__body {
  color: rgb(var(--v-theme-ink));
  padding: 1.5rem;
}

.admin-dialog__actions {
  border-top: 1px solid rgb(var(--v-theme-rule));
  gap: 0.5rem;
  min-height: 4rem;
  padding: 0.75rem 1rem;
}

</style>
