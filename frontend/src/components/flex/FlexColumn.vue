<template>
  <div
    class="flex-column"
    :class="classes">
    <slot />
  </div>
</template>

<script>

const propValidator = (propName) => (val) => {
  const isValid = parseInt(val) > 0 && val <= 12
  if (!isValid) {
    console.warn(`Invalid prop value for '${propName}' in FlexColumn. Values must be between 1-12.`)
  }
  return isValid
}

export default {
  name: 'FlexColumn',
  props: {
    cols: {
      type: [String, Number],
      required: false,
      default: null,
      validator: propValidator('col')
    },
    sm: {
      type: [String, Number],
      required: false,
      default: null,
      validator: propValidator('sm')
    },
    md: {
      type: [String, Number],
      required: false,
      default: null,
      validator: propValidator('md')
    },
    lg: {
      type: [String, Number],
      required: false,
      default: null,
      validator: propValidator('lg')
    },
    xl: {
      type: [String, Number],
      required: false,
      default: null,
      validator: propValidator('xl')
    }
  },
  computed: {
    classes () {
      return {
        'col': !this.cols,
        [`col-${this.cols}`]: this.cols,
        [`sm:col-${this.sm}`]: this.sm,
        [`md:col-${this.md}`]: this.md,
        [`lg:col-${this.lg}`]: this.lg,
        [`xl:col-${this.xl}`]: this.xl,
      }
    }
  }
}
</script>

<style scoped>

</style>
