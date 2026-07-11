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
    toolbarColor: 'primary',
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
    <v-toolbar
      v-if="!hideToolbar"
      :color="toolbarColor"
      density="comfortable"
      flat>
      <v-toolbar-title v-if="title">
        {{ title}}
      </v-toolbar-title>
      <v-spacer />
      <v-btn
        icon="mdi-close"
        variant="text"
        @click="dialog = false" />
    </v-toolbar>
    <v-card v-focus-first-input="focusFirstInput">
      <v-card-title v-if="$slots['card-title']">
        <slot name="card-title" />
      </v-card-title>
      <v-card-text v-if="$slots.default">
        <slot name="default" />
      </v-card-text>
      <v-divider v-if="divider" />
      <v-card-actions v-if="$slots.actions">
        <slot name="actions" />
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>

</style>
