<script setup lang="ts">
import type { PublicProjectFragment } from '~/generated/graphql'
import ProjectImageCarousel from '~/components/projects/ProjectImageCarousel/ProjectImageCarousel.vue'
import ProjectLinks from '~/components/projects/ProjectLinks.vue'

const open = defineModel<boolean>('open', { required: true })

const textSection = useTemplateRef('text-section')
const imageCarousel = useTemplateRef('image-carousel')

const scrollText = (amount: number) => {
  const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches

  textSection.value?.scrollBy({
    top: amount,
    behavior: reducedMotion ? 'auto' : 'smooth'
  })
}

const { md: isMobile } = useMediaQuery()

const props = defineProps<{
  project?: PublicProjectFragment | null
}>()

const projectImages = computed(() => props.project?.images ?? [])
const hasImages = computed(() => projectImages.value.length > 0)

const formatCount = (count: number, singular: string, plural = `${singular}s`) =>
  `${count} ${count === 1 ? singular : plural}`

const recordSummary = computed(() => [
  (props.project?.tags.length ?? 0) > 0
    ? formatCount(props.project?.tags.length ?? 0, 'technology', 'technologies')
    : null,
  (props.project?.links.length ?? 0) > 0
    ? formatCount(props.project?.links.length ?? 0, 'link')
    : null
].filter(summary => summary !== null).join(' · '))

const figureSummary = computed(() => projectImages.value.length > 0
  ? formatCount(projectImages.value.length, 'figure')
  : '')

watch(open, async (isOpen) => {
  if (!isOpen || !hasImages.value) {
    return
  }

  await nextTick()
  imageCarousel.value?.focusMainImage()
}, { flush: 'post' })

const modalUi = computed(() => ({
  overlay: 'project-dialog__overlay',
  content: `project-dialog ${isMobile.value ? 'project-dialog--fullscreen' : `${hasImages.value ? 'w-[min(92vw,78rem)]' : 'w-[min(92vw,52rem)]'} h-[min(90vh,60rem)] max-w-none`}`,
  header: 'project-dialog__header',
  title: 'project-dialog__title',
  description: 'project-dialog__description',
  body: 'project-dialog__body',
  close: 'project-dialog__close folio-accent-control folio-focus-ring folio-focus-ring--compact hover:text-dark-ink'
}))
</script>

<template>
  <UModal
    v-model:open="open"
    :fullscreen="isMobile"
    :ui="modalUi"
    :title="project?.title"
    description="Project study">
    <template #body>
      <div
        class="project-dialog__layout"
        :class="{ 'project-dialog__layout--text-only': !hasImages }">
        <section class="project-dialog__copy">
          <div
            v-if="recordSummary || figureSummary"
            class="project-dialog__record-line">
            <span v-if="recordSummary">{{ recordSummary }}</span>
            <span
              v-if="figureSummary"
              class="project-dialog__figure-count">{{ figureSummary }}</span>
          </div>
          <div
            ref="text-section"
            class="project-dialog__text folio-focus-ring"
            role="region"
            tabindex="0"
            aria-label="Project description"
            @keydown.up.prevent="scrollText(-80)"
            @keydown.down.prevent="scrollText(80)">
            <div
              v-if="project?.tags.length"
              class="project-dialog__metadata">
              <span class="project-dialog__metadata-label">Technologies</span>
              <ProjectTagIcons
                :tags="project.tags"
                class="project-dialog__tags" />
            </div>
            <div
              v-if="project?.body"
              class="project-dialog__prose whitespace-pre-wrap">
              {{ project.body }}
            </div>
            <div
              v-else-if="project?.summary"
              class="project-dialog__prose">
              {{ project.summary }}
            </div>
          </div>
          <footer
            v-if="project?.links.length"
            class="project-dialog__references">
            <span class="project-dialog__metadata-label">Links</span>
            <ProjectLinks
              :links="project.links"
              class="project-dialog__links" />
          </footer>
        </section>
        <section
          v-if="hasImages"
          class="project-dialog__figures"
          aria-label="Project figures">
          <ProjectImageCarousel
            ref="image-carousel"
            :images="projectImages" />
        </section>
      </div>
    </template>
  </UModal>
</template>

<style>
.project-dialog__overlay {
  background: color-mix(in srgb, var(--folio-ink) 58%, transparent);
  backdrop-filter: blur(2px);
}

