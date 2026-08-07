<script setup lang="ts">
import { PublicProjectImageFragmentDoc } from '~/generated/graphql'
import { useFragment, type FragmentType } from '~/generated'

const props = defineProps<{
  images: FragmentType<typeof PublicProjectImageFragmentDoc>[]
}>()

const resolvedImages = computed(() =>
  props.images.map(i => useFragment(PublicProjectImageFragmentDoc, i))
)

const thumbnailViewport = useTemplateRef('thumbnail-viewport')
const lightbox = useTemplateRef('lightbox')
const imageButton = useTemplateRef('image-button')
const activeIndex = ref(0)

const selectedImage = computed(() =>
  resolvedImages.value[activeIndex.value] ?? null
)

const scrollActiveThumbnailIntoView = async () => {
  await nextTick()

  const viewport = thumbnailViewport.value
  const thumbnail = viewport?.querySelector<HTMLElement>(`[data-index="${activeIndex.value}"]`)

  if (!viewport || !thumbnail) {
    return
  }

  const centeredLeft = thumbnail.offsetLeft
    - ((viewport.clientWidth - thumbnail.offsetWidth) / 2)

  viewport.scrollTo({ left: centeredLeft, behavior: 'smooth' })
}

watch(activeIndex, scrollActiveThumbnailIntoView)

const onClickPrev = () => {
  activeIndex.value = activeIndex.value <= 0
    ? resolvedImages.value.length - 1
    : activeIndex.value - 1
}
const onClickNext = () => {
  activeIndex.value = activeIndex.value >= resolvedImages.value.length - 1
    ? 0
    : activeIndex.value + 1
}

const select = (index: number) => {
  activeIndex.value = index
}

const openLightbox = () => {
  lightbox.value?.showModal()
}

const closeLightbox = () => {
  lightbox.value?.close()
}

const focusMainImage = () => {
  imageButton.value?.focus()
}

defineExpose({ focusMainImage })

const formatThumbnailAltText = (altText: string | null, type: string = 'thumbnail') => {
  if (altText && altText.length > 0) {
    return type === 'thumbnail'
      ? `Thumbnail of ${altText}`
      : `Full image of ${altText}`
  }
  return `Project ${type} image`
}
</script>

