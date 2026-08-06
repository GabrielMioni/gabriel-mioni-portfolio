<script setup lang="ts">

import BaseMenu from '~/components/base-components/BaseMenu.vue'

const props = defineProps<{
  projectId: string
  projectTitle: string
}>()

const actions = inject<{ delete: (id: string) => void }>('projectActions')
if (!actions) throw new Error('projectActions not provided')


const menuItems = [
  {
    title: 'Open Editor',
    icon: 'mdi-open-in-app',
    route: `/projects/${props.projectId}/`
  },
  {
    title: 'Delete',
    icon: 'mdi-delete',
    itemClass: 'text-error',
    action: () => {
      actions.delete(props.projectId)
    }
  }
]

</script>

<template>
  <BaseMenu
    :items="menuItems"
    :activator-label="`Actions for ${projectTitle}`" />
</template>

<style scoped>

</style>
