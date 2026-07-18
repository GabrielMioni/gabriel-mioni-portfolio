<script setup lang="ts">
import {
  type Project,
  type EditProjectInput,
  ProjectStatus
} from '~/generated/graphql'
import type { ProjectForm } from '~/types/ui/form'

const { editing, editProject } = useProjectMutations()

const dialog = defineModel<boolean>()

const isValid = ref(false)
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
    form.status = project?.status ?? ProjectStatus.Draft
  },
  { immediate: true }
)

const submit = async () => {
  try {
    await editProject(updateInput.value)
  } catch (error) {
    console.error('Failed to save project', error)
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
    <ProjectDetails
      v-model:form="form"
      v-model:is-valid="isValid"
      :show-tags="false" />
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
        :disabled="!isValid || editing"
        :loading="editing"
        @click="submit">
        Save
      </v-btn>
    </template>
  </BaseDialog>
</template>

<style scoped>

</style>
