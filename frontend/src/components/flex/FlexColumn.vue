<template>
  <div
    class="flex-column"
    :class="classes">
    <slot />
  </div>
</template>

<script>

const colPropValidator = (val) => parseInt(val) > 0 && val <= 12

export default {
  name: 'FlexColumn',
  props: {
    cols: {
      type: [String, Number],
      required: false,
      default: null,
      validator: colPropValidator
    },
    sm: {
      type: [String, Number],
      required: false,
      default: null,
      validator: colPropValidator
    },
    md: {
      type: [String, Number],
      required: false,
      default: null,
      validator: colPropValidator
    },
    lg: {
      type: [String, Number],
      required: false,
      default: null,
      validator: colPropValidator
    },
    xl: {
      type: [String, Number],
      required: false,
      default: null,
      validator: colPropValidator
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
