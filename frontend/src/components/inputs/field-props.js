export const fieldProps = {
  modelValue: {
    type: String,
    required: false,
    default: ''
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
  label: {
    type: String,
    required: false,
    default: null
  },
  fieldName: {
    type: String,
    required: true
  },
  floatLabel: {
    type: Boolean,
    required: false,
    default: false
  },
  rules: {
    type: Array,
    required: false,
    default: () => [ () => true ]
  }
}
