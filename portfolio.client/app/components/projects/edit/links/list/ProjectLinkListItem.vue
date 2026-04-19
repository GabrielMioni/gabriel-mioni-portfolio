<script setup lang="ts">
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import { required, validateUrl } from '~/utils/rules'

const item = defineModel<LinkEditorItem>('item', { required: true })

withDefaults(
  defineProps<{
    disableAction?: boolean
    isRemoving?: boolean
    }>(),
  {
    disableAction: true,
    isRemoving: false
  }
)

defineEmits<{
  (e: 'update', clientId: string): void
}>()

</script>

<template>
  <EditorItemLayout
    :draggable="isRemoving"
    :action-icon="isRemoving ? 'mdi-close' : 'mdi-plus'"
    :action-color="isRemoving ? 'error' : 'success'"
    @action="$emit('update', item.clientId)">
    <v-row dense>
      <v-col
        cols="12"
        md="4">
        <v-text-field
          v-model="item.url"
          label="Url"
          variant="filled"
          hide-details
          :rules="[required(), validateUrl()]" />
      </v-col>
      <v-col
        cols="12"
        md="4">
        <v-text-field
          v-model="item.text"
          label="Link Text"
          variant="filled"
          hide-details
          :rules="[required()]" />
      </v-col>
      <v-col
        cols="12"
        md="4">
        <ProjectLinkSelect v-model="item.type" />
      </v-col>
    </v-row>
  </EditorItemLayout>
</template>
