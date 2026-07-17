<script setup lang="ts">
import { PublicProjectLinkFragmentDoc } from '~/generated/graphql'
import { useFragment, type FragmentType } from '~/generated'

const props = defineProps<{
  links: FragmentType<typeof PublicProjectLinkFragmentDoc>[]
}>()

const resolvedLinks = computed(() =>
  props.links.map(l => useFragment(PublicProjectLinkFragmentDoc, l))
)
</script>

<template>
  <div class="flex flex-wrap gap-2">
    <UButton
      v-for="link in resolvedLinks"
      :key="link.id"
      :to="link.url"
      :icon="linkIcon[link.linkType] ?? 'i-lucide-link'"
      :label="link.linkText"
      target="_blank"
      size="sm"
      variant="subtle" />
  </div>
</template>
