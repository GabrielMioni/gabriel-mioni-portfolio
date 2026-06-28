<script setup lang="ts">
defineOptions({ inheritAttrs: false })

const config = useRuntimeConfig()

const props = defineProps<{
  alt?: string
  storageKey?: string
}>()

const src = computed(() => `${config.public.storageBase}/${props.storageKey}`)

const failed = ref(false)
const loaded = ref(false)

watch(() => props.storageKey, () => {
  failed.value = false
  loaded.value = false
})
</script>

<template>
  <img
    v-if="storageKey && !failed"
    v-bind="$attrs"
    :alt="alt ?? 'project image'"
    :src="src"
    loading="lazy"
    class="transition-opacity duration-1000"
    :class="loaded ? 'opacity-100' : 'opacity-0'"
    @load="loaded = true"
    @error="failed = true">
  <div
    v-else
    v-bind="$attrs"
    class="flex items-center justify-center w-full h-full">
    <UIcon
      name="i-lucide-image"
      class="size-12 text-stone-400" />
  </div>
</template>
