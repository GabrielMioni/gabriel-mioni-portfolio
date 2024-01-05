<script setup>
import { computed } from 'vue'

const props = defineProps({
  modelValue: {
    type: String,
    required: true
  }
})

const emit = defineEmits(['update:modelValue'])

const textValue = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

</script>

<template>
  <input-text
    v-model="textValue"
    size="normal" />
</template>
