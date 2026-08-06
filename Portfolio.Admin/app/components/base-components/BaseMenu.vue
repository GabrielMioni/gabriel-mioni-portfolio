<script setup lang="ts">
import type { MenuItem } from '~/types/ui/MenuItem'

withDefaults(defineProps<{
  items: MenuItem[]
  activatorLabel?: string
}>(), {
  activatorLabel: 'Open actions menu'
})

const onItemClick = async (item: MenuItem) => {
  if (item.action) await item.action()
}

</script>

<template>
  <v-menu>
    <template #activator="{ props: activatorProps }">
      <v-btn
        v-bind="activatorProps"
        :aria-label="activatorLabel"
        size="small"
        variant="text"
        icon="mdi-dots-vertical" />
    </template>
    <v-list class="admin-menu">
      <v-list-item
        v-for="(item, index) in items"
        :key="item.title + index"
        :append-icon="item.icon"
        :class="['admin-menu__item', item.itemClass]"
        :to="item.route"
        :disabled="item.disabled"
        @click="onItemClick(item)" >
        <v-list-item-title>{{ item.title }}</v-list-item-title>
      </v-list-item>
    </v-list>
  </v-menu>
</template>

<style scoped>
.admin-menu {
  background: rgb(var(--v-theme-paper-raised));
  border: 1px solid rgb(var(--v-theme-rule));
  border-radius: 0;
  box-shadow: 6px 6px 0 color-mix(in srgb, rgb(var(--v-theme-ink)) 18%, transparent);
  min-width: 11rem;
  padding: 0.25rem;
}

.admin-menu__item {
  border-radius: 0;
  color: rgb(var(--v-theme-ink));
}

.admin-menu__item:hover {
  background: rgb(var(--v-theme-amber));
  color: rgb(var(--v-theme-on-warning));
}
</style>
