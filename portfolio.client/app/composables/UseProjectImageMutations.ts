import { useMutation } from '@urql/vue'
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
  ProjectImageUploadInstructionFragmentDoc,
  ProjectFragmentDoc
} from '~/generated/graphql'
import { useFragment } from '~/generated'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import {
  imageEditorItemsToProjectImagePrepareItemInput,
  uploadImagesToStorage
} from '~/utils/images/'

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

    const project = response.data?.finalizeProjectImageUploads.project ?? null
    return project ? useFragment(ProjectFragmentDoc, project) : null
  }

  const deleteImageUploads = async (input: DeleteProjectImagesInput) => {
    const response = await deleteImagesUploadMutation({ input })

    if (response.error) throw response.error

    const project = response.data?.deleteProjectImages.project ?? null
    return project ? useFragment(ProjectFragmentDoc, project) : null
  }

  const uploadImages = async ({
    uploadItems,
    projectId
  }: {
    uploadItems: ImageEditorItem[]
    projectId: string
  }) => {
    const validUploadItems = uploadItems.filter(
      (item): item is ImageEditorItem =>
        !!item.fullFile && !!item.thumbFile
    )

    const items = imageEditorItemsToProjectImagePrepareItemInput(validUploadItems)

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

    let project: ProjectFragment | null = null

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
