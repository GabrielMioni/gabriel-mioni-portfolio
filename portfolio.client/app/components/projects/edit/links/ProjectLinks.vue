<script setup lang="ts">
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import { findEditorItemAndIndexByClientId, normalizeEditorItemsSortOrder } from '~/utils/editorItems'
import { ProjectLinkType } from '~/generated/graphql'

const activeLinkItems = defineModel<LinkEditorItem[]>('items', { required: true })
const isValid = defineModel<boolean | null>('isValid', { required: true })
const removedLinkItems = defineModel<LinkEditorItem[]>('removed', { required: true })

const updateActiveLinkItems = (items: LinkEditorItem[]) => {
  activeLinkItems.value = normalizeEditorItemsSortOrder(items)
}

const createLink = (): LinkEditorItem => ({
  clientId: crypto.randomUUID(),
  url: '',
  text: '',
  type: ProjectLinkType.Repository,
  sort: activeLinkItems.value.length + 1
})

const addLink = () => {
  activeLinkItems.value = [
    ...activeLinkItems.value,
    createLink()
  ]
}

const removeLink = (clientId: string) => {
  const result = findEditorItemAndIndexByClientId(clientId, activeLinkItems.value)
  if (!result) return

  const { item, index } = result

  const isEmpty = !item.url && !item.text

  const nextActiveItems = [...activeLinkItems.value]
  nextActiveItems.splice(index, 1)

  if (isEmpty) {
    updateActiveLinkItems(nextActiveItems)
    return
  }

  removedLinkItems.value.push(item)
  updateActiveLinkItems(nextActiveItems)
}

</script>

<template>
  <v-container
    fluid
    class="pa-0">
    <v-divider class="my-6" />
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
    <v-row>
      <v-col>
        <v-btn
          variant="text"
          prepend-icon="mdi-plus"
          @click="addLink">
          Add link
        </v-btn>
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>

</style>
