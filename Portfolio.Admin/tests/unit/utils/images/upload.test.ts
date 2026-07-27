import { afterEach, describe, expect, it, vi } from 'vitest'
import { uploadImagesToStorage } from '~/utils/images/upload'
import type { ProjectImageUploadInstructionFragment } from '~/generated/graphql'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'

type InstructionOptions = {
  clientId?: string
  projectImageId?: string
  fullUploadUrl?: string
  thumbUploadUrl?: string
}

const createInstruction = (
  options: InstructionOptions = {}
): ProjectImageUploadInstructionFragment => {
  const {
    clientId = 'upload-client-id',
    projectImageId = 'project-image-id',
    fullUploadUrl = 'https://storage.example.com/full',
    thumbUploadUrl = 'https://storage.example.com/thumb'
  } = options

  return {
    __typename: 'ProjectImageUploadInstruction',
    clientId,
    projectImageId,
    full: {
      __typename: 'ProjectImageUploadTarget',
      contentType: 'image/webp',
      key: 'projects/project-id/images/image-id/full.webp',
      publicUrl: 'https://cdn.example.com/full.webp',
      uploadUrl: fullUploadUrl
    },
    thumb: {
      __typename: 'ProjectImageUploadTarget',
      contentType: 'image/jpeg',
      key: 'projects/project-id/images/image-id/thumb.jpg',
      publicUrl: 'https://cdn.example.com/thumb.jpg',
      uploadUrl: thumbUploadUrl
    }
  }
}

const createUploadItem = (
  overrides: Partial<ImageEditorItem> = {}
): ImageEditorItem => ({
  id: null,
  clientId: 'upload-client-id',
  isRemoved: false,
  sort: 0,
  contentType: 'image/webp',
  fileName: 'portfolio-project.webp',
  sizeThumb: 30,
  sizeFull: 120,
  altText: 'Portfolio project',
  height: 800,
  width: 1_200,
  fullFile: new Blob(
    [new Uint8Array(120)],
    { type: 'application/octet-stream' }
  ),
  thumbFile: new Blob(
    [new Uint8Array(30)],
    { type: 'application/octet-stream' }
  ),
  ...overrides
})

const successfulResponse = {
  ok: true,
  status: 200
} as Response

const failedResponse = {
  ok: false,
  status: 500
} as Response

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('uploadImagesToStorage', () => {
  it('uploads the full and thumbnail files to their instructed targets', async () => {
    const fetchMock = vi.fn().mockResolvedValue(successfulResponse)
    vi.stubGlobal('fetch', fetchMock)
    const instruction = createInstruction()
    const uploadItem = createUploadItem()

    const result = await uploadImagesToStorage(
      [instruction],
      [uploadItem]
    )

    expect(fetchMock).toHaveBeenCalledTimes(2)
    expect(fetchMock).toHaveBeenNthCalledWith(
      1,
      instruction.full.uploadUrl,
      {
        method: 'PUT',
        headers: {
          'Content-Type': instruction.full.contentType
        },
        body: uploadItem.fullFile
      }
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      2,
      instruction.thumb.uploadUrl,
      {
        method: 'PUT',
        headers: {
          'Content-Type': instruction.thumb.contentType
        },
        body: uploadItem.thumbFile
      }
    )
    expect(result).toEqual({
      succeededProjectImageIds: ['project-image-id'],
      failedProjectImageIds: []
    })
  })

  it.each([
    {
      failedUpload: 'full image',
      responses: [failedResponse, successfulResponse]
    },
    {
      failedUpload: 'thumbnail',
      responses: [successfulResponse, failedResponse]
    }
  ])(
    'returns a failure when the $failedUpload upload fails',
    async ({ responses }) => {
      const fetchMock = vi.fn()
        .mockResolvedValueOnce(responses[0])
        .mockResolvedValueOnce(responses[1])
      vi.stubGlobal('fetch', fetchMock)

      const result = await uploadImagesToStorage(
        [createInstruction()],
        [createUploadItem()]
      )

      expect(result).toEqual({
        succeededProjectImageIds: [],
        failedProjectImageIds: ['project-image-id']
      })
    }
  )

  it.each([
    {
      missingData: 'matching editor item',
      uploadItems: []
    },
    {
      missingData: 'full file',
      uploadItems: [createUploadItem({ fullFile: null })]
    },
    {
      missingData: 'thumbnail file',
      uploadItems: [createUploadItem({ thumbFile: null })]
    }
  ])(
    'returns a failure without fetching when the $missingData is missing',
    async ({ uploadItems }) => {
      const fetchMock = vi.fn()
      vi.stubGlobal('fetch', fetchMock)

      const result = await uploadImagesToStorage(
        [createInstruction()],
        uploadItems
      )

      expect(fetchMock).not.toHaveBeenCalled()
      expect(result).toEqual({
        succeededProjectImageIds: [],
        failedProjectImageIds: ['project-image-id']
      })
    }
  )

  it('continues uploading after another image fails', async () => {
    const fetchMock = vi.fn().mockImplementation(
      async (uploadUrl: string) =>
        uploadUrl.includes('failed')
          ? failedResponse
          : successfulResponse
    )
    vi.stubGlobal('fetch', fetchMock)
    const successfulInstruction = createInstruction({
      clientId: 'successful-client-id',
      projectImageId: 'successful-project-image-id',
      fullUploadUrl: 'https://storage.example.com/successful/full',
      thumbUploadUrl: 'https://storage.example.com/successful/thumb'
    })
    const failedInstruction = createInstruction({
      clientId: 'failed-client-id',
      projectImageId: 'failed-project-image-id',
      fullUploadUrl: 'https://storage.example.com/failed/full',
      thumbUploadUrl: 'https://storage.example.com/failed/thumb'
    })

    const result = await uploadImagesToStorage(
      [successfulInstruction, failedInstruction],
      [
        createUploadItem({ clientId: 'successful-client-id' }),
        createUploadItem({ clientId: 'failed-client-id' })
      ]
    )

    expect(fetchMock).toHaveBeenCalledTimes(4)
    expect(result).toEqual({
      succeededProjectImageIds: ['successful-project-image-id'],
      failedProjectImageIds: ['failed-project-image-id']
    })
  })
})
