<script setup lang="ts">
import type { ProjectTagSummary } from '~/generated/graphql'

const dialog = defineModel<boolean>()

const props = defineProps<{
  tag: ProjectTagSummary | null
}>()

const emit = defineEmits<{
  deleted: []
}>()

const { deleteProjectTag, deletingTag } = useProjectTagMutations()
const { showSnackbar } = useSnackbarStore()

const onDelete = async () => {
  if (!props.tag) return
  try {
    await deleteProjectTag(props.tag.id)
    showSnackbar(`"${props.tag.name}" deleted`, 'success')
    dialog.value = false
    emit('deleted')
  } catch (e: unknown) {
    const message = e instanceof Error ? e.message : 'An error occurred'
    showSnackbar(message, 'error')
  }
}
</script>

<template>
  <BaseDialog
    v-model="dialog"
    :persistent="deletingTag"
    title="Delete Tag"
    width="440">
    <div>
      Are you sure you want to delete
      <span class="font-weight-bold">{{ tag?.name }}</span>?
      This will remove the tag from all associated projects.
    </div>
    <template #actions>
      <v-spacer />
      <v-btn
        variant="text"
        :disabled="deletingTag"
        @click="dialog = false">
        Cancel
      </v-btn>
      <v-btn
        color="error"
        variant="flat"
        :loading="deletingTag"
        @click="onDelete">
        Delete
      </v-btn>
    </template>
  </BaseDialog>
</template>
