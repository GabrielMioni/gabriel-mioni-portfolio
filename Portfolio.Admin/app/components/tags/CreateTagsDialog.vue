<script setup lang="ts">
import type { TagEditorItem } from '~/types/tags'

const dialog = defineModel<boolean>()

const emit = defineEmits<{ created: [] }>()

const { createTags, creatingTags } = useTagMutations()
const { showSnackbar } = useSnackbarStore()

const stagedTags = ref<TagEditorItem[]>([])

const pendingTags = computed(() => stagedTags.value.filter(t => t.id === null))

const createButtonLabel = computed(() => {
  const count = pendingTags.value.length
  if (count === 0) return 'Create tags'
  if (count === 1) return 'Create tag'
  return `Create ${count} tags`
})

watch(dialog, (open) => {
  if (open) stagedTags.value = []
})

const save = async () => {
  if (creatingTags.value || !pendingTags.value.length) return
  try {
    await createTags(pendingTags.value)
    const count = pendingTags.value.length
    showSnackbar(`${count} ${count === 1 ? 'tag' : 'tags'} created`, 'success')
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
    :persistent="creatingTags"
    title="Create tags"
    width="500"
    focus-first-input>
    <div class="create-tags-dialog__intro">
      <v-icon
        color="rust"
        icon="mdi-tag-plus-outline"
        size="26" />
      <div>
        <p class="create-tags-dialog__label">Tag catalog</p>
        <p>
          Enter one or more names. Press Enter to stage each tag before saving.
        </p>
      </div>
    </div>
    <TagsCombobox
      v-model:assigned-tags="stagedTags"
      disable-existing
      @submit="save" />
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
        {{ createButtonLabel }}
      </v-btn>
    </template>
  </BaseDialog>
</template>

<style scoped>
.create-tags-dialog__intro {
  align-items: start;
  background: color-mix(in srgb, rgb(var(--v-theme-cyan)) 10%, transparent);
  border: 1px solid rgb(var(--v-theme-rule));
  display: grid;
  gap: 0.85rem;
  grid-template-columns: auto minmax(0, 1fr);
  margin-bottom: 1rem;
  padding: 1rem;
}

.create-tags-dialog__intro p:last-child {
  color: rgb(var(--v-theme-muted));
  line-height: 1.5;
}

.create-tags-dialog__label {
  color: rgb(var(--v-theme-rust));
  font-family: var(--admin-font-mono);
  font-size: 0.68rem;
  font-weight: 700;
  letter-spacing: 0.09em;
  margin-bottom: 0.25rem;
  text-transform: uppercase;
}
</style>
