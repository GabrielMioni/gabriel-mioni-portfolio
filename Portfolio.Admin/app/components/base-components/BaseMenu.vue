<script setup lang="ts">
import type { MenuItem } from '~/types/ui/MenuItem'

defineProps<{
  items: MenuItem[]
}>()

const onItemClick = async (item: MenuItem) => {
  if (item.action) await item.action()
}

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
        v-for="(item, index) in items"
        :key="item.title + index"
        :append-icon="item.icon"
        :class="item.itemClass"
        :to="item.route"
        :disabled="item.disabled"
        @click="onItemClick(item)" >
        <v-list-item-title>{{ item.title }}</v-list-item-title>
      </v-list-item>
    </v-list>
  </v-menu>
</template>

<style scoped>

</style>
