<script setup lang="ts">
import {
  computed,
  ref,
  useTemplateRef
} from 'vue'
import { useIntersectionObserver } from '~/composables/useIntersectionObserver'
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
  <div>
    <TransitionGroup
      v-if="availableTags.length > 0"
      tag="div"
      name="tag"
      class="project-filters"
      appear>
      <UIcon
        key="filter-icon"
        name="i-lucide-filter"
        class="size-3.5 text-stone-400 dark:text-stone-500 shrink-0" />
      <UButton
        key="all"
        label="All"
        :variant="selectedTags.length === 0 ? 'solid' : 'outline'"
        color="primary"
        size="xs"
        class="select-none"
        @click="selectedTags = []" />
      <UButton
        v-for="(tag, index) in availableTags"
        :key="tag.value"
        :label="tag.name"
        :variant="selectedTags.includes(tag.value) ? 'solid' : 'outline'"
        :style="{ transitionDelay: `${(index + 1) * 25}ms` }"
        color="primary"
        size="xs"
        class="select-none"
        @click="toggleTag(tag.value)" />
    </TransitionGroup>
    <TransitionGroup
      tag="div"
      class="project-grid"
      appear
      @before-leave="(el) => { (el as HTMLElement).style.transitionDelay = '0ms' }">
      <div
        v-for="(item, index) in projects"
        :key="item.id"
        class="project-cell"
        :style="{ transitionDelay: `${index * 40}ms` }">
        <ProjectItem
          :project="item"
          @select="selectProject" />
      </div>
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
      class="grid gap-6 sm:grid-cols-2 lg:grid-cols-3 mt-6">
      <ProjectItemSkeleton
        v-for="n in (projects.length === 0 ? 9 : 3)"
        :key="n" />
    </div>
    <ProjectDialog
      v-model:open="dialogOpen"
      :project="selectedProject" />
  </div>
</template>

<style scoped>
.project-filters {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: .5rem;
  margin-bottom: 2rem;
  padding: .75rem;
  border: 1px solid var(--folio-ink);
  background: var(--folio-paper-raised);
}

.project-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 1rem;
}

.project-cell {
  position: relative;
  min-width: 0;
  padding: 1px;
  background: var(--folio-ink);
}

@media (max-width: 64rem) {
  .project-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 40rem) {
  .project-grid {
    grid-template-columns: 1fr;
  }
}

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
