<script setup lang="ts">
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import { findEditorItemAndIndexByClientId, normalizeEditorItemsSortOrder } from '~/utils/editorItems'

const isValid = ref<boolean>(false)

const activeLinkItems = defineModel<LinkEditorItem[]>('items', { required: true })
const removedLinkItems = defineModel<LinkEditorItem[]>('removed', { required: true })

const updateActiveLinkItems = (items: LinkEditorItem[]) => {
  activeLinkItems.value = normalizeEditorItemsSortOrder(items)
}

const removeLink = (clientId: string) => {
  const result = findEditorItemAndIndexByClientId(clientId, activeLinkItems.value)
  if (!result) return

  const { item, index } = result

  const nextActiveItems = [...activeLinkItems.value]
  nextActiveItems.splice(index, 1)

  removedLinkItems.value.push(item)
  updateActiveLinkItems(nextActiveItems)
}

</script>

<template>
  <v-form
    v-model="isValid"
    class="project-links-form">
    <v-container
      fluid
      class="pa-0">
      <v-row>
        <v-col xs="12">
          <ProjectLinkList
            v-model="activeLinkItems"
            @remove="removeLink" />
        </v-col>
      </v-row>
    </v-container>
  </v-form>
</template>

<style scoped>

</style>
