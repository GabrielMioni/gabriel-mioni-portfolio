<script setup lang="ts">
import type { PublicProjectFragment } from '~/generated/graphql'
import { PublicProjectImageFragmentDoc, PublicProjectLinkFragmentDoc } from '~/generated/graphql'
import { useFragment } from '~/generated'
import ProjectImageCarousel from '~/components/projects/ProjectImageCarousel/ProjectImageCarousel.vue'

const open = defineModel<boolean>('open', { required: true })

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

const linkIcon: Record<string, string> = {
  REPOSITORY: 'i-simple-icons-github',
  DEMO: 'i-lucide-external-link',
  EXTERNAL: 'i-lucide-external-link'
}

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
            class="flex flex-col gap-4 px-6"
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
            class="flex flex-wrap gap-2 px-6 pt-4 pb-6 shrink-0">
            <UButton
              v-for="link in links"
              :key="link.id"
              :to="link.url"
              :icon="linkIcon[link.linkType] ?? 'i-lucide-link'"
              :label="link.linkText"
              target="_blank"
              size="sm"
              variant="subtle" />
          </div>
        </div>
        <div
          v-if="images.length > 0"
          :class="isMobile ? '' : 'overflow-y-auto'"
          class="order-1 md:order-2">
          <div class="grid grid-cols-1">
            <ProjectImageCarousel :images="images" />
          </div>
        </div>
      </div>
    </template>
  </UModal>
</template>