<template>
  <div
    class="project-carousel"
    @keydown.left.prevent="onClickPrev"
    @keydown.right.prevent="onClickNext">
    <figure class="project-carousel__plate">
      <div class="project-carousel__image-stage editorial-corner-field">
        <button
          ref="image-button"
          type="button"
          class="project-carousel__image-button folio-focus-ring folio-focus-ring--inset"
          :aria-label="`Enlarge ${selectedImage?.altText ?? 'project figure'}`"
          @click="openLightbox">
          <StorageImage
            :storage-key="selectedImage?.fullKey"
            :alt="formatThumbnailAltText(selectedImage?.altText ?? null, 'full')"
            class="project-carousel__image" />
          <span
            class="project-carousel__zoom-indicator folio-accent-control"
            aria-hidden="true">
            <UIcon
              name="i-lucide-zoom-in"
              class="size-5" />
          </span>
        </button>
      </div>
      <figcaption class="project-carousel__caption">
        <span>Project figure</span>
        <span>{{ String(activeIndex + 1).padStart(2, '0') }} / {{ String(resolvedImages.length).padStart(2, '0') }}</span>
      </figcaption>
    </figure>
    <div
      v-if="resolvedImages.length > 1"
      class="project-carousel__strip">
      <button
        type="button"
        class="project-carousel__control folio-accent-control folio-focus-ring folio-focus-ring--compact hover:text-dark-ink"
        aria-label="Previous project figure"
        @click="onClickPrev">
        <UIcon
          name="i-lucide-arrow-left"
          class="size-5" />
      </button>
      <div
        ref="thumbnail-viewport"
        class="project-carousel__thumbnail-viewport">
        <div class="project-carousel__thumbnail-track">
          <button
            v-for="(item, index) in resolvedImages"
            :key="index"
            type="button"
            class="project-carousel__thumbnail"
            :class="{ 'project-carousel__thumbnail--active': activeIndex === index }"
            :data-index="index"
            :aria-label="`View project figure ${index + 1}`"
            :aria-pressed="activeIndex === index"
            @click="select(index)">
            <StorageImage
              :storage-key="item.thumbKey"
              :alt="formatThumbnailAltText(item.altText)"
              class="project-carousel__thumbnail-image" />
          </button>
        </div>
      </div>
      <button
        type="button"
        class="project-carousel__control folio-accent-control folio-focus-ring folio-focus-ring--compact hover:text-dark-ink"
        aria-label="Next project figure"
        @click="onClickNext">
        <UIcon
          name="i-lucide-arrow-right"
          class="size-5" />
      </button>
    </div>
    <dialog
      ref="lightbox"
      class="project-lightbox"
      aria-labelledby="project-lightbox-title"
      @keydown.esc.stop.prevent="closeLightbox"
      @click.self="closeLightbox">
      <div class="project-lightbox__layout">
        <header class="project-lightbox__header">
          <div>
            <span class="project-lightbox__label">Enlarged figure</span>
            <h2 id="project-lightbox-title">
              {{ selectedImage?.altText ?? 'Project figure' }}
            </h2>
          </div>
          <button
            type="button"
            class="project-lightbox__close folio-accent-control folio-focus-ring folio-focus-ring--compact hover:text-dark-ink"
            aria-label="Close enlarged figure"
            @click="closeLightbox">
            <UIcon
              name="i-lucide-x"
              class="size-5" />
          </button>
        </header>
        <div class="project-lightbox__image-stage">
          <StorageImage
            :storage-key="selectedImage?.fullKey"
            :alt="formatThumbnailAltText(selectedImage?.altText ?? null, 'full')"
            class="project-lightbox__image" />
        </div>
        <footer class="project-lightbox__controls">
          <button
            type="button"
            class="project-lightbox__control folio-accent-control folio-focus-ring folio-focus-ring--compact hover:text-dark-ink"
            aria-label="Previous enlarged figure"
            @click="onClickPrev">
            <UIcon
              name="i-lucide-arrow-left"
              class="size-5" />
            <span>Previous</span>
          </button>
          <span class="project-lightbox__count">
            {{ String(activeIndex + 1).padStart(2, '0') }} / {{ String(resolvedImages.length).padStart(2, '0') }}
          </span>
          <button
            type="button"
            class="project-lightbox__control folio-accent-control folio-focus-ring folio-focus-ring--compact hover:text-dark-ink"
            aria-label="Next enlarged figure"
            @click="onClickNext">
            <span>Next</span>
            <UIcon
              name="i-lucide-arrow-right"
              class="size-5" />
          </button>
        </footer>
      </div>
    </dialog>
  </div>
</template>

<style scoped>
.project-carousel {
  display: grid;
  grid-template-rows: minmax(0, 1fr) auto;
  gap: 1rem;
  width: 100%;
  height: 100%;
  min-height: 0;
}

.project-carousel__plate {
  display: grid;
  grid-template-rows: minmax(0, 1fr) auto;
  min-height: 0;
  border: 1px solid var(--folio-ink);
  background: var(--folio-paper-raised);
}

.project-carousel__image-stage {
  display: grid;
  min-height: 0;
  padding: clamp(1rem, 3vw, 2.25rem);
  overflow: hidden;
  --editorial-corner-background: var(--folio-paper-raised);
  place-items: center;
}

.project-carousel__image-button {
  position: relative;
  display: grid;
  width: 100%;
  height: 100%;
  min-width: 0;
  min-height: 0;
  padding: 0;
  overflow: hidden;
  border: 0;
  color: var(--folio-ink);
  background: transparent;
  cursor: zoom-in;
  place-items: center;
}

.project-carousel__image {
  width: 100%;
  height: 100%;
  min-width: 0;
  min-height: 0;
  object-fit: contain;
}

.project-carousel__zoom-indicator {
  position: absolute;
  top: .75rem;
  right: .75rem;
  display: grid;
  width: 2.5rem;
  height: 2.5rem;
  transition: translate 140ms ease;
  place-items: center;
}

.project-carousel__image-button:hover .project-carousel__zoom-indicator {
  color: var(--folio-dark-ink);
  background: var(--folio-amber) !important;
  translate: -2px 2px;
}

.project-carousel__image-button:focus-visible .project-carousel__zoom-indicator {
  translate: -2px 2px;
}

.project-carousel__caption {
  display: flex;
  justify-content: space-between;
  padding: .65rem .85rem;
  border-top: 1px solid var(--folio-ink);
  color: var(--folio-muted);
  font-family: var(--folio-font-mono);
  font-size: .65rem;
  font-weight: 700;
  letter-spacing: .1em;
}

