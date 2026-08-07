<script setup lang="ts">
const showDevViewportIndicator = import.meta.dev

useHead({
  link: [
    { rel: 'icon', type: 'image/svg+xml', href: '/owl-icon.svg' }
  ]
})

onMounted(async () => {
  const { public: { apiBase } } = useRuntimeConfig()
  const result = await $fetch(`${apiBase}/health`)
  console.log(result)
})
</script>

<template>
  <v-app>
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
    <DevViewportIndicator v-if="showDevViewportIndicator" />
  </v-app>
</template>
