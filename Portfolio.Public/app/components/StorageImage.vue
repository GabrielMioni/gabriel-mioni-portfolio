<script setup lang="ts">
const config = useRuntimeConfig()

const props = defineProps<{
  alt?: string | null
  storageKey: string
}>()

const src = computed(() => `${config.public.storageBase}/${props.storageKey}`)
</script>

<template>
  <img
    :src="src"
    :alt="alt ?? ''"
    loading="lazy"
    class="block mx-auto object-center"
    v-bind="$attrs">
</template>
