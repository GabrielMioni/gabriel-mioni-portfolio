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
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import {
  imageEditorItemsToProjectImagePrepareItemInput,
  uploadImagesToStorage
} from '~/utils/images/'

type UploadImageEditorItem = ImageEditorItem & {
  fullFile: Blob
  thumbFile: Blob
}

type UploadedProjectImage = {
  clientId: string
  projectImageId: string
}

type UploadImagesResult = {
  failedClientIds: string[]
  succeededItems: UploadedProjectImage[]
}

export const useProjectImageMutations = () => {
  const {
    executeMutation: prepareImagesUploadMutation,
    fetching: preparingImages
  } = useApiMutation(
    PrepareProjectImageUploadsDocument,
    data => data.prepareProjectImageUploads,
    'Failed to prepare image uploads.'
  )

  const {
    executeMutation: finalizeImagesUploadMutation,
    fetching: finalizingImages
  } = useApiMutation(
    FinalizeProjectImageUploadsDocument,
    data => data.finalizeProjectImageUploads,
    'Failed to finalize image uploads.'
  )

  const {
    executeMutation: deleteProjectImagesMutation,
    fetching: deletingImages
  } = useApiMutation(
    DeleteProjectImagesDocument,
    data => data.deleteProjectImages,
    'Failed to delete project images.'
  )

  const isProcessingImages = computed(() =>
    preparingImages.value ||
    finalizingImages.value ||
    deletingImages.value
  )

  const prepareImageUploads = async (input: PrepareProjectImageUploadsInput) => {
    const payload = await prepareImagesUploadMutation({ input })
    const items = payload.items

    if (!items?.length) {
      throw new Error('No upload instructions returned.')
    }

    return useFragment(ProjectImageUploadInstructionFragmentDoc, items)
  }

  const finalizeImageUploads = async (
    input: FinalizeProjectImageUploadsInput
  ) => {
    const payload = await finalizeImagesUploadMutation({ input })
    const project = payload.project

    if (!project) {
      throw new Error('Image uploads were finalized without returning the project.')
    }
  }

  const deleteImageUploads = async (
    input: DeleteProjectImagesInput
  ) => {
    const payload = await deleteProjectImagesMutation({ input })
    const project = payload.project

    if (!project) {
      throw new Error('Images were deleted without returning the project.')
    }
  }

  const uploadImages = async ({
    uploadItems,
    projectId
  }: {
    uploadItems: ImageEditorItem[]
    projectId: string
  }) => {
    const validUploadItems = uploadItems.filter(
      (item): item is UploadImageEditorItem =>
        item.fullFile instanceof Blob && item.thumbFile instanceof Blob
    )

    if (validUploadItems.length !== uploadItems.length) {
      const invalidCount = uploadItems.length - validUploadItems.length
      throw new Error(
        `${invalidCount} image${invalidCount === 1 ? '' : 's'} could not be uploaded because its files are missing.`
      )
    }

    const items = imageEditorItemsToProjectImagePrepareItemInput(validUploadItems)

    if (items.length === 0) {
      return {
        failedClientIds: [],
        succeededItems: []
      } satisfies UploadImagesResult
    }

    const instructions = await prepareImageUploads({
      projectId,
      items
    })

    if (instructions.length !== items.length) {
      throw new Error('Upload instruction count did not match upload item count.')
    }

    const {
      succeededProjectImageIds,
      failedProjectImageIds
    } = await uploadImagesToStorage(instructions, validUploadItems)

    if (succeededProjectImageIds.length > 0) {
      await finalizeImageUploads({
        projectId,
        projectImageIds: succeededProjectImageIds
      })
    }

    if (failedProjectImageIds.length > 0) {
      await deleteImageUploads({
        projectId,
        projectImageIds: failedProjectImageIds
      })
    }

    return {
      failedClientIds: instructions
        .filter(instruction => failedProjectImageIds.includes(instruction.projectImageId))
        .map(instruction => instruction.clientId),
      succeededItems: instructions
        .filter(instruction => succeededProjectImageIds.includes(instruction.projectImageId))
        .map(instruction => ({
          clientId: instruction.clientId,
          projectImageId: instruction.projectImageId
        }))
    } satisfies UploadImagesResult
  }

  return {
    isProcessingImages,
    deleteImageUploads,
    uploadImages
  }
}
