<script setup lang="ts">
import type { TagEditorItem } from '~/types/tags'

const dialog = defineModel<boolean>()

const emit = defineEmits<{ created: [] }>()

const { createProjectTags, creatingTags } = useProjectTagMutations()
const { showSnackbar } = useSnackbarStore()

const stagedTags = ref<TagEditorItem[]>([])

const pendingTags = computed(() => stagedTags.value.filter(t => t.id === null))

watch(dialog, (open) => {
  if (open) stagedTags.value = []
})

const save = async () => {
  if (!pendingTags.value.length) return
  try {
    await createProjectTags(pendingTags.value)
    showSnackbar(`${pendingTags.value.length} tag(s) created`, 'success')
    emit('created')
    dialog.value = false
  } catch (e: unknown) {
    const message = e instanceof Error ? e.message : 'An error occurred'
    showSnackbar(message, 'error')
  }
}
</script>

<template>
  <BaseDialog
    v-model="dialog"
    title="Create Tags"
    width="500"
    focus-first-input>
    <TagsCombobox
      v-model:assigned-tags="stagedTags"
      disable-existing />
    <template #actions>
      <v-spacer />
      <v-btn
        variant="text"
        :disabled="creatingTags"
        @click="dialog = false">
        Cancel
      </v-btn>
      <v-btn
        color="primary"
        variant="flat"
        :disabled="!pendingTags.length || creatingTags"
        :loading="creatingTags"
        @click="save">
        Create
      </v-btn>
    </template>
  </BaseDialog>
</template>
