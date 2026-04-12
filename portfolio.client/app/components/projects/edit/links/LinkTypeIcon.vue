<script setup lang="ts">
import { ProjectLinkType } from '~/generated/graphql'

const props = withDefaults(
  defineProps<{
      linkType: ProjectLinkType,
      color?: string,
      size?: 'x-small' | 'small' | 'default' | 'large' | 'x-large'
    }>(),
  {
    color: 'primary',
    size: 'default'
  }
)

const icon = computed(() => {
  switch (props.linkType) {
  case ProjectLinkType.Demo:
    return 'mdi-monitor-dashboard'
  case ProjectLinkType.Repository:
    return 'mdi-source-repository'
  default:
    return 'mdi-link-variant'
  }
})

</script>

<template>
  <v-icon
    :color="color"
    :size="size">
    {{ icon }}
  </v-icon>
</template>

<style scoped>

</style>
