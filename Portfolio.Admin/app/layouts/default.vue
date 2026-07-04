<script setup lang="ts">
const drawer = ref(true)
</script>

<template>
  <v-layout>
    <LeftNavDrawer v-model="drawer" />
    <v-app-bar
      flat
      border="b">
      <v-app-bar-nav-icon @click="drawer = !drawer" />
    </v-app-bar>
    <v-main>
      <slot />
      <GlobalSnackbar />
    </v-main>
  </v-layout>
</template>
