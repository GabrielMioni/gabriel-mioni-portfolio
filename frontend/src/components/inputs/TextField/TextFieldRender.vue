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
  hideDetails: {
    type: Boolean,
    required: false,
    default: false
  },
  errorMessage: {
    type: String,
    required: false,
    default: null
  }
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
    <div
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
    </div>
    <div class="flex flex-column">
      <div
        v-if="!hideDetails"
        class="details">
        <small
          v-if="errorMessage"
          class="p-error">
          {{ errorMessage }}
        </small>
      </div>
      <div
        v-if="helpText"
        class="details">
        <small>
          {{ helpText }}
        </small>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'TextFieldRender'
}
</script>
