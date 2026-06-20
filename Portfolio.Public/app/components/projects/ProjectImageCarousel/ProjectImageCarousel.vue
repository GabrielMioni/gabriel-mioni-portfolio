<script setup lang="ts">
import type { PublicProjectImageFragment } from '~/generated/graphql'

defineProps<{
  images: PublicProjectImageFragment[]
}>()

const carousel = useTemplateRef('carousel')
const activeIndex = ref(0)

const onClickPrev = () => {
  activeIndex.value--
}
const onClickNext = () => {
  activeIndex.value++
}
const onSelect = (index: number) => {
  activeIndex.value = index
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
  <div class="flex-1 w-full">
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
</template>

<style scoped>

</style>
