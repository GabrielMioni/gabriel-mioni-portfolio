import type { ProjectImageUploadRequestInput } from '~/generated/graphql'
import type { ImageUploadItem } from '~/types/images/ImageUploadItem'

export const toUploadRequestItems = (
  uploadItems: ImageUploadItem[]
): ProjectImageUploadRequestInput[] => {
  return uploadItems
    .map((item): ProjectImageUploadRequestInput | null => {
      const fullFile = item.fullFile
      const thumbFile = item.thumbFile

      if (!fullFile || !thumbFile) return null

      return {
        altText: item.altText,
        clientId: item.clientId,
        fullContentType: fullFile.type,
        fullSizeBytes: fullFile.size,
        thumbContentType: thumbFile.type,
        thumbSizeBytes: thumbFile.size
      }
    })
    .filter((item): item is ProjectImageUploadRequestInput => item !== null)
}

export const uploadFileToTarget = async ({
  file,
  uploadUrl,
  contentType
}: {
  file: Blob
  uploadUrl: string
  contentType: string
}) => {
  const response = await fetch(uploadUrl, {
    method: 'PUT',
    headers: {
      'Content-Type': contentType
    },
    body: file
  })

  if (!response.ok) {
    throw new Error(`Upload failed with status ${response.status}`)
  }
}
