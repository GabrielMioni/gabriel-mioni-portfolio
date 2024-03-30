<script setup>
import { computed } from 'vue'
import { fieldProps } from '@/components/inputs/field-props.js'
import { ErrorMessage, Field } from 'vee-validate'
import TextFieldLayout from '@/components/inputs/TextField/TextFieldLayout.vue'

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
  <text-field-layout
    :append-icon="appendIcon"
    :hide-details="hideDetails"
    :prepend-icon="prependIcon"
    :float-label="floatLabel"
    class="text-field">
    <template #label>
      <label
        v-if="label"
        :for="fieldNameDisplay"
        style="border-radius: 5px">
        {{ label }}
      </label>
    </template>
    <field
      v-slot="{ field, errors, meta }"
      :model-value="fieldValue"
      :name="fieldName"
      :type="type"
      :rules="rules">
      <input-text
        :id="fieldNameDisplay"
        v-model="fieldValue"
        v-bind="field"
        :class="{ 'p-invalid': errors.length > 0 && meta.touched }" />
    </field>
    <template #error>
      <small class="p-error">
        <error-message :name="fieldName" />
      </small>
    </template>
    <template #helpText>
      <small>
        {{ helpText }}
      </small>
    </template>
  </text-field-layout>
</template>

<script>
export default {
  name: 'TextField'
}
</script>

<style scoped lang="scss">

</style>
