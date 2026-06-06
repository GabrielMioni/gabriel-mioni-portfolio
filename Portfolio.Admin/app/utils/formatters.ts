import { format, parseISO } from 'date-fns'

export const formatDate = (
  value?: string | Date | null,
  fmt = 'M/d/yy h:mma',
  invalidReturn = '-'
) => {
  const date = typeof value === 'string' ? parseISO(value) : value
  if (!date) return invalidReturn
  return format(date, fmt)
}

export const formatTextWithEllipsis = (
  val: string | null,
  length: number = 100
) => {
  const text = val ?? ''
  const subString = text.substring(0, length)
  if (text.length > subString.length) return `${subString}...`

  return subString
}
