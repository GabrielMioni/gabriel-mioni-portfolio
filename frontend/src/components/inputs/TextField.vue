<script>
const sizeTypes = {
  small: 'small',
  normal: 'normal',
  large: 'large'
}
</script>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  modelValue: {
    type: String,
    required: true
  },
  small: {
    type: Boolean,
    required: false
  },
  medium: {
    type: Boolean,
    required: false
  },
  large: {
    type: Boolean,
    required: false
  }
})

const emit = defineEmits(['update:modelValue'])

const textValue = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

const size = computed(() => {
  if (props.small) {
    return sizeTypes.small
  }
  if (props.large) {
    return sizeTypes.large
  }
  return sizeTypes.medium
})
</script>

<template>
  <input-text
    v-model="textValue"
    :size="size" />
</template>
