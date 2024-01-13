<script setup>
import TextFieldRender from '@/components/inputs/TextField/TextFieldRender.vue'
import { computed } from 'vue'
import { useField } from 'vee-validate'

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
  hideDetails: {
    type: Boolean,
    required: false,
    default: false
  },
  label: {
    type: String,
    required: false,
    default: null
  },
  fieldName: {
    type: String,
    required: true
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
  },
  rules: {
    type: Array,
    required: false,
    default: () => [ () => true ]
  }
})

const emit = defineEmits(['update:modelValue'])

const { value: fieldValue, errorMessage } = useField(props.fieldName, props.rules)

const textValue = computed({
  get: () => fieldValue.value,
  set: (val) => {
    fieldValue.value = val
    emit('update:modelValue', val)
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
