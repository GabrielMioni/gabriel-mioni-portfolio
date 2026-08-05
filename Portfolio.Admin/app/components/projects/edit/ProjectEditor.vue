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

const menuItems = computed(() => {
  return [
    {
      title: 'Cancel',
      icon: 'mdi-open-in-app',
      action: () => {
        goToProjects()
      }
    },
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
  <v-container>
    <div
      v-if="isInitialLoading"
      class="d-flex justify-center">
      <v-progress-circular
        size="60"
        indeterminate />
    </div>
    <template v-else>
      <v-toolbar
        color="background"
        class="project-editor-toolbar position-sticky">
        <v-tabs v-model="tab">
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
            v-if="smAndDown"
            :items="menuItems" />
          <template v-else>
            <v-btn
              v-if="!isNewProject"
              text
              color="error"
              @click="deleteProjectDialog = true">
              Delete
            </v-btn>
            <v-btn
              text
              @click="goToProjects">
              Cancel
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
        flat>
        <v-tabs-window v-model="tab">
          <div class="mt-3">
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

.project-editor-toolbar {
  top: var(--v-layout-top);
  z-index: 10;
}
</style>
