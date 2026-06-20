<script setup lang="ts">
defineOptions({ inheritAttrs: false })

const config = useRuntimeConfig()

const props = withDefaults(
  defineProps<{
    alt?: string
    storageKey?: string
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

const failed = ref(false)

watch(
  () => props.storageKey,
  () => { failed.value = false }
)
</script>

<template>
  <img
    v-if="storageKey && !failed"
    v-bind="$attrs"
    :alt="alt"
    :height="height"
    :width="width"
    :src="src"
    loading="lazy"
    class="block mx-auto object-center"
    @error="failed = true">
  <UIcon
    v-else
    name="i-lucide-image"
    class="size-12 text-stone-400" />
</template>
