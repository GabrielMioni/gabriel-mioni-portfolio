/**
 * Defines the properties for a custom field component.
 *
 * @property {String} modelValue - The value bound to the field.
 * @property {String} helpText - Text providing additional guidance about the field. Not required, can be `null`.
 * @property {Boolean} hideDetails - Whether to hide the field's details (e.g., error messages, help text). Not required, defaults to `false`.
 * @property {String} label - The label text for the field. Not required, can be `null`.
 * @property {String} fieldName - The name of the field. Required.
 * @property {Boolean} floatLabel - Determines if the label should float above the field. Not required, defaults to `false`.
 * @property {Array<Function>} rules - An array of validation rules. Not required, defaults to an array with a single rule that always returns `true`.
 */
export const fieldProps = {
  modelValue: {
    type: String,
    required: true
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
