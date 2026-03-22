<script setup lang="ts">
import draggable from 'vuedraggable'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import ProjectImageUploadListItem from '~/components/projects/edit/images/list/ProjectImageUploadListItem.vue'

const props = defineProps<{
  items: ImageEditorItem[]
}>()

const emit = defineEmits<{
  (e: 'update:items', value: ImageEditorItem[]): void
}>()

const itemsLocal = computed({
  get: () => props.items,
  set: (value: ImageEditorItem[]) => {
    emit('update:items', value)
  }
})

const normalizeSortOrder = (items: ImageEditorItem[]) =>
  items.map((item, index) => ({
    ...item,
    sort: index
  }))

const removeItem = (clientId: string) => {
  emit(
    'update:items',
    normalizeSortOrder(
      props.items.filter(item => item.clientId !== clientId)
    )
  )
}

const syncSortOrder = () => {
  emit('update:items', normalizeSortOrder(props.items))
}
</script>

<template>
  <draggable
    v-model="itemsLocal"
    item-key="clientId"
    handle=".drag-handle"
    @end="syncSortOrder">
    <template #item="{ element, index }">
      <div>
        <ProjectImageUploadListItem
          :item="element"
          @remove="removeItem" />
        <v-divider
          v-if="index !== itemsLocal.length - 1"
          class="my-3" />
      </div>
    </template>
  </draggable>
</template>
