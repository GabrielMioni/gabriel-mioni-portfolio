import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  imageEditorItemsToProjectImagePrepareItemInput,
  imageFragmentToEditorItem
} from '~/utils/images/imageEditorItems'
import type { ProjectImageFragment } from '~/generated/graphql'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'

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

const createUploadEditorItem = (
  overrides: Partial<ImageEditorItem> = {}
): ImageEditorItem => ({
  id: null,
  clientId: 'upload-client-id',
  isRemoved: false,
  sort: 0,
  contentType: 'image/webp',
  fileName: 'portfolio-project.webp',
  sizeThumb: 999,
  sizeFull: 999,
  altText: 'Portfolio project',
  height: 800,
  width: 1_200,
  fullFile: new Blob(
    [new Uint8Array(120)],
    { type: 'image/webp' }
  ),
  thumbFile: new Blob(
    [new Uint8Array(30)],
    { type: 'image/jpeg' }
  ),
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

describe('imageEditorItemsToProjectImagePrepareItemInput', () => {
  it('maps image and file metadata to a preparation input', () => {
    const uploadItem = createUploadEditorItem()

    const inputs = imageEditorItemsToProjectImagePrepareItemInput([
      uploadItem
    ])

    expect(inputs).toEqual([
      {
        altText: 'Portfolio project',
        clientId: 'upload-client-id',
        fullContentType: 'image/webp',
        fullSizeBytes: 120,
        height: 800,
        width: 1_200,
        thumbContentType: 'image/jpeg',
        thumbSizeBytes: 30
      }
    ])
  })

  it.each([
    {
      missingFile: 'full file',
      overrides: { fullFile: null }
    },
    {
      missingFile: 'thumbnail file',
      overrides: { thumbFile: null }
    }
  ])('excludes an item missing its $missingFile', ({ overrides }) => {
    const uploadItem = createUploadEditorItem(overrides)

    const inputs = imageEditorItemsToProjectImagePrepareItemInput([
      uploadItem
    ])

    expect(inputs).toEqual([])
  })
})
