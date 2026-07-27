import { afterEach, describe, expect, it, vi } from 'vitest'
import { imageFragmentToEditorItem } from '~/utils/images/imageEditorItems'
import type { ProjectImageFragment } from '~/generated/graphql'

const createImageFragment = (
  overrides: Partial<ProjectImageFragment> = {}
): ProjectImageFragment => ({
  __typename: 'ProjectImage',
  id: 'image-id',
  altText: 'Portfolio project',
  fullKey: 'projects/project-id/images/image-id/full.jpg',
  thumbKey: 'projects/project-id/images/image-id/thumb.jpg',
  sortOrder: 3,
  isUploaded: true,
  contentType: 'image/jpeg',
  createdAt: '2026-07-27T12:00:00Z',
  height: 800,
  width: 1_200,
  sizeBytes: '120000',
  ...overrides
})

afterEach(() => {
  vi.restoreAllMocks()
})

describe('imageFragmentToEditorItem', () => {
  it('maps a persisted image to an editor item', () => {
    const generatedClientId = '11111111-1111-4111-8111-111111111111'
    vi.spyOn(globalThis.crypto, 'randomUUID')
      .mockReturnValue(generatedClientId)
    const imageFragment = createImageFragment()

    const editorItem = imageFragmentToEditorItem(imageFragment)

    expect(editorItem).toEqual({
      id: 'image-id',
      altText: 'Portfolio project',
      clientId: generatedClientId,
      contentType: 'image/jpeg',
      createdAt: '2026-07-27T12:00:00Z',
      height: 800,
      isRemoved: false,
      width: 1_200,
      fileName: null,
      fullKey: 'projects/project-id/images/image-id/full.jpg',
      thumbKey: 'projects/project-id/images/image-id/thumb.jpg',
      sizeThumb: 0,
      sizeFull: 120_000,
      sort: 3
    })
  })

  it('uses the intended defaults for nullable image fields', () => {
    const imageFragment = createImageFragment({
      altText: null,
      contentType: null,
      height: null,
      width: null,
      sizeBytes: null
    })

    const editorItem = imageFragmentToEditorItem(imageFragment)

    expect(editorItem).toEqual(expect.objectContaining({
      altText: '',
      contentType: 'unknown',
      height: -1,
      width: -1,
      sizeFull: -1
    }))
  })

  it('assigns a new client ID each time an image is mapped', () => {
    const imageFragment = createImageFragment()

    const firstEditorItem = imageFragmentToEditorItem(imageFragment)
    const secondEditorItem = imageFragmentToEditorItem(imageFragment)

    expect(firstEditorItem.clientId).not.toBe(secondEditorItem.clientId)
  })
})
