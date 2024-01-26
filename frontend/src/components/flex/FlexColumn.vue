<script setup>
import { computed } from 'vue'
import { propValidator } from '@/components/flex/propValidator.js'

const props = defineProps({
  cols: {
    type: [String, Number],
    required: false,
    default: null,
    validator: (val) => propValidator(val, 'cols')
  },
  sm: {
    type: [String, Number],
    required: false,
    default: null,
    validator: (val) => propValidator(val, 'sm')
  },
  md: {
    type: [String, Number],
    required: false,
    default: null,
    validator: (val) => propValidator(val, 'md')
  },
  lg: {
    type: [String, Number],
    required: false,
    default: null,
    validator: (val) => propValidator(val, 'lg')
  },
  xl: {
    type: [String, Number],
    required: false,
    default: null,
    validator: (val) => propValidator(val, 'xl')
  }
})

const setColumnValue = (val) => {
  if (!val) {
    return null
  }
  const columnValue = parseInt(val)

  if (columnValue > 12) {
    return 12
  }
  if (columnValue < 1) {
    return 1
  }
  return columnValue
}

const colsVal = computed(() => {
  return setColumnValue(props.cols)
})

const smVal = computed(() => {
  return setColumnValue(props.sm)
})

const mdVal = computed(() => {
  return setColumnValue(props.md)
})

const lgVal = computed(() => {
  return setColumnValue(props.lg)
})

const xlVal = computed(() => {
  return setColumnValue(props.xl)
})

const classes = computed(() => {
  return {
    [`col-${colsVal.value}`]: colsVal.value,
    [`sm:col-${smVal.value}`]: smVal.value,
    [`md:col-${mdVal.value}`]: mdVal.value,
    [`lg:col-${lgVal.value}`]: lgVal.value,
    [`xl:col-${xlVal.value}`]: xlVal.value,
  }
})


</script>

<template>
  <div
    class="flex-column col"
    :class="classes">
    <slot />
  </div>
</template>

<script>

export default {
  name: 'FlexColumn'
}
</script>

<style scoped>

</style>
