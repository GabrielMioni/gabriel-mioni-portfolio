<script setup lang="ts">
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import { ProjectLinkType } from '~/generated/graphql'
import { ProjectLinkForm } from '#components'

const props = defineProps<{
  linkCount: number
}>()

const emit = defineEmits<{
  (e: 'add', item: LinkEditorItem): void
}>()

const isValid = ref<boolean | null>(false)
const form = ref<InstanceType<typeof ProjectLinkForm> | null>(null)

const linkEditorItem = ref<LinkEditorItem>({
  type: ProjectLinkType.Repository,
  url: '',
  text: '',
  clientId: '',
  sort: -1
})

const reset = async () => {
  linkEditorItem.value = {
    type: ProjectLinkType.Repository,
    url: '',
    text: '',
    clientId: '',
    sort: -1
  }

  await nextTick()
  form.value?.resetValidation()
}

const submit = () => {
  if (!isValid.value) return

  const out = {
    ...linkEditorItem.value,
    clientId: crypto.randomUUID(),
    sort: props.linkCount + 1
  }

  emit('add', out)
  reset()
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
          :link-type="linkEditorItem.type"
          size="x-large" />
      </v-col>
      <v-col>
        <ProjectLinkForm
          ref="form"
          v-model="linkEditorItem"
          v-model:is-valid="isValid" />
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
