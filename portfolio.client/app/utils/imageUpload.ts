import type {
  ProjectImageUploadRequestInput,
  ProjectImageUploadInstructionFragment
} from '~/generated/graphql'
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

const uploadFileToTarget = async ({
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

export const uploadImagesToStorage = async (
  instructions: ProjectImageUploadInstructionFragment[],
  uploadItems: ImageUploadItem[]
) => {
  const uploadItemByClientId = new Map(
    uploadItems.map(item => [item.clientId, item])
  )

  const uploads = instructions.map(async (instruction) => {
    const matchingItem = uploadItemByClientId.get(instruction.clientId)

    if (!matchingItem?.fullFile || !matchingItem.thumbFile) {
      throw new Error(`Missing files for clientId ${instruction.clientId}`)
    }

    await Promise.all([
      uploadFileToTarget({
        file: matchingItem.fullFile,
        uploadUrl: instruction.full.uploadUrl,
        contentType: instruction.full.contentType
      }),
      uploadFileToTarget({
        file: matchingItem.thumbFile,
        uploadUrl: instruction.thumb.uploadUrl,
        contentType: instruction.thumb.contentType
      })
    ])
  })

  await Promise.all(uploads)
}
