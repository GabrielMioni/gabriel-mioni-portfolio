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

  // computed
  activeImageItems,
  activeLinkItems,
  removedImageItems,
  removedLinkItems,
  isNewProject,
  isSavingProject,
  hasUpdates,
  isInitialLoading,
  projectId,
  projectLinksIsValid,

  // methods
  restoreImageItem,
  restoreLinkItem,
  submitProject
} = useProjectEditor()

const router = useRouter()

const tab = ref<string>(tabValues.details)
const removedDialog = ref(false)
const deleteProjectDialog = ref<boolean>(false)

const hasRemovedItems = computed(() => {
  if (tab.value === tabValues.images) {
    return removedImageItems.value.length > 0
  }
  if (tab.value === tabValues.links) {
    return removedLinkItems.value.length > 0
  }
  return true
})

const removedItemText = computed(() => {
  if (!hasRemovedItems.value) {
    return 'Empty'
  }
  if (tab.value === tabValues.images) {
    return `Removed Images (${removedImageItems.value.length})`
  }
  if (tab.value === tabValues.links) {
    return `Removed Links (${removedLinkItems.value.length})`
  }
  return ''
})

const openRemovedItemsDialog = () => {
  if (!hasRemovedItems.value) return
  removedDialog.value = true
}

const goToProjects = () => {
  router.push('/projects')
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
        class="position-sticky top-0"
        style="z-index: 999">
        <v-tabs v-model="tab">
          <v-tab :value="tabValues.details">Details</v-tab>
          <v-tab :value="tabValues.images">
            <EditorItemTabDisplay
              label="Images"
              :items="activeImageItems" />
          </v-tab>
          <v-tab :value="tabValues.links">
            <EditorItemTabDisplay
              label="Links"
              :items="activeLinkItems" />
          </v-tab>
        </v-tabs>
        <v-spacer />
        <v-card-actions>
          <v-btn
            v-if="!isNewProject && (tab === tabValues.images || tab === tabValues.links)"
            text
            prepend-icon="mdi-trash-can-outline"
            :disabled="!hasRemovedItems"
            @click="openRemovedItemsDialog">
            {{ removedItemText }}
          </v-btn>
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
          <v-btn
            class="bg-primary"
            :disabled="isSavingProject || !projectLinksIsValid || !hasUpdates"
            :loading="isSavingProject"
            @click="submitProject">
            Save
          </v-btn>
        </v-card-actions>
      </v-toolbar>
      <v-card
        flat>
        <v-tabs-window v-model="tab">
          <div class="mt-3">
            <v-tabs-window-item :value="tabValues.details">
              <ProjectDetails
                v-model:form="projectDetailsModel"
                v-model:is-valid="projectLinksIsValid" />
            </v-tabs-window-item>
            <v-tabs-window-item :value="tabValues.images">
              <ProjectImageUpload
                v-model:items="imageItems" />
            </v-tabs-window-item>
            <v-tabs-window-item :value="tabValues.links">
              <ProjectLinks
                v-model:is-valid="projectLinksIsValid"
                v-model:items="linkItems" />
            </v-tabs-window-item>
          </div>
        </v-tabs-window>
      </v-card>
      <template v-if="!isNewProject">
        <RemovedImagesDialog
          v-if="tab === tabValues.images"
          v-model="removedDialog"
          :removed-image-items="removedImageItems"
          @add="restoreImageItem" />
        <RemovedLinksDialog
          v-if="tab === tabValues.links"
          v-model="removedDialog"
          :removed-link-items="removedLinkItems"
          @add="restoreLinkItem" />
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

</style>
