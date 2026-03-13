import { useMutation } from '@urql/vue'
import type { RequestProjectImageUploadsInput } from '~/generated/graphql'
import { PrepareProjectImageUploadsDocument } from '~/generated/graphql'
import type { ImageUploadItem } from '~/types/images/ImageUploadItem'
import { toUploadRequestItems, uploadFileToTarget } from '~/utils/imageUpload'

export const useProjectImageMutations = () => {
  const {
    executeMutation: prepareImagesUploadMutation,
    fetching: preparingImages
  } = useMutation(PrepareProjectImageUploadsDocument)

  const prepareImageUploads = async (input: RequestProjectImageUploadsInput) => {
    const response = await prepareImagesUploadMutation({ input })

    if (response.error) throw response.error

    const items = response.data?.prepareProjectImageUploads.items

    if (!items?.length) {
      throw new Error('No upload instructions returned')
    }

    return items
  }

  const uploadImages = async ({
    uploadItems,
    projectId
  }: {
    uploadItems: ImageUploadItem[]
    projectId: string
  }) => {
    const items = toUploadRequestItems(uploadItems)

    if (items.length === 0) return

    const instructions = await prepareImageUploads({ projectId, items })

    if (instructions.length !== items.length) {
      throw new Error('Upload instruction count did not match upload item count')
    }

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

  return {
    preparingImages,
    uploadImages
  }
}
