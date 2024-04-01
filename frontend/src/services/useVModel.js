import { computed } from 'vue'

const useVModel = (props, emit, modelName = 'modelValue') => {
  const localValue = computed({
    get: () => props[modelName],
    set: (value) => emit(`update:${modelName}`, value)
  })

  return { localValue }
}

export default useVModel
