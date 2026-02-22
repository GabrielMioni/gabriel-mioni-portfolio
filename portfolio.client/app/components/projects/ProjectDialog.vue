<script setup lang="ts">
import {
  type Project,
  type CreateProjectInput,
  type EditProjectInput,
  ProjectStatus
} from '~/generated/graphql'
import type { ProjectForm } from '~/types/ui/form'

const {
  creating,
  createProject,
  editing,
  editProject
} = useProjectMutations()

const dialog = defineModel<boolean>()

const form = reactive<ProjectForm>({
  id: '',
  title: '',
  summary: '',
  body: '',
  status: ProjectStatus.Draft
})

const props = defineProps<{
  project?: Project | null
}>()

const createInput = computed<CreateProjectInput>(() => ({
  title: form.title,
  summary: form.summary,
  body: form.body,
  status: form.status
}))

const updateInput = computed<EditProjectInput>(() => ({
  id: form.id,
  title: form.title,
  summary: form.summary,
  body: form.body,
  status: form.status
}))


watch(
  () => dialog.value,
  (isOpen) => {
    if (!isOpen) return

    const project = props.project

    form.id = project?.id ?? ''
    form.title = project?.title ?? ''
    form.summary = project?.summary ?? ''
    form.body = project?.body ?? ''
  },
  { immediate: true }
)


const submitCreateProject = async () => {
  try {
    await createProject(createInput.value)
  } catch (error) {
    console.error('Failed to create project', error)
  }
}

const submitEditProject = async () => {
  try {
    await editProject(updateInput.value)
  } catch (error) {
    console.error('Failed to save project', error)
  }
}

const submit = async () => {
  if (props.project) {
    await submitEditProject()
  } else {
    await submitCreateProject()
  }
  dialog.value = false
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
        :loading="creating || editing"
        @click="submit">
        Save
      </v-btn>
    </template>
  </BaseDialog>
</template>

<style scoped>

</style>
