<script setup lang="ts">
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import { ProjectLinkType } from '~/generated/graphql'
import { removeEditorItem } from '~/utils/editorItems'

const linkItems = defineModel<LinkEditorItem[]>('items', { required: true })
const isValid = defineModel<boolean>('isValid', { required: true })

const createLink = (): LinkEditorItem => ({
  clientId: crypto.randomUUID(),
  url: '',
  text: '',
  type: ProjectLinkType.Repository,
  sort: linkItems.value.length + 1,
  isRemoved: false
})

const addLink = () => {
  linkItems.value = [
    ...linkItems.value,
    createLink()
  ]
}

const removeLink = (clientId: string) => {
  linkItems.value = removeEditorItem(clientId, linkItems.value)
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
