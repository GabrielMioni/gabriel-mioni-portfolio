<script setup lang="ts">
import { useProjectQueries } from '~/composables/useProjectQueries'
import ProjectItem from '~/components/projects/ProjectItem.vue'
import ProjectDialog from '~/components/projects/ProjectDialog.vue'

const dialogOpen = ref(false)
const selectedProjectId = ref<string | null>(null)

const {
  projects,
  fetchingProjects
} = useProjectQueries()

const selectedProject = computed(() =>
  projects.value.find(p => p.id === selectedProjectId.value) ?? null
)

const selectProject = (projectId: string) => {
  selectedProjectId.value = projectId
  dialogOpen.value = true
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
      v-model:open="dialogOpen"
      :project="selectedProject" />
  </UContainer>
</template>

<style scoped>

</style>
