<script setup lang="ts">
const enum tabValues {
  details = 'details',
  images = 'images',
  links = 'links'
}

const {
  // refs
  projectDetailsModel,
  imageItems,
  linkItems,
  tagItems,

  // computed
  activeLinkItems,
  isNewProject,
  isSavingProject,
  hasUpdates,
  isInitialLoading,
  projectId,

  submitProject
} = useProjectEditor()

const router = useRouter()

const { smAndDown } = useDisplay()

const tab = ref<string>(tabValues.details)
const deleteProjectDialog = ref<boolean>(false)
const linksValid = ref<boolean>(true)
const detailsValid = ref<boolean>(false)

const editorTitle = computed(() => {
  if (isNewProject.value) return 'New project'
  return projectDetailsModel.title || 'Untitled project'
})

const editorDescription = computed(() =>
  isNewProject.value
    ? 'Prepare a new project study for the public portfolio.'
    : 'Revise project content, media, and references.'
)

const menuItems = computed(() => {
  return [
    {
      title: 'Delete',
      icon: 'mdi-file-document',
      itemClass: 'text-error',
      filter: isNewProject.value,
      action: () => {
        deleteProjectDialog.value = true
      }
    }
  ].filter(i => !i.filter)
})

const goToProjects = async () => {
  const previous = router.options.history.state.back

  if (typeof previous === 'string') {
    router.back()
    return
  }

  await router.replace('/projects')
}

</script>

<template>
  <v-container
    fluid
    class="admin-page project-editor">
    <div
      v-if="isInitialLoading"
      class="project-editor-loading d-flex align-center justify-center">
      <v-progress-circular
        size="60"
        indeterminate />
    </div>
    <template v-else>
      <AdminPageHeader
        eyebrow="Project record"
        :title="editorTitle"
        :description="editorDescription">
        <template #actions>
          <v-btn
            prepend-icon="mdi-arrow-left"
            variant="text"
            @click="goToProjects">
            Back
          </v-btn>
        </template>
      </AdminPageHeader>
      <v-toolbar
        color="paper-raised"
        class="project-editor-toolbar position-sticky">
        <v-tabs
          v-model="tab"
          class="project-editor-tabs"
          color="rust"
          show-arrows>
          <v-tab :value="tabValues.details">Details</v-tab>
          <v-tab :value="tabValues.images">
            <EditorItemTabDisplay
              label="Images"
              :items="imageItems" />
          </v-tab>
          <v-tab :value="tabValues.links">
            <EditorItemTabDisplay
              label="Links"
              :items="linkItems" />
          </v-tab>
        </v-tabs>
        <v-spacer />
        <div class="project-editor-actions">
          <BaseMenu
            v-if="smAndDown && menuItems.length > 0"
            :items="menuItems"
            activator-label="Project actions" />
          <template v-else>
            <v-btn
              v-if="!isNewProject"
              text
              color="error"
              @click="deleteProjectDialog = true">
              Delete
            </v-btn>
          </template>
          <v-btn
            class="bg-primary"
            :disabled="isSavingProject || !detailsValid || (activeLinkItems.length > 0 && !linksValid) || !hasUpdates"
            :loading="isSavingProject"
            @click="submitProject">
            Save
          </v-btn>
        </div>
      </v-toolbar>
      <v-card
        class="project-editor-panel"
        flat>
        <v-tabs-window v-model="tab">
          <div class="project-editor-panel__content">
            <v-tabs-window-item :value="tabValues.details">
              <ProjectDetails
                v-model:form="projectDetailsModel"
                v-model:is-valid="detailsValid"
                v-model:assigned-tags="tagItems"
                show-tags />
            </v-tabs-window-item>
            <v-tabs-window-item :value="tabValues.images">
              <ProjectImageUpload
                v-model:items="imageItems" />
            </v-tabs-window-item>
            <v-tabs-window-item :value="tabValues.links">
              <ProjectLinks
                v-model:is-valid="linksValid"
                v-model:items="linkItems" />
            </v-tabs-window-item>
          </div>
        </v-tabs-window>
      </v-card>
      <template v-if="!isNewProject">
        <DeleteProjectDialog
          v-if="!isNewProject && projectId"
          v-model="deleteProjectDialog"
          :project-id="projectId"
          :summary="projectDetailsModel.summary"
          :title="projectDetailsModel.title"
          @deleted="goToProjects" />
      </template>
    </template>
  </v-container>
</template>

<style scoped>
.project-editor-actions {
  display: flex;
  gap: 0.5rem;
}

.project-editor-loading {
  min-height: 60vh;
}

.project-editor-toolbar {
  top: var(--v-layout-top);
  z-index: 5;
}

.project-editor-toolbar :deep(.v-toolbar__content) {
  border: 1px solid rgb(var(--v-theme-rule));
  border-top: 5px solid rgb(var(--v-theme-cyan));
  padding-inline: 0.5rem;
}

.project-editor-tabs {
  min-width: 0;
}

.project-editor-tabs :deep(.v-tab--selected) {
  background: color-mix(
    in srgb,
    rgb(var(--v-theme-amber)) 22%,
    rgb(var(--v-theme-paper-raised))
  );
  color: rgb(var(--v-theme-ink)) !important;
}

.project-editor-panel {
  background: rgb(var(--v-theme-paper-raised));
  border: 1px solid rgb(var(--v-theme-rule));
  border-top: 0;
  border-radius: 0;
}

.project-editor-panel__content {
  padding: clamp(1rem, 2.5vw, 2rem);
}

@media (max-width: 599px) {
  .project-editor-panel__content {
    padding: 0.75rem;
  }
}
</style>
