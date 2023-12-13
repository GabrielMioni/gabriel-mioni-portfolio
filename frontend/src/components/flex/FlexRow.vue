<template>
  <div :class="gridClasses">
    <slot />
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  noGutters: Boolean
})

const gridClasses = computed(() => ({
  grid: true,
  'grid-nogutter': props.noGutters
}))
</script>
