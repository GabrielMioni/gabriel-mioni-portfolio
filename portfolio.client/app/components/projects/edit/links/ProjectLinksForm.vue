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

const addLink = (item: LinkEditorItem) => {
  const sort = activeLinkItems.value.length + 1
  const nextActiveItems = [...activeLinkItems.value, { ...item, sort }]
  updateActiveLinkItems(nextActiveItems)
}

</script>

<template>
  <v-container
    fluid
    class="pa-0">
    <v-row>
      <v-col xs12>
        <AddProjectLink
          :link-count="activeLinkItems.length"
          @add="addLink" />
      </v-col>
    </v-row>
    <v-row>
      <v-col xs="12">
        <v-form
          v-model="isValid"
          class="project-links-list-form"
          autocomplete="off">
          <ProjectLinkList
            v-model="activeLinkItems"
            @remove="removeLink" />
        </v-form>
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>

</style>
