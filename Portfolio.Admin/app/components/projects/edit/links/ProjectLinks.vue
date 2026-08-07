<script setup lang="ts">
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import { ProjectLinkType } from '~/generated/graphql'
import { removeEditorItem, restoreEditorItem } from '~/utils/editorItems'

const linkItems = defineModel<LinkEditorItem[]>('items', { required: true })
const isValid = defineModel<boolean>('isValid', { required: true })
const focusClientId = ref<string | null>(null)

const createLink = (): LinkEditorItem => ({
  clientId: crypto.randomUUID(),
  url: '',
  text: '',
  type: ProjectLinkType.Repository,
  sort: linkItems.value.length + 1,
  isRemoved: false
})

const addLink = async () => {
  const link = createLink()

  linkItems.value = [
    ...linkItems.value,
    link
  ]

  await nextTick()
  focusClientId.value = link.clientId
}

const removeLink = (clientId: string) => {
  linkItems.value = removeEditorItem(clientId, linkItems.value)
}

const restoreLink = (clientId: string) => {
  linkItems.value = restoreEditorItem(clientId, linkItems.value)
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
            v-model="linkItems"
            :focus-client-id="focusClientId"
            @remove="removeLink"
            @restore="restoreLink" />
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
