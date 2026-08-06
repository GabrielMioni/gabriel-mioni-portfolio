<script setup lang="ts">

const { deleteProject, deleting } = useProjectMutations()

const dialog = defineModel<boolean>()
const snackbarStore = useSnackbarStore()

const emit = defineEmits<{
  (e: 'deleted'): void
}>()

const props = withDefaults(
  defineProps<{
      projectId: string,
      title?: string | null,
      summary?: string | null,
    }>(),
  {
    title: null,
    summary: null
  }
)

const showDetails = computed(() => !!(props.title || props.summary))

const deleteProjectAsync = async () => {
  try {
    const projectId = props.projectId
    if (!projectId) {
      return
    }
    await deleteProject({ projectId })

    snackbarStore.showSnackbar('Project deleted successfully', 'success')
    dialog.value = false
    emit('deleted')
  } catch (err) {
    const message = err instanceof Error
      ? err.message
      : 'Failed to delete project.'
    snackbarStore.showSnackbar(message, 'error')
    console.error(err)
  }
}

const summaryDisplay = computed(() => {
  if (!props.summary) return ''
  return shorten(props.summary)
})

const titleDisplay = computed(() => {
  if (!props.title) return ''
  return shorten(props.title)
})

const shorten = (val?: string | null) => {
  if (!val) return ''
  const shortenVal = 50
  const value = val.trim()
  if (value.length > shortenVal) {
    return value.slice(0, shortenVal) + '...'
  }
  return value
}

</script>

<template>
  <BaseDialog
    v-model="dialog"
    :persistent="deleting"
    width="500"
    title="Delete project">
    <BaseDestructiveNotice title="Remove this project?">
      The project record and its associated images will be deleted.
      This cannot be undone.
    </BaseDestructiveNotice>
    <dl
      v-if="showDetails"
      class="admin-record-summary">
      <div v-if="titleDisplay">
        <dt>Project</dt>
        <dd>{{ titleDisplay }}</dd>
      </div>
      <div v-if="summaryDisplay">
        <dt>Summary</dt>
        <dd>{{ summaryDisplay }}</dd>
      </div>
    </dl>
    <template #actions>
      <v-spacer />
      <v-btn
        variant="text"
        :disabled="deleting"
        @click="dialog = false">
        Cancel
      </v-btn>
      <v-btn
        variant="flat"
        color="error"
        :loading="deleting"
        @click="deleteProjectAsync">
        Delete project
      </v-btn>
    </template>
  </BaseDialog>
</template>

<style scoped>

</style>
