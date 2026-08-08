<script setup lang="ts">
import { useQuery } from '@urql/vue'
import type { ProjectTagSummary } from '~/generated/graphql'
import { GetProjectsByTagIdDocument } from '~/generated/graphql'
import { MAX_TAG_NAME_LENGTH } from '~/utils/tags/limits'
import { maxLength, required } from '~/utils/rules'

const dialog = defineModel<boolean>()

const props = defineProps<{
  tag: ProjectTagSummary | null
}>()

const {
  renameTag,
  removeTagFromProjects,
  renamingTag,
  removingFromProjects
} = useTagMutations()
const { showSnackbar } = useSnackbarStore()

const emit = defineEmits<{ save: [], deleted: [] }>()

const confirmingDelete = ref(false)

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

watch(
  [dialog, () => props.tag] as const,
  ([open, tag]) => {
    if (open && tag) {
      editedName.value = tag.name
      pendingRemovals.value = new Set()
      confirmingDelete.value = false
      executeQuery()
    }
  },
  { immediate: true }
)

const toggleRemoval = (projectId: string) => {
  const next = new Set(pendingRemovals.value)
  if (next.has(projectId)) next.delete(projectId)
  else next.add(projectId)
  pendingRemovals.value = next
}

const onDeleted = () => {
  dialog.value = false
  emit('deleted')
}

const save = async () => {
  if (!props.tag) return

  try {
    if (editedName.value.trim() !== props.tag.name) {
      await renameTag(props.tag.id, editedName.value.trim())
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
    title="Edit tag"
    width="560"
    focus-first-input>
    <p class="tag-dialog__record-label">Tag record</p>
    <p class="tag-dialog__record-name">{{ tag?.name }}</p>
    <v-text-field
      v-model="editedName"
      label="Name"
      variant="filled"
      :maxlength="MAX_TAG_NAME_LENGTH"
      :counter="MAX_TAG_NAME_LENGTH"
      persistent-counter
      :rules="[
        required(),
        maxLength(MAX_TAG_NAME_LENGTH, 'Tag name')
      ]"
      class="mb-6" />
    <section class="tag-dialog__projects">
      <div class="tag-dialog__section-heading">
        <div>
          <p class="tag-dialog__section-label">Assignments</p>
          <h3>Projects</h3>
        </div>
        <span class="tag-dialog__count">{{ projects.length }}</span>
      </div>
      <div
        v-if="fetchingProjects"
        class="d-flex justify-center py-8">
        <v-progress-circular
          color="primary"
          indeterminate />
      </div>
      <v-list
        v-else-if="projects.length"
        class="tag-dialog__project-list"
        density="compact">
        <v-list-item
          v-for="project in projects"
          :key="project.id"
          :class="{
            'tag-dialog__project--removing': pendingRemovals.has(project.id)
          }"
          :to="pendingRemovals.has(project.id) ? undefined : `/projects/${project.id}`"
          :prepend-icon="pendingRemovals.has(project.id) ? 'mdi-link-off' : 'mdi-open-in-app'"
          :title="project.title">
          <template #title>
            <div
              :class="{
                'text-decoration-line-through': pendingRemovals.has(project.id)
              }"
              v-text="project.title" />
          </template>
          <template #append>
            <v-btn
              :aria-label="pendingRemovals.has(project.id)
                ? `Restore ${project.title}`
                : `Remove ${project.title}`"
              :icon="pendingRemovals.has(project.id) ? 'mdi-undo' : 'mdi-close'"
              :color="pendingRemovals.has(project.id) ? 'warning' : undefined"
              size="small"
              variant="text"
              @click.stop.prevent="toggleRemoval(project.id)" />
          </template>
        </v-list-item>
      </v-list>
      <div
        v-else
        class="tag-dialog__empty">
        <v-icon
          icon="mdi-folder-outline"
          size="24" />
        <span>This tag is not assigned to any projects.</span>
      </div>
    </section>
    <template #actions>
      <v-btn
        variant="text"
        color="error"
        :disabled="saving"
        @click="confirmingDelete = true">
        Delete tag
      </v-btn>
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
        Save changes
      </v-btn>
    </template>
  </BaseDialog>
  <DeleteTagDialog
    v-model="confirmingDelete"
    :tag="tag"
    @deleted="onDeleted" />
</template>

<style scoped>
.tag-dialog__record-label,
.tag-dialog__section-label {
  color: rgb(var(--v-theme-rust));
  font-family: var(--admin-font-mono);
  font-size: 0.68rem;
  font-weight: 700;
  letter-spacing: 0.09em;
  text-transform: uppercase;
}

.tag-dialog__record-name {
  color: rgb(var(--v-theme-muted));
  font-family: var(--admin-font-body);
  margin: 0.2rem 0 1rem;
}

.tag-dialog__projects {
  border: 1px solid rgb(var(--v-theme-rule));
}

.tag-dialog__section-heading {
  align-items: center;
  background: rgb(var(--v-theme-paper));
  border-bottom: 1px solid rgb(var(--v-theme-rule));
  display: flex;
  justify-content: space-between;
  padding: 0.75rem 1rem;
}

.tag-dialog__section-heading h3 {
  font-family: var(--admin-font-display);
  font-size: 1.15rem;
  line-height: 1.1;
}

.tag-dialog__count {
  align-items: center;
  background: rgb(var(--v-theme-amber));
  border: 1px solid rgb(var(--v-theme-ink));
  color: rgb(var(--v-theme-ink));
  display: inline-flex;
  font-family: var(--admin-font-mono);
  font-size: 0.75rem;
  height: 1.75rem;
  justify-content: center;
  min-width: 1.75rem;
  padding-inline: 0.35rem;
}

.tag-dialog__project-list {
  background: transparent;
  padding: 0;
}

.tag-dialog__project-list :deep(.v-list-item + .v-list-item) {
  border-top: 1px solid rgb(var(--v-theme-rule));
}

.tag-dialog__project--removing {
  background: color-mix(in srgb, rgb(var(--v-theme-error)) 10%, transparent);
  color: rgb(var(--v-theme-muted));
}

.tag-dialog__empty {
  align-items: center;
  color: rgb(var(--v-theme-muted));
  display: flex;
  font-family: var(--admin-font-body);
  gap: 0.75rem;
  justify-content: center;
  padding: 2rem 1rem;
  text-align: center;
}
</style>
