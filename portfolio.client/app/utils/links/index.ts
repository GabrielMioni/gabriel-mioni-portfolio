export const normalizeUrl = (value: string): string => {
  const trimmed = value.trim()

  if (!trimmed) return trimmed

  // If no protocol, assume https
  if (!/^https?:\/\//i.test(trimmed)) {
    return `https://${trimmed}`
  }

  return trimmed
}
