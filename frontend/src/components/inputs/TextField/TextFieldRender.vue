<script setup>
import { computed } from 'vue'

const props = defineProps({
  appendIcon: {
    type: String,
    required: false,
    default: null
  },
  prependIcon: {
    type: String,
    required: false,
    default: null
  },
  floatLabel: {
    type: Boolean,
    required: false,
    default: false
  },
  helpText: {
    type: String,
    required: false,
    default: null
  },
})

const iconClasses = computed( () => {
  return {
    'p-input-icon-left': props.prependIcon,
    'p-input-icon-right': props.appendIcon,
    'p-float-label': props.floatLabel
  }
})

</script>

<template>
  <div class="text-field-render">
    <span
      class="flex flex-column"
      :class="iconClasses">
      <i
        v-if="appendIcon || prependIcon"
        :class="`pi ${ prependIcon || appendIcon }`" />
      <slot
        v-if="!floatLabel"
        name="label" />
      <slot name="default" />
      <slot
        v-if="floatLabel"
        name="label" />
    </span>
    <small v-if="helpText">
      {{ helpText }}
    </small>
  </div>
</template>

<script>
export default {
  name: 'TextFieldRender'
}
</script>

<style scoped>
span {
  display: inherit;
  flex-direction: inherit;
}
</style>
