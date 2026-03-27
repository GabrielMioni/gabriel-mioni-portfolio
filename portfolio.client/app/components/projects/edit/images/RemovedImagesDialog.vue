<script setup lang="ts">
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'

const dialog = defineModel<boolean>()

const props = defineProps<{
  removedImageItems: ImageEditorItem[]
}>()

defineEmits<{
  (e: 'add', clientId: string): void
}>()

const removedImagesLength = computed(() => props.removedImageItems.length)

watch(
  removedImagesLength,
  (val) => {
    if (val <= 0) {
      dialog.value = false
    }
  }
)

</script>

<template>
  <BaseDialog
    v-model="dialog"
    hide-toolbar>
    <template #card-title>
      <div class="d-flex align-center">
        <span class="fs-14 font-weight-bold">
          Removed Images
        </span>
        <v-spacer />
        <v-btn
          icon="mdi-close"
          flat
          small
          density="compact"
          @click="dialog = false" />
      </div>
    </template>
    <div>
      <ProjectImageUploadListItem
        v-for="item in removedImageItems"
        :key="item.clientId"
        :item="item"
        :is-removing="false"
        @update="$emit('add', $event)"/>
    </div>
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
