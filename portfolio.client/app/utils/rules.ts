import { normalizeUrl } from '~/utils/links'

export type Rule = (v: unknown) => true | string

export const required = (msg = 'Required'): Rule => (v) =>
  (v !== null && v !== undefined && String(v).trim().length > 0) || msg

const isLikelyRealDomain = (hostname: string): boolean => {
  if (!hostname.includes('.')) return false

  const parts = hostname.split('.')
  const tld = parts[parts.length - 1]

  return (tld ?? []).length >= 2
}

export const validateUrl = (msg = 'Must be a valid URL'): Rule => (v) => {
  if (!v || String(v).trim().length === 0) return true

  try {
    const normalized = normalizeUrl(String(v))
    const url = new URL(normalized)

    if (url.protocol !== 'http:' && url.protocol !== 'https:') {
      return msg
    }

    return isLikelyRealDomain(url.hostname) || msg
  } catch {
    return msg
  }
}
