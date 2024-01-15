<script setup>
import { fieldProps } from '@/components/inputs/field-props.js'
import { useField } from 'vee-validate'
import { computed } from 'vue'

const props = defineProps({
  ...fieldProps,
  rows: {
    type: [String, Number],
    required: false,
    default: 5
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

</script>

<template>
  <textarea
    v-model="textValue"
    class="text-area p-inputtext"
    :rows="rows" />
  <span>{{ errorMessage }}</span>
</template>

<script>
export default {
  name: 'TextArea'
}
</script>

<style lang="scss" scoped>
.text-area {
  width: 100%
}
</style>
