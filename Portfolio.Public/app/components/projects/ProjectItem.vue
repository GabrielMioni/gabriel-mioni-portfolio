<script setup lang="ts">
import type { PublicProjectFragment } from '~/generated/graphql'
import { PublicProjectImageFragmentDoc, PublicProjectLinkFragmentDoc } from '~/generated/graphql'
import { useFragment } from '~/generated'

const props = defineProps<{
  project: PublicProjectFragment
}>()

const emit = defineEmits<{
  select: [projectId: string]
}>()

const mainImage = computed(() => {
  const raw = props.project.images[0]
  return raw ? useFragment(PublicProjectImageFragmentDoc, raw) : null
})

const links = computed(() =>
  props.project.links.map(l => useFragment(PublicProjectLinkFragmentDoc, l))
)

const hasAdditionalContent = computed(() => {
  return (
    (props.project.body?.trim() || '').length > 0
    || props.project.images.length > 1)
})

const selectProject = () => {
  emit('select', props.project.id)
}
</script>

<template>
  <UCard
    :ui="{ header: 'p-0' }"
    :class="{ 'cursor-pointer group transition hover:ring-2 hover:ring-primary': hasAdditionalContent }"
    @click="selectProject">
    <template #header>
      <div class="h-[250px] content-center">
        <div class="aspect-video bg-stone-100 dark:bg-stone-800">
          <StorageImage
            :storage-key="mainImage?.thumbKey"
            :alt="mainImage?.altText ?? 'Project image'"
            :class="{ 'transition-transform duration-300 group-hover:scale-105': hasAdditionalContent }"
            class="w-full h-full object-cover" />
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
      <ProjectLinks
        :links="links"
        @click.stop />
    </template>
  </UCard>
</template>
