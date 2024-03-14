<script setup>
import { computed } from 'vue'
import { ErrorMessage, Field } from 'vee-validate'

const props = defineProps({
  modelValue: {
    type: String,
    required: true
  },
  fieldName: {
    type: String,
    required: true
  },
  type: {
    type: String,
    required: false,
    default: 'text'
  },
  rules: {
    type: Array,
    required: false,
    default: () => []
  }
})

const emit = defineEmits(['update:modelValue'])

const fieldValue = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

</script>

<template>
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
  <ErrorMessage :name="fieldName" />
</template>

<style scoped lang="scss">

</style>
