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
    <section
      v-if="availableTags.length > 0"
      class="project-filters"
      aria-labelledby="project-filter-label">
      <div class="project-filters__heading">
        <div
          id="project-filter-label"
          class="project-filters__label">
          <UIcon
            name="i-lucide-filter"
            class="size-3.5 shrink-0" />
          <span>Filter by technology</span>
        </div>
        <span class="project-filters__status">
          {{ selectedTags.length === 0 ? 'Showing all' : `${selectedTags.length} selected` }}
        </span>
      </div>
      <TransitionGroup
        tag="div"
        name="tag"
        class="project-filters__options"
        appear>
        <button
          key="all"
          type="button"
          class="project-filter folio-focus-ring folio-focus-ring--compact"
          :class="{
            'project-filter--active': selectedTags.length === 0,
            'text-dark-ink': selectedTags.length === 0
          }"
          :aria-pressed="selectedTags.length === 0"
          @click="selectedTags = []">
          All
        </button>
        <button
          v-for="(tag, index) in availableTags"
          :key="tag.value"
          type="button"
          class="project-filter folio-focus-ring folio-focus-ring--compact"
          :class="{
            'project-filter--active': selectedTags.includes(tag.value),
            'text-dark-ink': selectedTags.includes(tag.value)
          }"
          :style="{ transitionDelay: `${(index + 1) * 25}ms` }"
          :aria-pressed="selectedTags.includes(tag.value)"
          @click="toggleTag(tag.value)">
          {{ tag.name }}
        </button>
      </TransitionGroup>
    </section>
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
  margin-bottom: 2rem;
  border: 1px solid var(--folio-ink);
  background: var(--folio-paper-raised);
}

.project-filters__heading {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: center;
  padding: .55rem .75rem;
  border-bottom: 1px solid var(--folio-ink);
  background: var(--folio-paper);
}

.project-filters__label,
.project-filters__status,
.project-filter {
  font-family: var(--folio-font-mono);
  font-size: .68rem;
  font-weight: 700;
  letter-spacing: .08em;
}

.project-filters__label {
  display: flex;
  gap: .55rem;
  align-items: center;
}

.project-filters__status {
  color: var(--folio-muted);
}

.project-filters__options {
  display: flex;
  flex-wrap: wrap;
  gap: .5rem;
  padding: .75rem;
}

.project-filter {
  min-height: 2rem;
  padding: .35rem .7rem;
  border: 1px solid var(--folio-rule);
  border-radius: 0;
  color: var(--folio-ink);
  background: transparent;
  cursor: pointer;
  transition: color 140ms ease, background-color 140ms ease, border-color 140ms ease;
  user-select: none;
}

.project-filter:hover {
  border-color: var(--folio-ink);
  background: color-mix(in srgb, var(--folio-cyan) 14%, transparent);
}

.project-filter--active {
  border-color: var(--folio-ink);
  background: var(--folio-amber);
  box-shadow: inset 0 -3px 0 var(--folio-rust);
}

.project-filter--active:hover {
  color: var(--folio-ink) !important;
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
