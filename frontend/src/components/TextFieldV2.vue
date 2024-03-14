<script setup>
import { computed } from 'vue'
import { fieldProps } from '@/components/inputs/field-props.js'
import { ErrorMessage, Field } from 'vee-validate'
import TextFieldRender from '@/components/inputs/TextField/TextFieldRender.vue'

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
  },
  type: {
    type: String,
    required: false,
    default: 'text'
  }
})

const emit = defineEmits(['update:modelValue'])

const fieldValue = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

const fieldNameDisplay = props.fieldName.toString().toLowerCase()

</script>

<template>
  <text-field-render
    :append-icon="appendIcon"
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
    <Field
      v-slot="{ field, errors, meta }"
      :name="fieldName"
      :type="type"
      :rules="rules">
      <input-text
        v-model="fieldValue"
        v-bind="field"
        :class="{ 'p-invalid': errors.length > 0 && meta.touched }" />
    </Field>
    <template #error>
      <small class="p-error">
        <ErrorMessage :name="fieldName" />
      </small>
    </template>
  </text-field-render>
</template>

<style scoped lang="scss">

</style>
