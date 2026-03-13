import { useMutation } from '@urql/vue'
import type { RequestProjectImageUploadsInput } from '~/generated/graphql'
import {
  PrepareProjectImageUploadsDocument,
  ProjectImageUploadInstructionFragmentDoc
} from '~/generated/graphql'
import { useFragment } from '~/generated'
import type { ImageUploadItem } from '~/types/images/ImageUploadItem'
import {
  toUploadRequestItems,
  uploadImagesToStorage
} from '~/utils/imageUpload'

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

    return useFragment(ProjectImageUploadInstructionFragmentDoc, items)
  }

  const uploadImages = async ({
    uploadItems,
    projectId
  }: {
    uploadItems: ImageUploadItem[]
    projectId: string
  }) => {
    const validUploadItems = uploadItems.filter(
      (
        item
      ): item is ImageUploadItem =>
        !!item.fullFile && !!item.thumbFile
    )

    const items = toUploadRequestItems(validUploadItems)

    if (items.length === 0) return

    const instructions = await prepareImageUploads({
      projectId,
      items
    })

    if (instructions.length !== items.length) {
      throw new Error('Upload instruction count did not match upload item count')
    }

    await uploadImagesToStorage(instructions, validUploadItems)
  }

  return {
    preparingImages,
    uploadImages
  }
}
