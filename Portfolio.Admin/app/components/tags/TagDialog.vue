<script setup lang="ts">
import { useQuery } from '@urql/vue'
import type { ProjectTagSummary } from '~/generated/graphql'
import { GetProjectsByTagIdDocument } from '~/generated/graphql'

const dialog = defineModel<boolean>()

const props = defineProps<{
  tag: ProjectTagSummary | null
}>()

const { renameProjectTag, removeTagFromProjects, renamingTag, removingFromProjects } = useProjectTagMutations()
const { showSnackbar } = useSnackbarStore()

const emit = defineEmits<{ save: [] }>()

const { data, fetching: fetchingProjects, executeQuery } = useQuery({
  query: GetProjectsByTagIdDocument,
  variables: computed(() => ({ tagId: props.tag?.id ?? '' })),
  pause: true
})

const projects = computed(() => data.value?.projectsByTagId ?? [])

const editedName = ref('')
const pendingRemovals = ref<Set<string>>(new Set())

const isDirty = computed(() =>
  editedName.value.trim() !== (props.tag?.name ?? '') || pendingRemovals.value.size > 0
)

const saving = computed(() => renamingTag.value || removingFromProjects.value)

watch(dialog, (open) => {
  if (open && props.tag) {
    editedName.value = props.tag.name
    pendingRemovals.value = new Set()
    executeQuery()
  }
})

const toggleRemoval = (projectId: string) => {
  const next = new Set(pendingRemovals.value)
  if (next.has(projectId)) next.delete(projectId)
  else next.add(projectId)
  pendingRemovals.value = next
}

const save = async () => {
  if (!props.tag) return

  try {
    if (editedName.value.trim() !== props.tag.name) {
      await renameProjectTag(props.tag.id, editedName.value.trim())
    }

    if (pendingRemovals.value.size > 0) {
      await removeTagFromProjects(props.tag.id, [...pendingRemovals.value])
    }

    showSnackbar('Tag saved', 'success')
    emit('save')
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
    :persistent="saving"
    :title="tag?.name ?? ''"
    width="560">
    <v-text-field
      v-model="editedName"
      label="Name"
      variant="filled"
      hide-details
      class="mb-6" />
    <v-divider class="mb-4" />
    <div class="text-subtitle-2 mb-2">Projects</div>
    <div
      v-if="fetchingProjects"
      class="d-flex justify-center py-4">
      <v-progress-circular indeterminate />
    </div>
    <v-list
      v-else
      density="compact">
      <v-list-item
        v-for="project in projects"
        :key="project.id"
        :to="pendingRemovals.has(project.id) ? undefined : `/projects/${project.id}`"
        :prepend-icon="pendingRemovals.has(project.id) ? undefined : 'mdi-open-in-app'"
        :title="project.title">
        <template #title>
          <div
            :class="{ 'text-decoration-line-through text-disabled': pendingRemovals.has(project.id) }"
            v-text="project.title" />
        </template>
        <template #append>
          <v-btn
            :icon="pendingRemovals.has(project.id) ? 'mdi-undo' : 'mdi-close'"
            :color="pendingRemovals.has(project.id) ? 'warning' : undefined"
            size="x-small"
            variant="text"
            @click.prevent="toggleRemoval(project.id)" />
        </template>
      </v-list-item>
      <v-list-item
        v-if="projects.length === 0"
        class="text-grey">
        No projects
      </v-list-item>
    </v-list>
    <template #actions>
      <v-spacer />
      <v-btn
        variant="text"
        :disabled="saving"
        @click="dialog = false">
        Cancel
      </v-btn>
      <v-btn
        color="primary"
        variant="flat"
        :disabled="!isDirty || saving"
        :loading="saving"
        @click="save">
        Save
      </v-btn>
    </template>
  </BaseDialog>
</template>
