<script setup lang="ts">
import type { PublicProjectLinkFragment } from '~/generated/graphql'

defineProps<{
  links: PublicProjectLinkFragment[]
}>()
</script>

<template>
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
