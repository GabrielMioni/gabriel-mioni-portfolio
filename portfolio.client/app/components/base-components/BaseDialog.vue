<script setup lang="ts">
const dialog = defineModel<boolean>()

withDefaults(
  defineProps<{
    title?: string
    divider?: boolean
    persistent?: boolean
    toolbarColor?: string,
    width?: string | number
  }>(),
  {
    title: undefined,
    divider: false,
    persistent: false,
    toolbarColor: 'primary',
    width: 600
  }
)

</script>

<template>
  <v-dialog
    v-model="dialog"
    :width="width"
    :persistent="persistent">
    <v-toolbar
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
    <v-card>
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
