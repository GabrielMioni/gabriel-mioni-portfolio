<script setup lang="ts">
import { ProjectLinkType } from '~/generated/graphql'
import { required, validateUrl } from '~/utils/rules'
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import { normalizeUrl } from '~/utils/links'

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
    url: normalizeUrl(linkUrl.value),
    text: linkText.value,
    sort: 0
  }
  emit('add', newLink)
}

const { mdAndUp, smAndDown } = useDisplay()

</script>

<template>
  <v-container
    fluid
    class="pa-0">
    <v-row class="align-start">
      <v-col
        cols="auto"
        class="pt-6">
        <LinkTypeIcon
          :link-type="linkType"
          size="x-large" />
      </v-col>
      <v-col>
        <v-form
          v-model="isValid"
          @keyup.enter="submit">
          <v-row dense>
            <v-col
              cols="12"
              md="4">
              <v-text-field
                v-model="linkUrl"
                label="Url"
                variant="filled"
                :rules="[required(), validateUrl()]" />
            </v-col>
            <v-col
              cols="12"
              md="4">
              <v-text-field
                v-model="linkText"
                label="Link Text"
                variant="filled"
                :rules="[required()]" />
            </v-col>
            <v-col
              cols="12"
              md="4">
              <ProjectLinkSelect v-model="linkType" />
            </v-col>
          </v-row>
        </v-form>
        <v-row v-if="smAndDown">
          <v-col>
            <v-btn
              :disabled="!isValid"
              block
              class="bg-primary"
              @click="submit">
              Add Link
            </v-btn>
          </v-col>
        </v-row>
      </v-col>
      <v-col
        v-if="mdAndUp"
        cols="auto"
        class="pt-6">
        <v-btn
          :disabled="!isValid"
          density="comfortable"
          icon="mdi-plus"
          @click="submit" />
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>

</style>
