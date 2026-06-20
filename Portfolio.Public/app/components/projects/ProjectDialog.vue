<script setup lang="ts">
import type { PublicProjectFragment } from '~/generated/graphql'
import { PublicProjectImageFragmentDoc, PublicProjectLinkFragmentDoc } from '~/generated/graphql'
import { useFragment } from '~/generated'
import ProjectImageCarousel from '~/components/projects/ProjectImageCarousel/ProjectImageCarousel.vue'

const open = defineModel<boolean>('open', { required: true })

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
</script>

<template>
  <UModal
    v-model:open="open"
    :ui="{ content: 'w-[90vw] max-w-full', header: 'border-0' }"
    title="">
    <template #body>
      <div class="grid md:grid-cols-2 max-h-[70vh]">
        <div class="flex flex-col gap-4 p-6 overflow-y-auto">
          <p
            v-if="project?.summary"
            class="text-sm text-stone-500 dark:text-stone-400">
            {{ project.summary }}
          </p>
          <p
            v-if="project?.body"
            class="text-sm text-stone-700 dark:text-stone-300 leading-relaxed whitespace-pre-wrap flex-1">
            {{ project.body }}
          </p>
          <div
            v-if="links.length"
            class="flex flex-wrap gap-2">
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
        <div class="bg-stone-100 dark:bg-stone-800 overflow-y-auto">
          <div class="grid grid-cols-1">
            <ProjectImageCarousel :images="images" />
          </div>
        </div>
      </div>
    </template>
  </UModal>
</template>
