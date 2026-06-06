<script setup lang="ts">
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'

const config = useRuntimeConfig()

const item = defineModel<ImageEditorItem>('item', { required: true })

withDefaults(
  defineProps<{
      isRemoving?: boolean
    }>(),
  {
    isRemoving: true
  }
)

defineEmits<{
  (e: 'update', clientId: string): void
}>()

const imageUrl = computed(() => {
  const current = item.value

  if (!current.id) {
    if (!current.thumbFile) return null
    return URL.createObjectURL(current.thumbFile)
  }

  const base = config.public.storageBase
  const key = current.thumbKey

  if (!base || !key) return null

  return `${base}/${key}`
})

const imageFileName = computed(() => {
  return item.value?.fileName ?? 'Unnamed Image'
})

const createdAtDate = computed(() => {
  if (!item.value?.createdAt) return null
  return new Date(item.value.createdAt).toLocaleDateString()
})
</script>

<template>
  <EditorItemLayout
    :draggable="isRemoving"
    :action-icon="isRemoving ? 'mdi-close' : 'mdi-plus'"
    :action-color="isRemoving ? 'error' : 'success'"
    @action="$emit('update', item.clientId)">
    <template #leading>
      <v-img
        v-if="imageUrl"
        class="ma-2"
        :src="imageUrl"
        width="50" />
    </template>

    <v-row align="center">
      <v-col
        class="order-2"
        sm="2">
        <div class="image-details text-break fs-12">
          <div>
            <template v-if="createdAtDate">
              {{ createdAtDate }}
            </template>
            <span
              v-else
              class="font-italic text-grey">
              {{ imageFileName }} (pending)
            </span>
          </div>
          <div>{{ item.contentType }}</div>
          <div>{{ (item.sizeFull / 1024).toFixed(2) }} KB</div>
        </div>
      </v-col>

      <v-col
        cols="12"
        sm>
        <v-text-field
          v-model="item.altText"
          :disabled="!isRemoving"
          variant="filled"
          label="Alt Text"
          hide-details />
      </v-col>
    </v-row>
  </EditorItemLayout>
</template>
