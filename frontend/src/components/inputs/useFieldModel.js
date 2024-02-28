import { computed, ref, watch } from 'vue'
import { useField } from 'vee-validate'

export const useFieldModel = (props, emit) => {
  const { errorMessage, value } = useField(props.fieldName, props.rules, { validateOnMount: false })

  const textValue = ref(value)

  const hasContent = computed(() => {
    if (!textValue.value) {
      return false
    }
    return textValue.value.toString().trim().length > 0
  })

  watch(() => props.modelValue, (newValue) => {
    if (textValue.value !== newValue) {
      textValue.value = newValue
    }
  }, { immediate: true })

  watch(textValue, (newValue) => {
    if (props.modelValue !== newValue) {
      emit('update:modelValue', newValue)
    }
  })

  return { errorMessage, textValue, hasContent }
}