.project-dialog {
  overflow: hidden;
  border: 1px solid var(--folio-ink);
  border-top: 7px solid var(--folio-cyan);
  border-radius: 0;
  color: var(--folio-ink);
  background: var(--folio-paper-raised);
  box-shadow: 12px 12px 0 color-mix(in srgb, var(--folio-ink) 30%, transparent);
}

.project-dialog--fullscreen {
  border-right: 0;
  border-bottom: 0;
  border-left: 0;
  box-shadow: none;
}

.project-dialog__header {
  min-height: 5.25rem;
  padding: .7rem clamp(1.25rem, 3vw, 2.75rem);
  border-bottom: 1px solid var(--folio-ink);
  background: var(--folio-paper-raised);
}

.project-dialog__title {
  max-width: 24ch;
  font-family: var(--folio-font-display);
  font-size: clamp(1.85rem, 3.5vw, 3.25rem);
  font-stretch: condensed;
  font-weight: 900;
  letter-spacing: -.035em;
  line-height: .95;
}

.project-dialog__description,
.project-dialog__metadata-label,
.project-dialog__record-line {
  font-family: var(--folio-font-mono);
  font-size: .68rem;
  font-weight: 700;
  letter-spacing: .11em;
}

.project-dialog__description {
  margin-bottom: .55rem;
  color: var(--folio-rust);
  text-transform: none;
}

.project-dialog__body {
  min-height: 0;
  padding: 0;
  overflow: hidden;
}

.project-dialog__layout {
  display: grid;
  grid-template-areas: 'copy figures';
  grid-template-columns: minmax(18rem, .8fr) minmax(24rem, 1.2fr);
  height: 100%;
  min-height: 0;
}

.project-dialog__layout--text-only {
  display: block;
}

.project-dialog__copy {
  display: flex;
  grid-area: copy;
  min-width: 0;
  min-height: 0;
  flex-direction: column;
  border-right: 1px solid var(--folio-ink);
}

.project-dialog__layout--text-only .project-dialog__copy {
  height: 100%;
  border-right: 0;
}

.project-dialog__record-line {
  display: flex;
  justify-content: space-between;
  padding: .7rem clamp(1.25rem, 3vw, 2.5rem);
  border-bottom: 1px solid var(--folio-rule);
  color: var(--folio-muted);
  background: var(--folio-paper);
}

.project-dialog__figure-count {
  margin-left: auto;
}

.project-dialog__text {
  flex: 1;
  min-height: 0;
  padding: clamp(1.5rem, 3vw, 2.75rem);
  overflow-y: auto;
}

.project-dialog__metadata {
  display: flex;
  gap: 1rem;
  align-items: center;
  padding-bottom: 1rem;
  margin-bottom: 1.5rem;
  border-bottom: 1px solid var(--folio-rule);
}

.project-dialog__metadata-label {
  color: var(--folio-muted);
}

.project-dialog__tags {
  gap: .65rem;
}

.project-dialog__tags svg {
  width: 1.1rem;
  height: 1.1rem;
  color: var(--folio-ink);
}

.project-dialog__prose {
  color: var(--folio-ink);
  font-family: var(--folio-font-body);
  font-size: clamp(1rem, 1.35vw, 1.12rem);
  line-height: 1.72;
}

.project-dialog__references {
  display: grid;
  gap: .75rem;
  padding: 1rem clamp(1.25rem, 3vw, 2.5rem) 1.25rem;
  border-top: 1px solid var(--folio-ink);
  background: var(--folio-paper);
}

.project-dialog__figures {
  grid-area: figures;
  min-width: 0;
  min-height: 0;
  padding: clamp(1rem, 2.5vw, 2rem);
  overflow: hidden;
  background: var(--folio-paper);
}

@media (max-width: 47.99rem) {
  .project-dialog {
    overflow-y: auto;
  }

  .project-dialog__header {
    min-height: auto;
  }

  .project-dialog__body {
    overflow: visible;
  }

  .project-dialog__layout {
    display: grid;
    grid-template-areas:
      'figures'
      'copy';
    grid-template-columns: minmax(0, 1fr);
    height: auto;
  }

  .project-dialog__copy {
    border-top: 1px solid var(--folio-ink);
    border-right: 0;
  }

  .project-dialog__layout--text-only .project-dialog__copy {
    border-top: 0;
  }

  .project-dialog__text {
    overflow: visible;
  }

  .project-dialog__figures {
    overflow: visible;
  }
}
</style>
