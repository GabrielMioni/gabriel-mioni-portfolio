<script setup>
import { ErrorMessage, Field } from 'vee-validate'
import { fieldProps } from '@/components/inputs/field-props.js'
import useVModel from '@/services/useVModel.js'

const props = defineProps({
  ...fieldProps
})

const emit = defineEmits(['update:modelValue'])

const { localValue: fieldValue } = useVModel(props, emit)


</script>

<template>
  <div class="text-area flex flex-column">
    <label v-if="!floatLabel && label">
      {{ props.label }}
    </label>
    <Field
      v-slot="{ field, errors, meta }"
      :model-value="fieldValue"
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

<script>
export default {
  name: 'TextArea'
}
</script>

<style scoped lang="scss">

</style>
