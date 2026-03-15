import { useMutation } from '@urql/vue'
import type {
  DeleteProjectImagesInput,
  FinalizeProjectImageUploadsInput,
  PrepareProjectImageUploadsInput
} from '~/generated/graphql'
import {
  DeleteProjectImagesDocument,
  FinalizeProjectImageUploadsDocument,
  PrepareProjectImageUploadsDocument,
  ProjectImageUploadInstructionFragmentDoc
} from '~/generated/graphql'
import { useFragment } from '~/generated'
import type { ImageUploadItem } from '~/types/images/ImageUploadItem'
import {
  toProjectImagePrepareItem,
  uploadImagesToStorage
} from '~/utils/imageUpload'

export const useProjectImageMutations = () => {
  const {
    executeMutation: prepareImagesUploadMutation,
    fetching: preparingImages
  } = useMutation(PrepareProjectImageUploadsDocument)

  const {
    executeMutation: finalizeImagesUploadMutation,
    fetching: finalizingImages
  } = useMutation(FinalizeProjectImageUploadsDocument)

  const {
    executeMutation: deleteImagesUploadMutation,
    fetching: deletingImages
  } = useMutation(DeleteProjectImagesDocument)

  const isProcessingImages = computed(() =>
    preparingImages.value ||
    finalizingImages.value ||
    deletingImages.value
  )

  const prepareImageUploads = async (input: PrepareProjectImageUploadsInput) => {
    const response = await prepareImagesUploadMutation({ input })

    if (response.error) throw response.error

    const items = response.data?.prepareProjectImageUploads.items

    if (!items?.length) {
      throw new Error('No upload instructions returned')
    }

    return useFragment(ProjectImageUploadInstructionFragmentDoc, items)
  }

  const finalizeImageUploads = async (input: FinalizeProjectImageUploadsInput) => {
    const response = await finalizeImagesUploadMutation({ input })

    if (response.error) throw response.error

    return response.data?.finalizeProjectImageUploads.project ?? null
  }

  const deleteImageUploads = async (input: DeleteProjectImagesInput) => {
    const response = await deleteImagesUploadMutation({ input })

    if (response.error) throw response.error

    return response.data?.deleteProjectImages.project ?? null
  }

  const uploadImages = async ({
    uploadItems,
    projectId
  }: {
    uploadItems: ImageUploadItem[]
    projectId: string
  }) => {
    const validUploadItems = uploadItems.filter(
      (item): item is ImageUploadItem =>
        !!item.fullFile && !!item.thumbFile
    )

    const items = toProjectImagePrepareItem(validUploadItems)

    if (items.length === 0) {
      return {
        project: null,
        succeededProjectImageIds: [],
        failedProjectImageIds: []
      }
    }

    const instructions = await prepareImageUploads({
      projectId,
      items
    })

    if (instructions.length !== items.length) {
      throw new Error('Upload instruction count did not match upload item count')
    }

    const {
      succeededProjectImageIds,
      failedProjectImageIds
    } = await uploadImagesToStorage(instructions, validUploadItems)

    let project = null

    if (succeededProjectImageIds.length > 0) {
      project = await finalizeImageUploads({
        projectId,
        projectImageIds: succeededProjectImageIds
      })
    }

    if (failedProjectImageIds.length > 0) {
      project = await deleteImageUploads({
        projectId,
        projectImageIds: failedProjectImageIds
      })
    }

    return {
      project,
      succeededProjectImageIds,
      failedProjectImageIds
    }
  }

  return {
    isProcessingImages,
    uploadImages
  }
}
