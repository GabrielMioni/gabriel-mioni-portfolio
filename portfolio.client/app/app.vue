<script setup lang="ts">
onMounted(async () => {
  const { public: { apiBase } } = useRuntimeConfig()
  const result = await $fetch(`${apiBase}/health`)
  console.log(result)
})
</script>

<template>
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
</template>
