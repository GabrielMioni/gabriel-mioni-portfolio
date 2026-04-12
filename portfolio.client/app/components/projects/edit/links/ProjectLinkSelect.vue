<script setup lang="ts">
import { ProjectLinkType } from '~/generated/graphql'

const linkType = defineModel<ProjectLinkType>({ required: true })

function formatTitle (linkType: ProjectLinkType): string {
  switch (linkType) {
  case ProjectLinkType.Demo:
    return 'Demo'
  case ProjectLinkType.Repository:
    return 'Repository'
  default:
    return 'External'
  }
}

const selectItems = computed(() => {
  return Object.values(ProjectLinkType).map((itemType) => ({
    value: itemType,
    title: formatTitle(itemType)
  }))
})
</script>

<template>
  <v-select
    v-model="linkType"
    :items="selectItems"
    variant="filled"
    hide-details
    density="compact" />
</template>

<style scoped>

</style>
