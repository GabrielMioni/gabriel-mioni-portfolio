import type {
  ProjectImagePrepareItemInput,
  ProjectImageUploadInstructionFragment
} from '~/generated/graphql'
import type { ImageUploadItem } from '~/types/images/ImageUploadItem'
import type { UploadProjectImageResults } from '~/types/images/UploadToImageResults'

export const toProjectImagePrepareItem = (
  uploadItems: ImageUploadItem[]
): ProjectImagePrepareItemInput[] => {
  return uploadItems
    .map((item): ProjectImagePrepareItemInput | null => {
      const fullFile = item.fullFile
      const thumbFile = item.thumbFile

      if (!fullFile || !thumbFile) return null

      return {
        altText: item.altText,
        clientId: item.clientId,
        fullContentType: fullFile.type,
        fullSizeBytes: fullFile.size,
        height: item.height,
        width: item.width,
        thumbContentType: thumbFile.type,
        thumbSizeBytes: thumbFile.size
      }
    })
    .filter((item): item is ProjectImagePrepareItemInput => item !== null)
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
): Promise<UploadProjectImageResults> => {
  const uploadItemByClientId = new Map(
    uploadItems.map(item => [item.clientId, item])
  )

  const results = await Promise.all(
    instructions.map(async (instruction) => {
      const matchingItem = uploadItemByClientId.get(instruction.clientId)

      if (!matchingItem?.fullFile || !matchingItem.thumbFile) {
        return {
          ok: false as const,
          projectImageId: instruction.projectImageId
        }
      }

      try {
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

        return {
          ok: true as const,
          projectImageId: instruction.projectImageId
        }
      } catch {
        return {
          ok: false as const,
          projectImageId: instruction.projectImageId
        }
      }
    })
  )

  return {
    succeededProjectImageIds: results
      .filter(result => result.ok)
      .map(result => result.projectImageId),

    failedProjectImageIds: results
      .filter(result => !result.ok)
      .map(result => result.projectImageId)
  }
}
