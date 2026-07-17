<script setup lang="ts">
import { PublicProjectImageFragmentDoc } from '~/generated/graphql'
import { useFragment, type FragmentType } from '~/generated'

const props = defineProps<{
  images: FragmentType<typeof PublicProjectImageFragmentDoc>[]
}>()

const resolvedImages = computed(() =>
  props.images.map(i => useFragment(PublicProjectImageFragmentDoc, i))
)

const carousel = useTemplateRef('carousel')
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

watch(activeIndex, (index) => {
  carousel.value?.emblaApi?.scrollTo(index)
})

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
  <div class="w-full">
    <div class="h-65 flex items-center justify-center bg-stone-100 dark:bg-stone-800 rounded-lg overflow-hidden mb-4">
      <StorageImage
        :storage-key="selectedImage?.fullKey"
        :alt="formatThumbnailAltText(selectedImage?.altText ?? null, 'full')"
        class="max-w-full max-h-64 object-contain" />
    </div>
    <div v-if="resolvedImages.length > 1">
      <UCarousel
        ref="carousel"
        v-slot="{ item, index }"
        loop
        arrows
        wheel-gestures
        :prev="{ onClick: onClickPrev }"
        :next="{ onClick: onClickNext }"
        :items="resolvedImages"
        :ui="{
          item: 'basis-1/4 ps-2',
          prev: 'sm:start-8',
          next: 'sm:end-8',
          container: 'ms-0'
        }">
        <div
          class="aspect-square overflow-hidden rounded-lg cursor-pointer"
          @click="select(index)">
          <StorageImage
            :storage-key="item.thumbKey"
            :alt="formatThumbnailAltText(item.altText)"
            class="w-full h-full object-cover" />
        </div>
      </UCarousel>
    </div>
  </div>
</template>

<style scoped>

</style>
