import { describe, expect, it } from 'vitest'
import { MAX_IMAGE_ALT_TEXT_LENGTH } from '~/utils/images/limits'
import {
  MAX_LINK_TEXT_LENGTH,
  MAX_LINK_URL_LENGTH
} from '~/utils/links/limits'
import { MAX_TAG_NAME_LENGTH } from '~/utils/tags/limits'

describe('text limits', () => {
  it('matches the limits enforced by the API', () => {
    expect(MAX_IMAGE_ALT_TEXT_LENGTH).toBe(500)
    expect(MAX_LINK_URL_LENGTH).toBe(2048)
    expect(MAX_LINK_TEXT_LENGTH).toBe(300)
    expect(MAX_TAG_NAME_LENGTH).toBe(50)
  })
})
