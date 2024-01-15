<script setup>
import { onMounted, watch } from 'vue'
import { useForm } from 'vee-validate'

defineProps({
  modelValue: {
    type: Boolean,
    required: false,
    default: false
  }
})

const emit = defineEmits(['update:modelValue'])

const { values, isFieldValid } = useForm()

watch(values, (formValues) => {
  const fieldNames = Object.keys(formValues)
  // nextTick doesn't work here
  setTimeout(() => {
    const allFieldsValid = fieldNames.every(field => isFieldValid(field))
    updateFormErrorState(allFieldsValid)
  })
})

const updateFormErrorState = (val) => emit('update:modelValue', val)

onMounted(() => {
  // normalize form valid state
  updateFormErrorState(false)
})

</script>

<template>
  <form>
    <slot />
  </form>
</template>

<script>
export default {
  name: 'BaseForm'
}
</script>
