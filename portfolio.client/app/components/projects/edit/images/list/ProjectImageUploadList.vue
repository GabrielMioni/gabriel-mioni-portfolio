<script setup lang="ts">
import draggable from 'vuedraggable'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import ProjectImageUploadListItem from '~/components/projects/edit/images/list/ProjectImageUploadListItem.vue'

const items = defineModel<ImageEditorItem[]>({ required: true })

const removeItem = (clientId: string) => {
  const index = items.value.findIndex(item => item.clientId === clientId)
  if (index !== -1) {
    items.value.splice(index, 1)
    syncSortOrder()
  }
}

const syncSortOrder = () => {
  items.value = items.value.map((item, index) => ({
    ...item,
    sort: index
  }))
}

</script>

<template>
  <draggable
    v-model="items"
    item-key="clientId"
    handle=".drag-handle"
    @end="syncSortOrder">
    <template #item="{ element, index }">
      <div>
        <ProjectImageUploadListItem
          :item="element"
          @remove="removeItem" />
        <v-divider
          v-if="index !== items.length - 1"
          class="my-3" />
      </div>
    </template>
  </draggable>
</template>

<style scoped>

</style>
