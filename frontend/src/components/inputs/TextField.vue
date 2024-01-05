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

const shouldShowIcon = computed(() => {
  return props.appendIcon || props.prependIcon
})

const iconClass = computed( () => {
  return {
    'p-input-icon-left': props.prependIcon,
    'p-input-icon-right': props.appendIcon,
    'p-float-label': props.floatLabel
  }
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
      v-if="shouldShowIcon"
      :class="iconClass"
      class="text-field__icon-parent">
      <i :class="`pi ${ prependIcon || appendIcon }`" />
      <input-text
        :id="label"
        v-model="textValue"
        :size="size" />
      <label
        v-if="label"
        :for="label"
        style="border-radius: 5px">
        {{ label }}
      </label>
    </span>
    <span
      v-else
      class="text-field__icon-parent"
      :class="{ 'p-float-label': floatLabel }">
      <label
        v-if="label && !floatLabel"
        :for="label"
        style="border-radius: 5px">
        {{ label }}
      </label>
      <input-text
        v-model="textValue"
        :size="size" />
      <label
        v-if="label && floatLabel"
        :for="label"
        style="border-radius: 5px">
        {{ label }}
      </label>
    </span>
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
