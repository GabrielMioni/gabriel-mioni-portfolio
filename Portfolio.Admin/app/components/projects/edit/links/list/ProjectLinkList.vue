<script setup lang="ts">
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'

const model = defineModel<LinkEditorItem[]>({ required: true })

defineEmits<{
  (e: 'remove' | 'restore', clientId: string): void
}>()

const itemsLocal = computed({
  get: () => model.value,
  set: (value: LinkEditorItem[]) => {
    model.value = normalizeEditorItemsSortOrder(value)
  }
})
</script>

<template>
  <DraggableList v-model="itemsLocal">
    <template #default="{ element }">
      <ProjectLinkListItem
        :item="element"
        @remove="$emit('remove', $event)"
        @restore="$emit('restore', $event)" />
    </template>
  </DraggableList>
</template>

<style scoped>

</style>
