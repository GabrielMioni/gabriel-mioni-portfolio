<script setup lang="ts">

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
    class: 'text-error',
    action: () => {
      actions.delete(props.projectId)
    }
  }
]

</script>

<template>
  <v-menu>
    <template #activator="{ props: activatorProps }">
      <v-btn
        v-bind="activatorProps"
        size="small"
        variant="flat"
        icon="mdi-dots-vertical" />
    </template>
    <v-list>
      <v-list-item
        v-for="(item, index) in menuItems"
        :key="index"
        :append-icon="item.icon"
        :class="item.class ?? undefined"
        @click="item.action()">
        <v-list-item-title>{{ item.title }}</v-list-item-title>
      </v-list-item>
    </v-list>
  </v-menu>
</template>

<style scoped>

</style>
