<script setup lang="ts">
import type { ProjectTagSummary } from '~/generated/graphql'

const dialog = defineModel<boolean>()

const props = defineProps<{
  tag: ProjectTagSummary | null
}>()

const emit = defineEmits<{
  deleted: []
}>()

const { deleteTag, deletingTag } = useTagMutations()
const { showSnackbar } = useSnackbarStore()

const onDelete = async () => {
  if (!props.tag) return
  try {
    await deleteTag(props.tag.id)
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
    title="Delete tag"
    width="440">
    <BaseDestructiveNotice title="Remove this tag?">
      The tag will be removed from every assigned project. The projects
      themselves will remain unchanged.
    </BaseDestructiveNotice>
    <dl
      v-if="tag"
      class="admin-record-summary">
      <div>
        <dt>Tag</dt>
        <dd>{{ tag.name }}</dd>
      </div>
      <div>
        <dt>Assignments</dt>
        <dd>{{ tag.projectsCount }}</dd>
      </div>
    </dl>
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
        Delete tag
      </v-btn>
    </template>
  </BaseDialog>
</template>
