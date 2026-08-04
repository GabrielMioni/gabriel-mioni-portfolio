<script setup lang="ts">
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import { required, validateUrl } from '~/utils/rules'

const item = defineModel<LinkEditorItem>('item', { required: true })

const emit = defineEmits<{
  (e: 'remove' | 'restore', clientId: string): void
}>()

const updateRemovalState = () => {
  const event = item.value.isRemoved ? 'restore' : 'remove'
  emit(event, item.value.clientId)
}

const createdAtDate = computed(() => {
  if (!item.value?.createdAt) return null
  return new Date(item.value.createdAt).toLocaleDateString()
})

</script>

<template>
  <EditorItemLayout
    :draggable="!item.isRemoved"
    :is-pending="!item.id"
    :is-removed="item.isRemoved"
    @action="updateRemovalState">
    <v-row dense>
      <v-col
        cols="12"
        md="3">
        <v-text-field
          v-model="item.url"
          label="Url"
          variant="filled"
          hide-details
          :disabled="item.isRemoved"
          :rules="[required(), validateUrl()]" />
      </v-col>
      <v-col
        cols="12"
        md="3">
        <v-text-field
          v-model="item.text"
          label="Link Text"
          variant="filled"
          hide-details
          :disabled="item.isRemoved"
          :rules="[required()]" />
      </v-col>
      <v-col
        cols="12"
        md="4">
        <ProjectLinkSelect
          v-model="item.type"
          :disabled="item.isRemoved" />
      </v-col>
      <v-col
        cols="12"
        md="1"
        class="d-flex align-center">
        <div class="link-details text-break fs-12">
          Created: <br>
          <template v-if="createdAtDate">
            {{ createdAtDate }}
          </template>
          <span
            v-else
            class="font-italic text-grey">
            (pending)
          </span>
        </div>
      </v-col>
    </v-row>
  </EditorItemLayout>
</template>
