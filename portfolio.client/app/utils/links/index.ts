export * from './linkEditorItems'

const normalizeUrl = (value: string): string => {
  const trimmed = value.trim()

  if (!trimmed) return trimmed

  // If no protocol, assume https
  if (!/^https?:\/\//i.test(trimmed)) {
    return `https://${trimmed}`
  }

  return trimmed
}

const hasLikelyRealDomain = (hostname: string): boolean => {
  const parts = hostname.split('.')
  if (parts.length < 2) return false

  const tld = parts.at(-1)
  return !!tld && tld.length >= 2
}

export const isLikelyValidHttpUrl = (value: string): boolean => {
  try {
    const normalized = normalizeUrl(value)
    const url = new URL(normalized)

    if (url.protocol !== 'http:' && url.protocol !== 'https:') {
      return false
    }

    return hasLikelyRealDomain(url.hostname)
  } catch {
    return false
  }
}
