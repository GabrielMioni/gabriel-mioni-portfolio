import { useMutation } from '@urql/vue'
import { useFragment } from '~/generated'
import type {
  DeleteProjectImagesInput,
  FinalizeProjectImageUploadsInput,
  PrepareProjectImageUploadsInput,
  ProjectFragment
} from '~/generated/graphql'
import {
  DeleteProjectImagesDocument,
  FinalizeProjectImageUploadsDocument,
  PrepareProjectImageUploadsDocument,
  ProjectFragmentDoc,
  ProjectImageUploadInstructionFragmentDoc
} from '~/generated/graphql'
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
  } = useMutation(PrepareProjectImageUploadsDocument)

  const {
    executeMutation: finalizeImagesUploadMutation,
    fetching: finalizingImages
  } = useMutation(FinalizeProjectImageUploadsDocument)

  const {
    executeMutation: deleteProjectImagesMutation,
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
      throw new Error('No upload instructions returned.')
    }

    return useFragment(ProjectImageUploadInstructionFragmentDoc, items)
  }

  const finalizeImageUploads = async (
    input: FinalizeProjectImageUploadsInput
  ): Promise<ProjectFragment> => {
    const response = await finalizeImagesUploadMutation({ input })

    if (response.error) throw response.error

    const project = response.data?.finalizeProjectImageUploads.project ?? null

    if (!project) {
      throw new Error('Image uploads were finalized without returning the project.')
    }

    return useFragment(ProjectFragmentDoc, project)
  }

  const deleteImageUploads = async (
    input: DeleteProjectImagesInput
  ): Promise<ProjectFragment> => {
    const response = await deleteProjectImagesMutation({ input })

    if (response.error) throw response.error

    const project = response.data?.deleteProjectImages.project ?? null

    if (!project) {
      throw new Error('Images were deleted without returning the project.')
    }

    return useFragment(ProjectFragmentDoc, project)
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
