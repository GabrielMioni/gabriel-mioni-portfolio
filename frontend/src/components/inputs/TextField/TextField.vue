<script setup>
import { computed } from 'vue'
import TextFieldRender from '@/components/inputs/TextField/TextFieldRender.vue'
import { fieldProps } from '@/components/inputs/field-props.js'
import { useFieldModel } from '@/components/inputs/useFieldModel.js'

const props = defineProps({
  ...fieldProps,
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

const { errorMessage, textValue } = useFieldModel(props, emit)

const size = computed(() => {
  if (props.small) {
    return sizeTypes.small
  }
  if (props.large) {
    return sizeTypes.large
  }
  return sizeTypes.medium
})

const fieldNameDisplay = props.fieldName.toString().toLowerCase()

</script>

<template>
  <text-field-render
    :append-icon="appendIcon"
    :error-message="errorMessage"
    :hide-details="hideDetails"
    :prepend-icon="prependIcon"
    :float-label="floatLabel"
    :help-text="helpText"
    class="text-field">
    <template #label>
      <label
        v-if="label"
        :for="fieldNameDisplay"
        style="border-radius: 5px">
        {{ label }}
      </label>
    </template>
    <template #default>
      <input-text
        :id="fieldNameDisplay"
        v-model="textValue"
        :class="{ 'p-invalid': errorMessage }"
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
