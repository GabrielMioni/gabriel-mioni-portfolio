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

const { errors } = useForm()

watch(errors, (val) => {
  const isValid = Object.keys(val).length <= 0
  updateValue(isValid)
})

const updateValue = (val) => emit('update:modelValue', val)

onMounted(() => {
  // normalize form valid state
  updateValue(false)
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
