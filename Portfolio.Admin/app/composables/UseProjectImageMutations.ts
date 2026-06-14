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
  ): Promise<ProjectFragment | null> => {
    const response = await finalizeImagesUploadMutation({ input })

    if (response.error) throw response.error

    const project = response.data?.finalizeProjectImageUploads.project ?? null

    return project
      ? useFragment(ProjectFragmentDoc, project)
      : null
  }

  const deleteImageUploads = async (
    input: DeleteProjectImagesInput
  ): Promise<ProjectFragment | null> => {
    const response = await deleteProjectImagesMutation({ input })

    if (response.error) throw response.error

    const project = response.data?.deleteProjectImages.project ?? null

    return project
      ? useFragment(ProjectFragmentDoc, project)
      : null
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
      throw new Error('Upload instruction count did not match upload item count.')
    }

    const {
      succeededProjectImageIds,
      failedProjectImageIds
    } = await uploadImagesToStorage(instructions, validUploadItems)

    let updatedProject: ProjectFragment | null = null

    if (succeededProjectImageIds.length > 0) {
      updatedProject = await finalizeImageUploads({
        projectId,
        projectImageIds: succeededProjectImageIds
      })
    }

    if (failedProjectImageIds.length > 0) {
      updatedProject = await deleteImageUploads({
        projectId,
        projectImageIds: failedProjectImageIds
      })
    }

    return {
      project: updatedProject,
      succeededProjectImageIds,
      failedProjectImageIds
    }
  }

  return {
    isProcessingImages,
    deleteImageUploads,
    uploadImages
  }
}
