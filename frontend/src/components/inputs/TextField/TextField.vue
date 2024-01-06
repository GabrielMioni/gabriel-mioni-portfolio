<script setup>
import TextFieldRender from '@/components/inputs/TextField/TextFieldRender.vue'
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
  label: {
    type: String,
    required: false,
    default: null
  },
  floatLabel: {
    type: Boolean,
    required: false,
    default: false
  },
  small: {
    type: Boolean,
    required: false,
    default: false
  },
  medium: {
    type: Boolean,
    required: false,
    default: false
  },
  large: {
    type: Boolean,
    required: false,
    default: false
  },
  appendIcon: {
    type: String,
    required: false,
    default: null
  },
  prependIcon: {
    type: String,
    required: false,
    default: null
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
  <text-field-render
    :append-icon="appendIcon"
    :prepend-icon="prependIcon"
    :float-label="floatLabel"
    :help-text="helpText"
    class="text-field">
    <template #label>
      <label
        v-if="label"
        style="border-radius: 5px">
        {{ label }}
      </label>
    </template>
    <template #default>
      <input-text
        v-model="textValue"
        :size="size" />
    </template>
  </text-field-render>
</template>

<script>
const sizeTypes = {
  small: 'small',
  normal: 'normal',
  large: 'large'
}

export default {
  name: 'TextField'
}
</script>
