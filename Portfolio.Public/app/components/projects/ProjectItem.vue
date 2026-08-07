<script setup lang="ts">
import { computed } from 'vue'
import type { PublicProjectFragment } from '~/generated/graphql'
import { PublicProjectImageFragmentDoc } from '~/generated/graphql'
import { useFragment } from '~/generated'

const props = withDefaults(defineProps<{
  project: PublicProjectFragment
  imageLoading?: 'eager' | 'lazy'
}>(), {
  imageLoading: 'lazy'
})

const emit = defineEmits<{
  select: [projectId: string]
}>()

const mainImage = computed(() => {
  const raw = props.project.images[0]
  return raw ? useFragment(PublicProjectImageFragmentDoc, raw) : null
})

const hasAdditionalContent = computed(() => {
  return (
    (props.project.body?.trim() || '').length > 0
    || props.project.images.length > 0)
})

const projectTitleId = computed(() => `project-title-${props.project.id}`)
const projectSummaryId = computed(() => `project-summary-${props.project.id}`)

const selectProject = () => {
  emit('select', props.project.id)
}
</script>

<template>
  <UCard
    as="article"
    :ui="{ header: 'p-0' }"
    class="project-card"
    :class="{ 'cursor-pointer group project-card--interactive': hasAdditionalContent }">
    <button
      v-if="hasAdditionalContent"
      type="button"
      class="project-card__open folio-focus-ring"
      :aria-labelledby="projectTitleId"
      :aria-describedby="project.summary ? projectSummaryId : undefined"
      @click="selectProject"
      @keydown.enter.prevent="selectProject"
      @keydown.space.prevent="selectProject" />
    <template #header>
      <div class="project-image-frame editorial-corner-field">
        <div class="aspect-video bg-stone-100 dark:bg-stone-800">
          <StorageImage
            :storage-key="mainImage?.thumbKey"
            :alt="mainImage?.altText ?? 'Project image'"
            :loading="imageLoading"
            :class="{ 'transition-transform duration-300 group-hover:scale-105': hasAdditionalContent }"
            class="w-full h-full object-cover" />
        </div>
      </div>
    </template>
    <div class="project-copy">
      <span class="project-label">Project record</span>
      <h3
        :id="projectTitleId"
        class="project-title">
        {{ project.title }}
      </h3>
      <p
        v-if="project.summary"
        :id="projectSummaryId"
        class="project-summary line-clamp-3">
        {{ project.summary }}
      </p>
      <ProjectTagIcons
        :tags="project.tags"
        class="project-card__tags pt-1" />
    </div>
    <template
      v-if="project.links.length"
      #footer>
      <ProjectLinks
        :links="project.links"
        class="project-card__links" />
    </template>
  </UCard>
</template>

<style scoped>
.project-card {
  position: relative;
  height: 100%;
  overflow: visible;
  border: 0;
  border-radius: 0;
  background: var(--folio-paper-raised);
  box-shadow: 0 -1px 0 transparent;
  transition: background-color 180ms ease, box-shadow 180ms ease, translate 180ms ease;
}

.project-card__open {
  position: absolute;
  z-index: 1;
  inset: 0;
  padding: 0;
  border: 0;
  background: transparent;
  cursor: pointer;
}

.project-card__links {
  position: relative;
  z-index: 2;
}

.project-card__tags {
  position: relative;
  z-index: 2;
  width: fit-content;
}

.project-card--interactive:hover {
  z-index: 1;
  background: color-mix(in srgb, var(--folio-amber) 22%, var(--folio-paper-raised));
  box-shadow: 0 -1px 0 var(--folio-ink);
  translate: 0 -3px;
}

.project-image-frame {
  display: grid;
  min-height: 15rem;
  padding: 1rem;
  border-bottom: 1px solid var(--folio-ink);
  place-items: center;
}

.project-image-frame > div {
  width: 100%;
  overflow: hidden;
  border: 1px solid var(--folio-ink);
}

.project-copy {
  display: grid;
  gap: .65rem;
}

.project-label {
  font-family: var(--folio-font-mono);
  font-size: .62rem;
  font-weight: 700;
  letter-spacing: .12em;
}

.project-title {
  font-family: var(--folio-font-display);
  font-size: 1.7rem;
  font-stretch: condensed;
  font-weight: 900;
  letter-spacing: -.015em;
  line-height: 1;
}

.project-summary {
  color: var(--folio-muted);
  font-family: var(--folio-font-body);
  font-size: .95rem;
  line-height: 1.5;
}

@media (prefers-reduced-motion: reduce) {
  .project-card {
    transition: none;
  }
}
</style>