.project-carousel__strip {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto;
  gap: .75rem;
  align-items: center;
  padding: .5rem;
  border: 1px solid var(--folio-ink);
  background: var(--folio-paper-raised);
}

.project-carousel__control {
  display: grid;
  width: 2.25rem;
  height: 2.25rem;
  padding: 0;
  cursor: pointer;
  place-items: center;
}

.project-carousel__thumbnail-viewport {
  min-width: 0;
  overflow-x: auto;
  overscroll-behavior-inline: contain;
  scrollbar-width: none;
}

.project-carousel__thumbnail-viewport::-webkit-scrollbar {
  display: none;
}

.project-carousel__thumbnail-track {
  display: flex;
  width: max-content;
  min-width: 100%;
  gap: .5rem;
  justify-content: center;
}

.project-carousel__thumbnail {
  position: relative;
  display: block;
  width: clamp(4rem, 6vw, 5rem);
  aspect-ratio: 1;
  padding: 0;
  overflow: hidden;
  border: 1px solid var(--folio-rule);
  border-radius: 0;
  background: var(--folio-paper);
  cursor: pointer;
}

.project-carousel__thumbnail::after {
  position: absolute;
  inset: 0;
  border: 3px solid transparent;
  content: '';
  pointer-events: none;
}

.project-carousel__thumbnail:hover,
.project-carousel__thumbnail:focus-visible {
  border-color: var(--folio-ink);
  outline: 0;
}

.project-carousel__thumbnail--active::after {
  border-color: var(--folio-cyan);
}

.project-carousel__thumbnail-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.project-lightbox {
  inset: 0;
  width: 100vw;
  height: 100dvh;
  max-width: none;
  max-height: none;
  margin: 0;
  padding: 0;
  overflow: hidden;
  border: 0;
  border-top: 7px solid var(--folio-cyan);
  border-radius: 0;
  color: var(--folio-ink);
  background: var(--folio-paper-raised);
}

.project-lightbox::backdrop {
  background: color-mix(in srgb, var(--folio-ink) 72%, transparent);
  backdrop-filter: blur(3px);
}

.project-lightbox[open] {
  display: grid;
}

.project-lightbox__layout {
  display: grid;
  grid-template-rows: auto minmax(0, 1fr) auto;
  width: 100%;
  height: 100%;
  min-height: 0;
}

.project-lightbox__header {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: center;
  padding: .8rem 1rem;
  border-bottom: 1px solid var(--folio-ink);
}

.project-lightbox__label,
.project-lightbox__count,
.project-lightbox__control {
  font-family: var(--folio-font-mono);
  font-size: .68rem;
  font-weight: 700;
  letter-spacing: .08em;
}

.project-lightbox__label {
  display: block;
  margin-bottom: .2rem;
  color: var(--folio-rust);
}

.project-lightbox h2 {
  font-family: var(--folio-font-display);
  font-size: clamp(1.25rem, 2.5vw, 2rem);
  font-stretch: condensed;
  font-weight: 900;
  line-height: 1;
}

.project-lightbox__close,
.project-lightbox__control {
  cursor: pointer;
}

.project-lightbox__close {
  display: grid;
  width: 2.5rem;
  height: 2.5rem;
  padding: 0;
  place-items: center;
}

.project-lightbox__image-stage {
  display: grid;
  min-width: 0;
  min-height: 0;
  padding: clamp(1rem, 3vw, 2.5rem);
  overflow: hidden;
  background: var(--folio-paper);
  place-items: center;
}

.project-lightbox__image {
  width: 100%;
  height: 100%;
  min-width: 0;
  min-height: 0;
  object-fit: contain;
}

.project-lightbox__controls {
  display: grid;
  grid-template-columns: 1fr auto 1fr;
  gap: 1rem;
  align-items: center;
  padding: .75rem 1rem;
  border-top: 1px solid var(--folio-ink);
}

.project-lightbox__control {
  display: inline-flex;
  gap: .5rem;
  align-items: center;
  width: fit-content;
  min-height: 2.5rem;
  padding: .45rem .75rem;
}

.project-lightbox__control:last-child {
  justify-self: end;
}

.project-lightbox__count {
  color: var(--folio-muted);
}

@media (max-width: 47.99rem) {
  .project-carousel {
    grid-template-rows: auto auto;
    height: auto;
  }

  .project-carousel__plate {
    grid-template-rows: auto auto;
  }

  .project-carousel__image-stage {
    height: min(60vh, 30rem);
    min-height: 15rem;
  }

  .project-lightbox__control span {
    display: none;
  }
}
</style>
