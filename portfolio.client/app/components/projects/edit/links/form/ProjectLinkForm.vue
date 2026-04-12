<script setup lang="ts">
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import { ProjectLinkType } from '~/generated/graphql'
import { required, validateUrl } from '~/utils/rules'
import { VForm } from 'vuetify/components'

const model = defineModel<LinkEditorItem>({ required: true })
const isValid = defineModel<boolean | null>('isValid', { required: true })

const form = ref<InstanceType<typeof VForm> | null>(null)

const linkType = computed({
  get: () => model.value.type ?? ProjectLinkType.Repository,
  set: v => model.value.type = v
})
const linkUrl = computed({
  get: () => model.value.url ?? '',
  set: v => model.value.url = v
})
const linkText = computed({
  get: () => model.value.text ?? '',
  set: v => model.value.text = v
})

const resetValidation = () => {
  form.value?.resetValidation()
}

defineExpose({
  resetValidation
})
</script>

<template>
  <v-form
    ref="form"
    v-model="isValid">
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
</template>
