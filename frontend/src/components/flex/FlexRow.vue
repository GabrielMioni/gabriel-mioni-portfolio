<template>
  <div
    class="flex-row grid"
    :class="gridClasses">
    <slot />
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  noGutters: {
    type: Boolean,
    required: false,
    default: false
  },
  justifyCenter: {
    type: Boolean,
    required: false,
    default: false
  }
})

const gridClasses = computed(() => ({
  'grid-nogutter': props.noGutters,
  'justify-content-center': props.justifyCenter
}))
</script>

<script>
export default {
  name: 'FlexRow'
}
</script>
