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

// Computed
const textValue = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

const classes = computed(() => {
  return {
    'gap-2': props.helpText
  }
})

const iconClass = computed( () => {
  if (props.prependIcon) {
    return 'p-input-icon-left'
  }
  if (props.appendIcon) {
    return 'p-input-icon-right'
  }
  return null
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
  <div
    class="flex flex-column text-field"
    :class="classes">
    <span
      v-if="iconClass"
      :class="iconClass"
      class="text-field__icon-parent">
      <i :class="`pi ${ prependIcon || appendIcon }`" />
      <input-text
        v-model="textValue"
        :size="size" />
    </span>
    <input-text
      v-else
      v-model="textValue"
      :size="size" />
    <small
      v-if="helpText"
      id="user=help">
      {{ helpText }}
    </small>
  </div>
</template>

<style lang="scss" scoped>
.text-field {
  &__icon-parent {
    display: inherit;
    flex-direction: inherit;
  }
}
</style>
