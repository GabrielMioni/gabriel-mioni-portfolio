<script setup lang="ts">
import { useProjectQueries } from '~/composables/useProjectQueries'
import ProjectItem from '~/components/projects/ProjectItem.vue'
import ProjectDialog from '~/components/projects/ProjectDialog.vue'

const selectedProjectId = ref<string | null>(null)

const {
  projects,
  fetchingProjects
} = useProjectQueries()

const projectDialogOpen = computed({
  get: () => selectedProjectId.value !== null,
  set: (open: boolean) => {
    if (!open) {
      selectedProjectId.value = null
    }
  }
})

const selectedProject = computed(() => {
  if (!selectedProjectId.value) {
    return null
  }
  return projects.value.find(project => project.id === selectedProjectId.value) ?? null
})

const selectProject = (projectId: string) => {
  selectedProjectId.value = projectId
}
</script>

<template>
  <UContainer>
    <div class="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
      <ProjectItem
        v-for="item in projects"
        :key="item.id"
        :project="item"
        @select="selectProject" />
    </div>
    <ProjectDialog
      v-model:open="projectDialogOpen"
      :project="selectedProject" />
  </UContainer>
</template>

<style scoped>

</style>
