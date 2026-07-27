import { describe, expect, it } from 'vitest'
import { getOutputMimeType } from '~/utils/images/processing'

describe('getOutputMimeType', () => {
  it.each([
    {
      inputType: 'image/jpeg',
      expectedType: 'image/jpeg'
    },
    {
      inputType: 'image/jpg',
      expectedType: 'image/jpeg'
    },
    {
      inputType: 'image/png',
      expectedType: 'image/png'
    },
    {
      inputType: 'image/webp',
      expectedType: 'image/webp'
    },
    {
      inputType: 'application/octet-stream',
      expectedType: 'image/webp'
    }
  ])(
    'returns $expectedType for $inputType',
    ({ inputType, expectedType }) => {
      const file = new Blob([], { type: inputType })

      expect(getOutputMimeType(file)).toBe(expectedType)
    }
  )
})
