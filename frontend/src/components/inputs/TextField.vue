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
  helpText: {
    type: String,
    required: false,
    default: null
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
  <div class="flex flex-column gap-2">
    <input-text
      v-model="textValue"
      :size="size" />
    <small
      v-if="helpText"
      id="user=help">
      {{ helpText }}
    </small>
  </div>
</template>
