<script setup lang="ts">
withDefaults(
  defineProps<{
      draggable?: boolean
      actionIcon?: string
      actionColor?: string
      compact?: boolean
    }>(),
  {
    draggable: false,
    actionIcon: 'mdi-close',
    actionColor: 'error',
    compact: false
  }
)

defineEmits<{
  (e: 'action'): void
}>()
</script>

<template>
  <v-container
    fluid
    class="py-0 editor-list-item-layout hover-surface">
    <v-row
      :class="{ 'editor-list-item-layout--compact': compact }">
      <v-col
        v-if="draggable"
        cols="auto"
        class="d-flex align-center justify-center order-1">
        <div class="d-flex align-center">
          <v-icon
            class="drag-handle cursor-grab"
            icon="mdi-drag" />
          <slot name="leading" />
        </div>
      </v-col>
      <v-col class="order-2 order-sm-3">
        <slot />
      </v-col>
      <v-col
        cols="auto"
        class="d-flex align-center justify-end order-3 order-sm-4">
        <slot name="actions">
          <v-btn
            :icon="actionIcon"
            class="ma-2"
            variant="text"
            :color="actionColor"
            @click="$emit('action')" />
        </slot>
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>
.editor-list-item-layout--compact {
  min-height: 72px;
}
</style>
