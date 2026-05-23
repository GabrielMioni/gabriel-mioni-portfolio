<script setup lang="ts">
import type { Project } from '~/generated/graphql'

const { deleteProject, deleting } = useProjectMutations()

const dialog = defineModel<boolean>()
const snackbarStore = useSnackbarStore()

const emit = defineEmits<{
  (e: 'deleted'): void
}>()

const props = defineProps<{
  project?: Project | null
}>()

const deleteProjectAsync = async () => {
  try {
    const projectId = props.project?.id
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

</script>

<template>
  <BaseDialog
    v-model="dialog"
    :persistent="deleting"
    width="500"
    title="Delete Project">
    <div class="mb-3">
      Are you sure you want to delete this project?
    </div>
    <v-container class="pa-0">
      <v-row no-gutters>
        <v-col
          cols="3"
          class="font-weight-bold">
          Title:
        </v-col>
        <v-col>
          <div
            class="font-italic"
            v-text="(props.project?.title ?? '').trim()"/>
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
            v-text="(props.project?.summary ?? '').trim()"/>
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
