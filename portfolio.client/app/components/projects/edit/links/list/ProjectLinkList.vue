<script setup lang="ts">
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'

const model = defineModel<LinkEditorItem[]>({ required: true })

defineEmits<{
  (e: 'remove', clientId: string): void
}>()

const itemsLocal = computed({
  get: () => model.value.filter(item => !item.isRemoved),
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
        is-removing
        @update="$emit('remove', $event)" />
    </template>
  </DraggableList>
</template>

<style scoped>

</style>
