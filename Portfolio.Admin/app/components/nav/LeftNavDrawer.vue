<script setup lang="ts">
const drawer = defineModel<boolean>({ default: true })

const navItems = [
  { title: 'Projects', icon: 'mdi-folder-multiple-outline', to: '/projects' },
  { title: 'Tags', icon: 'mdi-tag-multiple-outline', to: '/tags' }
]
</script>

<template>
  <v-navigation-drawer v-model="drawer">
    <v-list nav>
      <v-list-item
        v-for="item in navItems"
        :key="item.to"
        :prepend-icon="item.icon"
        :title="item.title"
        :to="item.to" />
    </v-list>
  </v-navigation-drawer>
</template>
