import { describe, expect, it } from 'vitest'
import { isLikelyValidHttpUrl } from '~/utils/links/'

describe('isLikelyValidHttpUrl', () => {
  it.each([
    {
      description: 'an HTTPS URL',
      value: 'https://example.com'
    },
    {
      description: 'an HTTP URL',
      value: 'http://example.com'
    },
    {
      description: 'a domain without a protocol',
      value: 'example.com'
    },
    {
      description: 'a URL surrounded by whitespace',
      value: '  https://example.com/projects  '
    }
  ])('returns true for $description', ({ value }) => {
    expect(isLikelyValidHttpUrl(value)).toBe(true)
  })

  it.each([
    {
      description: 'an unsupported protocol',
      value: 'ftp://example.com'
    },
    {
      description: 'a hostname without a domain suffix',
      value: 'http://localhost'
    },
    {
      description: 'a malformed URL',
      value: 'not a valid URL'
    },
    {
      description: 'an empty value',
      value: ''
    }
  ])('returns false for $description', ({ value }) => {
    expect(isLikelyValidHttpUrl(value)).toBe(false)
  })
})
