<script setup lang="ts">
import type { PublicProjectFragment } from '~/generated/graphql'
import { PublicProjectImageFragmentDoc, PublicProjectLinkFragmentDoc } from '~/generated/graphql'
import { useFragment } from '~/generated'

const props = defineProps<{
  project: PublicProjectFragment
}>()

const mainImage = computed(() => {
  const raw = props.project.images[0]
  return raw ? useFragment(PublicProjectImageFragmentDoc, raw) : null
})

const links = computed(() =>
  props.project.links.map(l => useFragment(PublicProjectLinkFragmentDoc, l))
)

const linkIcon: Record<string, string> = {
  REPOSITORY: 'i-simple-icons-github',
  DEMO: 'i-lucide-external-link',
  EXTERNAL: 'i-lucide-external-link'
}
</script>

<template>
  <UCard
    :ui="{ header: 'p-0' }"
    class="overflow-hidden">
    <template #header>
      <div class="h-[250px] content-center">
        <div class="aspect-video overflow-hidden bg-stone-100 dark:bg-stone-800">
          <StorageImage
            v-if="mainImage"
            :storage-key="mainImage.thumbKey"
            :alt="mainImage.altText"
            class="w-full h-full object-cover" />
          <div
            v-else
            class="w-full h-full flex items-center justify-center text-stone-400">
            <UIcon
              name="i-lucide-image"
              class="size-10" />
          </div>
        </div>
      </div>
    </template>
    <div class="space-y-2">
      <p class="text-lg font-semibold text-stone-900 dark:text-stone-100">
        {{ project.title }}
      </p>
      <p
        v-if="project.summary"
        class="text-sm text-stone-500 dark:text-stone-400 line-clamp-3">
        {{ project.summary }}
      </p>
    </div>
    <template
      v-if="links.length"
      #footer>
      <div class="flex flex-wrap gap-2">
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
    </template>
  </UCard>
</template>
