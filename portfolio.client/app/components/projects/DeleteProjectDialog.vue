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
    snackbarStore.showSnackbar('Project deleted failed', 'error')
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
    title="Delete Project">
    <div :class="{ 'mb-3': showDetails }">
      Are you sure you want to delete this project?
    </div>
    <v-container
      v-if="showDetails"
      class="pa-0">
      <v-row no-gutters>
        <v-col
          cols="3"
          class="font-weight-bold">
          Title:
        </v-col>
        <v-col>
          <div
            class="font-italic"
            v-text="titleDisplay"/>
        </v-col>
      </v-row>
      <v-row no-gutters>
        <v-col
          cols="3"
          class="font-weight-bold">
          Summary:
        </v-col>
        <v-col>
          <div
            class="font-italic"
            v-text="summaryDisplay"/>
        </v-col>
      </v-row>
    </v-container>
    <template #actions>
      <v-btn
        variant="text"
        @click="dialog = false">
        Cancel
      </v-btn>
      <v-btn
        variant="flat"
        color="error"
        @click="deleteProjectAsync">
        Delete
      </v-btn>
    </template>
  </BaseDialog>
</template>

<style scoped>

</style>
