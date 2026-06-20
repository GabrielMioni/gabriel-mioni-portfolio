<script setup lang="ts">
import type { PublicProjectImageFragment } from '~/generated/graphql'

const props = defineProps<{
  images: PublicProjectImageFragment[]
}>()

const carousel = useTemplateRef('carousel')
const activeIndex = ref(0)

const selectedImage = computed(() => {
  const image = props.images[activeIndex.value]
  return image ?? null
})

const onClickPrev = () => {
  if (activeIndex.value <= 0) {
    activeIndex.value = props.images.length - 1
    return
  }
  activeIndex.value--
}
const onClickNext = () => {
  if (activeIndex.value >= props.images.length - 1) {
    activeIndex.value = 0
    return
  }
  activeIndex.value++
}

const select = (index: number) => {
  activeIndex.value = index

  carousel.value?.emblaApi?.scrollTo(index)
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
    <div class="h-80 flex items-center justify-center bg-stone-100 dark:bg-stone-800 rounded-lg overflow-hidden mb-4">
      <StorageImage
        :storage-key="selectedImage?.fullKey"
        :alt="formatThumbnailAltText(selectedImage?.altText ?? null, 'full')"
        class="max-w-full max-h-70 object-contain" />
    </div>
    <div>
      <UCarousel
        ref="carousel"
        v-slot="{ item, index }"
        loop
        arrows
        wheel-gestures
        :prev="{ onClick: onClickPrev }"
        :next="{ onClick: onClickNext }"
        :items="images"
        :ui="{
          item: 'basis-1/3 ps-0',
          prev: 'sm:start-8',
          next: 'sm:end-8',
          container: 'ms-0'
        }">
        <StorageImage
          :storage-key="item.thumbKey"
          :alt="formatThumbnailAltText(item.altText)"
          class="rounded-lg"
          @click="select(index)" />
      </UCarousel>
    </div>
  </div>
</template>

<style scoped>

</style>
