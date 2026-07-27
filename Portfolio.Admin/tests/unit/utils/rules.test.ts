import { describe, expect, it } from 'vitest'
import { required, validateUrl } from '~/utils/rules'

describe('required', () => {
  it.each([
    {
      description: 'null',
      value: null
    },
    {
      description: 'undefined',
      value: undefined
    },
    {
      description: 'an empty string',
      value: ''
    },
    {
      description: 'a whitespace-only string',
      value: '   '
    }
  ])('returns the validation message for $description', ({ value }) => {
    expect(required()(value)).toBe('Required')
  })

  it.each([
    {
      description: 'a non-empty string',
      value: 'Portfolio project'
    },
    {
      description: 'a number',
      value: 42
    }
  ])('returns true for $description', ({ value }) => {
    expect(required()(value)).toBe(true)
  })

  it('returns the custom validation message', () => {
    expect(required('Enter a title')('')).toBe('Enter a title')
  })
})

describe('validateUrl', () => {
  it.each([
    {
      description: 'an empty string',
      value: ''
    },
    {
      description: 'a whitespace-only string',
      value: '   '
    }
  ])('returns true for optional $description', ({ value }) => {
    expect(validateUrl()(value)).toBe(true)
  })

  it('returns true for a valid URL', () => {
    expect(validateUrl()('example.com')).toBe(true)
  })

  it('returns the validation message for an invalid URL', () => {
    expect(validateUrl()('localhost')).toBe('Must be a valid URL')
  })

  it('returns the custom validation message', () => {
    expect(validateUrl('Enter a valid project URL')('localhost'))
      .toBe('Enter a valid project URL')
  })
})
