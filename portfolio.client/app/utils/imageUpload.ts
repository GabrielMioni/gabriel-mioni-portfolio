import type { ProjectImageUploadInstructionFragment } from '~/generated/graphql'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import type { UploadProjectImageResults } from '~/types/images/UploadToImageResults'

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
  uploadItems: ImageEditorItem[]
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

