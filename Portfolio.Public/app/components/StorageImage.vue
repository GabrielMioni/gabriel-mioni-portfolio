<script setup lang="ts">
const config = useRuntimeConfig()

const props = withDefaults(
  defineProps<{
    alt?: string
    storageKey: string
    height?: string | number
    width?: string | number
  }>(),
  {
    alt: 'project image',
    height: 'auto',
    width: 'auto'
  }
)

const src = computed(() => `${config.public.storageBase}/${props.storageKey}`)
</script>

<template>
  <img
    v-bind="$attrs"
    :alt="alt"
    :height="height"
    :width="width"
    :src="src"
    loading="lazy"
    class="block mx-auto object-center">
</template>
