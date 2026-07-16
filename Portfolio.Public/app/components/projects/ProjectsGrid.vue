<script setup lang="ts">
import { useProjectQueries } from '~/composables/useProjectQueries'
import ProjectItem from '~/components/projects/ProjectItem.vue'
import ProjectDialog from '~/components/projects/ProjectDialog.vue'

const dialogOpen = ref(false)
const selectedProjectId = ref<string | null>(null)
const selectedTags = ref<string[]>([])

const {
  projects,
  fetchingProjects,
  hasNextPage,
  loadMore,
  availableTags
} = useProjectQueries(selectedTags)

const selectedProject = computed(() =>
  projects.value.find(p => p.id === selectedProjectId.value) ?? null
)

const selectProject = (projectId: string) => {
  selectedProjectId.value = projectId
  dialogOpen.value = true
}

const toggleTag = (value: string) => {
  const idx = selectedTags.value.indexOf(value)
  if (idx === -1) selectedTags.value = [...selectedTags.value, value]
  else selectedTags.value = selectedTags.value.filter(v => v !== value)
}

const observer = useTemplateRef('observer')

useIntersectionObserver(observer, ([entry]) => {
  if (!entry) return
  if (entry.isIntersecting && hasNextPage.value && !fetchingProjects.value) {
    loadMore()
  }
})
</script>

<template>
  <UContainer>
    <TransitionGroup
      v-if="availableTags.length > 0"
      tag="div"
      name="tag"
      class="flex flex-wrap gap-2 mb-6"
      appear>
      <UButton
        v-for="(tag, index) in availableTags"
        :key="tag.value"
        :label="tag.name"
        :variant="selectedTags.includes(tag.value) ? 'solid' : 'outline'"
        :style="{ transitionDelay: `${index * 25}ms` }"
        color="primary"
        size="xs"
        class="select-none"
        @click="toggleTag(tag.value)" />
    </TransitionGroup>
    <TransitionGroup
      tag="div"
      class="grid gap-6 sm:grid-cols-2 lg:grid-cols-3"
      appear
      @before-leave="(el) => { (el as HTMLElement).style.transitionDelay = '0ms' }">
      <ProjectItem
        v-for="(item, index) in projects"
        :key="item.id"
        :project="item"
        :style="{ transitionDelay: `${index * 40}ms` }"
        @select="selectProject" />
    </TransitionGroup>
    <div
      v-if="!fetchingProjects && projects.length <= 0"
      class="flex flex-col items-center justify-center py-24 text-stone-400 dark:text-stone-600">
      <UIcon
        name="i-lucide-ghost"
        class="size-10 mb-3" />
      <p class="text-sm">
        No projects yet.
      </p>
    </div>
    <div ref="observer" />
    <div
      v-if="fetchingProjects"
      class="flex justify-center py-8">
      <UIcon
        name="i-lucide-loader-circle"
        class="size-6 text-stone-400 animate-spin" />
    </div>
    <ProjectDialog
      v-model:open="dialogOpen"
      :project="selectedProject" />
  </UContainer>
</template>

<style scoped>
.v-enter-active {
  transition: opacity 0.4s ease, transform 0.4s ease;
}
.v-enter-from {
  opacity: 0;
  transform: translateY(10px);
}
.v-leave-active {
  transition: opacity 0.2s ease;
}
.v-leave-to {
  opacity: 0;
}

.tag-enter-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}
.tag-enter-from {
  opacity: 0;
  transform: translateY(-6px);
}
</style>
