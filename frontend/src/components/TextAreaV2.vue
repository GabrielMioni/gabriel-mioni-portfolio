<script setup>
import { ErrorMessage, Field } from 'vee-validate'
import { fieldProps } from '@/components/inputs/field-props.js'
import { computed } from 'vue'

const props = defineProps({
  ...fieldProps
})

const emit = defineEmits(['update:modelValue'])

const fieldValue = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

// const fieldNameDisplay = props.fieldName.toString().toLowerCase()


</script>

<template>
  <div class="text-area flex flex-column">
    <label v-if="!floatLabel && label">
      {{ props.label }}
    </label>
    <Field
      v-slot="{ field, errors, meta }"
      :name="fieldName"
      :rules="rules">
      <div class="p-float-label">
        <label v-if="!floatLabel && label">
          {{ props.label }}
        </label>
        <Textarea
          v-model="fieldValue"
          v-bind="field"
          class="w-full"
          :class="{
            'p-invalid': errors.length && meta.touched
          }" />
        <label v-if="floatLabel && label">
          {{ props.label }}
        </label>
      </div>
    </Field>
    <small class="p-error">
      <ErrorMessage :name="fieldName" />
    </small>
  </div>
</template>

<style scoped lang="scss">

</style>
