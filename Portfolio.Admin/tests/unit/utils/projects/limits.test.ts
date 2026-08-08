import { describe, expect, it } from 'vitest'
import {
  getRemainingCapacity,
  MAX_PROJECT_BODY_LENGTH,
  MAX_PROJECT_IMAGES,
  MAX_PROJECT_SUMMARY_LENGTH,
  MAX_PROJECT_TAGS,
  MAX_PROJECT_TITLE_LENGTH,
  takeItemsWithinCapacity
} from '~/utils/projects/limits'

describe('project limits', () => {
  it('matches the limits enforced by the API', () => {
    expect(MAX_PROJECT_IMAGES).toBe(6)
    expect(MAX_PROJECT_TAGS).toBe(15)
    expect(MAX_PROJECT_TITLE_LENGTH).toBe(300)
    expect(MAX_PROJECT_SUMMARY_LENGTH).toBe(500)
    expect(MAX_PROJECT_BODY_LENGTH).toBe(10_000)
  })

  it.each([
    {
      description: 'returns the unused capacity',
      currentCount: 6,
      maximumCount: 15,
      expected: 9
    },
    {
      description: 'returns zero at the limit',
      currentCount: 15,
      maximumCount: 15,
      expected: 0
    },
    {
      description: 'returns zero above the limit',
      currentCount: 16,
      maximumCount: 15,
      expected: 0
    }
  ])('$description', ({ currentCount, maximumCount, expected }) => {
    expect(getRemainingCapacity(currentCount, maximumCount)).toBe(expected)
  })

  it('returns only the items that fit without mutating the input', () => {
    const items = ['one', 'two', 'three']

    const acceptedItems = takeItemsWithinCapacity(
      items,
      MAX_PROJECT_IMAGES - 2,
      MAX_PROJECT_IMAGES
    )

    expect(acceptedItems).toEqual(['one', 'two'])
    expect(items).toEqual(['one', 'two', 'three'])
  })
})
