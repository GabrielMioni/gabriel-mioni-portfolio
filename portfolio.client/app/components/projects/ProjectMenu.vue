<script setup lang="ts">

import BaseMenu from '~/components/base-components/BaseMenu.vue'

const props = defineProps<{
  projectId: string
}>()

const actions = inject<{ edit: (id: string) => void; delete: (id: string) => void }>('projectActions')
if (!actions) throw new Error('projectActions not provided')


const menuItems = [
  {
    title: 'Edit',
    icon: 'mdi-pencil',
    action: () => {
      actions.edit(props.projectId)
    }
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
  <BaseMenu :items="menuItems" />
</template>

<style scoped>

</style>
