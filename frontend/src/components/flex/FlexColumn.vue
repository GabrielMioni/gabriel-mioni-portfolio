<template>
  <div
    class="flex-column col"
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
    colsVal () {
      return this.setColumnValue(this.cols)
    },
    smVal () {
      return this.setColumnValue(this.sm)
    },
    mdVal () {
      return this.setColumnValue(this.md)
    },
    lgVal () {
      return this.setColumnValue(this.lg)
    },
    xlVal () {
      return this.setColumnValue(this.xl)
    },
    classes () {
      return {
        [`col-${this.colsVal}`]: this.colsVal,
        [`sm:col-${this.smVal}`]: this.smVal,
        [`md:col-${this.mdVal}`]: this.mdVal,
        [`lg:col-${this.lgVal}`]: this.lgVal,
        [`xl:col-${this.xlVal}`]: this.xlVal,
      }
    },
  },
  methods: {
    setColumnValue (val) {
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
  }
}
</script>

<style scoped>

</style>
