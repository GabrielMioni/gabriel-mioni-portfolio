import { isLikelyValidHttpUrl } from '~/utils/links'

export type Rule = (v: unknown) => true | string

export const required = (msg = 'Required'): Rule => (v) =>
  (v !== null && v !== undefined && String(v).trim().length > 0) || msg

export const maxLength = (
  maximumLength: number,
  fieldLabel = 'Value'
): Rule => (v) =>
  (v === null || v === undefined || String(v).length <= maximumLength) ||
  `${fieldLabel} cannot exceed ${maximumLength} characters`

export const validateUrl = (msg = 'Must be a valid URL'): Rule => (v) => {
  if (!v || String(v).trim().length === 0) return true

  return isLikelyValidHttpUrl(String(v)) || msg
}
