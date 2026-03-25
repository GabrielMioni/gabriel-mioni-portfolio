<script setup lang="ts">
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'

const dialog = defineModel<boolean>()

defineProps<{
  removedImageItems: ImageEditorItem[]
}>()

</script>

<template>
  <BaseDialog
    v-model="dialog"
    title="Removed Images">
    Blah
    <template #actions>
      <v-spacer />
      <v-btn
        variant="flat"
        @click="dialog = false">
        Close
      </v-btn>
    </template>
  </BaseDialog>
</template>

<style scoped>

</style>
