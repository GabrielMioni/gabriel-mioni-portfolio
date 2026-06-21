<script setup lang="ts">
import type { PublicProjectFragment } from '~/generated/graphql'
import { PublicProjectImageFragmentDoc, PublicProjectLinkFragmentDoc } from '~/generated/graphql'
import { useFragment } from '~/generated'
import ProjectImageCarousel from '~/components/projects/ProjectImageCarousel/ProjectImageCarousel.vue'
import ProjectLinks from '~/components/projects/ProjectLinks.vue'

const open = defineModel<boolean>('open', { required: true })

const textSection = useTemplateRef('text-section')

onMounted(() => {
  addEventListener('keydown', keyNavigation)
})

onUnmounted(() => {
  removeEventListener('keydown', keyNavigation)
})

const keyNavigation = (event: KeyboardEvent) => {
  const element = textSection?.value

  if (event.key === 'ArrowUp') {
    element?.scrollBy({ top: -80, behavior: 'smooth' })
  } else if (event.key === 'ArrowDown') {
    element?.scrollBy({ top: 80, behavior: 'smooth' })
  }
}

const { md: isMobile } = useMediaQuery()

const props = defineProps<{
  project?: PublicProjectFragment | null
}>()

const images = computed(() =>
  props.project?.images.map(i => useFragment(PublicProjectImageFragmentDoc, i)) ?? []
)

const links = computed(() =>
  props.project?.links.map(l => useFragment(PublicProjectLinkFragmentDoc, l)) ?? []
)

const modalUi = computed(() => ({
  content: isMobile.value ? '' : `${images.value.length > 0 ? 'w-[90vw]' : 'w-[45vw]'} h-[80vh] max-w-full`,
  header: 'border-0'
}))
</script>

<template>
  <UModal
    v-model:open="open"
    :fullscreen="isMobile"
    :ui="modalUi"
    :title="project?.title">
    <template #body>
      <div
        class="grid md:grid-cols-2 gap-6"
        :class="{ 'h-[calc(70vh-4rem)]': !isMobile }">
        <div
          class="flex flex-col order-2 md:order-1"
          :class="isMobile ? '' : 'h-full'">
          <div
            ref="text-section"
            class="flex flex-col px-6"
            :class="isMobile ? '' : 'overflow-y-auto flex-1'">
            <div
              v-if="project?.body"
              class="text-sm text-stone-700 dark:text-stone-300 leading-relaxed whitespace-pre-wrap"
              :class="!isMobile ? 'max-h-[calc(70vh-8rem)]' : ''">
              {{ project.body }}
            </div>
          </div>
          <div
            v-if="links.length"
            class="px-6 pt-4 pb-6 shrink-0">
            <ProjectLinks :links="links" />
          </div>
        </div>
        <div
          v-if="images.length > 0"
          :class="isMobile ? '' : 'overflow-y-auto'"
          class="order-1 md:order-2">
          <ProjectImageCarousel :images="images" />
        </div>
      </div>
    </template>
  </UModal>
</template>
