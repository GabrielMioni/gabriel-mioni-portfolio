<script setup lang="ts">
import {
  type Project,
  ProjectStatus
} from '~/generated/graphql'
import type { ProjectForm } from '~/types/ui/form'

const { editing, editProject } = useProjectMutations()

const dialog = defineModel<boolean>()

const form = reactive<ProjectForm>({
  id: '',
  title: '',
  summary: '',
  body: '',
  status: ProjectStatus.Draft
})

const props = defineProps<{
  project: Project
}>()


watch(
  () => [dialog.value, props.project] as const,
  ([isOpen, project]) => {
    if (!isOpen) return

    form.id = project.id
    form.title = project.title ?? ''
    form.summary = project.summary ?? ''
    form.body = project.body ?? ''
  },
  { immediate: true }
)

const submit = async () => {
  try {
    await editProject(form)
  } catch (error) {
    console.error('Failed to save project', error)
  } finally {
    editing.value = false
  }
}

</script>

<template>
  <BaseDialog
    v-model="dialog"
    divider
    title="Edit Project"
    :persistent="editing">
    <v-form>
      <v-container
        class="pa-0"
        fluid>
        <v-row no-gutters>
          <v-col>
            <v-text-field
              v-model="form.title"
              variant="filled"
              label="Title" />
          </v-col>
        </v-row>
        <v-row no-gutters>
          <v-col>
            <v-text-field
              v-model="form.summary"
              variant="filled"
              label="Summary" />
          </v-col>
        </v-row>
        <v-row no-gutters>
          <v-textarea
            v-model="form.body"
            max-height="300"
            label="Body"
            auto-grow
            persistent-hint />
        </v-row>
      </v-container>
    </v-form>
    <template #actions>
      <v-spacer />
      <v-btn
        variant="flat"
        @click="dialog = false">
        Cancel
      </v-btn>
      <v-btn
        variant="flat"
        color="primary"
        :loading="editing"
        @click="submit">
        Save
      </v-btn>
    </template>
  </BaseDialog>
</template>

<style scoped>

</style>
