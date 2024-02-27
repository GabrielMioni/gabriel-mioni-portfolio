<script setup>
import { fieldProps } from '@/components/inputs/field-props.js'
import { useFieldModel } from '@/components/inputs/useFieldModel.js'

const props = defineProps({
  ...fieldProps,
  rows: {
    type: [String, Number],
    required: false,
    default: 5
  }
})

const emit = defineEmits(['update:modelValue'])

const { errorMessage, textValue, hasContent } = useFieldModel(props, emit)

</script>

<template>
  <span :class="{ 'p-float-label' : floatLabel }">
    <label v-if="!floatLabel && label">
      {{ props.label }}
    </label>
    <textarea
      v-model="textValue"
      class="p-inputtextarea p-inputtext p-component text-area__input"
      :class="{
        'p-filled': hasContent,
        'p-invalid': errorMessage
      }"
      :rows="rows" />
    <label v-if="floatLabel && label">
      {{ props.label }}
    </label>
  </span>
  <div
    v-if="!hideDetails"
    class="flex flex-column">
    <div
      v-if="errorMessage"
      class="details">
      <small
        class="p-error">
        {{ errorMessage }}
      </small>
    </div>
    <div
      v-if="helpText"
      class="details">
      <small>
        {{ helpText }}
      </small>
    </div>
  </div>
</template>

<script>
export default {
  name: 'TextArea'
}
</script>

<style lang="scss" scoped>
.text-area__input {
  width: 100%
}
</style>
