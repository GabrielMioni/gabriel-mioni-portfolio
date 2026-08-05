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
const activeIndex = ref(0)

onMounted(() => {
  addEventListener('keydown', keyNavigation)
})

onUnmounted(() => {
  removeEventListener('keydown', keyNavigation)
})

const keyNavigation = (event: KeyboardEvent) => {
  if (event.key === 'ArrowLeft') {
    onClickPrev()
  } else if (event.key === 'ArrowRight') {
    onClickNext()
  }
}

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

const formatThumbnailAltText = (altText: string | null, type: string = 'thumbnail') => {
  if (altText && altText.length > 0) {
    return `Thumbnail of ${altText}`
  }
  return `Project ${type} image`
}
</script>

<template>
  <div class="project-carousel">
    <figure class="project-carousel__plate">
      <div class="project-carousel__image-stage editorial-corner-field">
        <StorageImage
          :storage-key="selectedImage?.fullKey"
          :alt="formatThumbnailAltText(selectedImage?.altText ?? null, 'full')"
          class="project-carousel__image" />
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
        class="project-carousel__control"
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
        class="project-carousel__control"
        aria-label="Next project figure"
        @click="onClickNext">
        <UIcon
          name="i-lucide-arrow-right"
          class="size-5" />
      </button>
    </div>
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

.project-carousel__image {
  width: 100%;
  height: 100%;
  min-width: 0;
  min-height: 0;
  object-fit: contain;
}

.project-carousel__caption {
  display: flex;
  justify-content: space-between;
  padding: .65rem .85rem;
  border-top: 1px solid var(--folio-ink);
  color: var(--folio-muted);
  font-family: 'Courier New', monospace;
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
  border: 1px solid var(--folio-ink);
  border-radius: 0;
  color: var(--folio-ink);
  background: var(--folio-amber);
  cursor: pointer;
  place-items: center;
}

.project-carousel__control:hover,
.project-carousel__control:focus-visible {
  background: color-mix(in srgb, var(--folio-amber) 72%, var(--folio-paper-raised));
  outline: 2px solid var(--folio-ink);
  outline-offset: 2px;
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
}
</style>
