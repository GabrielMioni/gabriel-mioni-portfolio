<script setup lang="ts">
import { ProjectLinkType } from '~/generated/graphql'
import { required, validateUrl } from '~/utils/rules'
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'

const emit = defineEmits<{
  (e: 'add', item: LinkEditorItem): void
}>()

const isValid = ref<boolean>(false)

const linkType = ref<ProjectLinkType>(ProjectLinkType.Repository)
const linkUrl = ref<string>('')
const linkText = ref<string>('')

const submit = () => {
  if (!isValid.value) return

  const newLink: LinkEditorItem = {
    clientId: crypto.randomUUID(),
    type: linkType.value,
    url: linkUrl.value,
    text: linkText.value,
    sort: 0
  }
  emit('add', newLink)
}

</script>

<template>
  <v-form
    v-model="isValid"
    @keyup.enter="submit">
    <v-container
      fluid
      class="pa-0">
      <v-row>
        <v-col
          cols="auto"
          class="d-flex justify-center align-center">
          <LinkTypeIcon
            :link-type="linkType"
            size="x-large" />
        </v-col>
        <v-col class="d-flex justify-start align-center">
          <v-text-field
            v-model="linkUrl"
            label="Url"
            variant="filled"
            :rules="[required(), validateUrl()]"/>
        </v-col>
        <v-col>
          <v-text-field
            v-model="linkText"
            label="Link Text"
            variant="filled"
            :rules="[required()]"
            hide-details />
        </v-col>
        <v-col>
          <ProjectLinkSelect
            v-model="linkType"/>
        </v-col>
        <v-col
          cols="auto"
          class="d-flex justify-center align-center">
          <v-btn
            :disabled="!isValid"
            density="comfortable"
            icon="mdi-plus"
            @click="submit()" />
        </v-col>
      </v-row>
    </v-container>
  </v-form>
</template>

<style scoped>

</style>
