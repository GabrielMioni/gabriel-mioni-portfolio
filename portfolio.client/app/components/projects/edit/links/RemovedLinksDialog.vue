<script setup lang="ts">
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'

defineProps<{
  removedLinkItems: LinkEditorItem[]
}>()

defineEmits<{
  (e: 'add', clientId: string): void
}>()

const dialog = defineModel<boolean>()
</script>

<template>
  <RemovedItemsDialog
    v-model="dialog"
    title="Removed Links"
    :items="removedLinkItems"
    @add="$emit('add', $event)">
    <template #item="{ item, restore }">
      <ProjectLinkListItem
        :item="item"
        :is-removing="false"
        @update="restore" />
    </template>
  </RemovedItemsDialog>
</template>
