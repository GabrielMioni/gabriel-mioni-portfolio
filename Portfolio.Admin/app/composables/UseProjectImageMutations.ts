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
  project: ProjectFragment | null
  succeededProjectImageIds: string[]
  failedProjectImageIds: string[]
  succeededClientIds: string[]
  failedClientIds: string[]
  succeededItems: UploadedProjectImage[]
  error: unknown | null
}

type PendingUploadBatch = {
  projectId: string
  succeededItems: UploadedProjectImage[]
  failedItems: UploadedProjectImage[]
  isFinalized: boolean
  isCleanedUp: boolean
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

  const hasPendingImageOperations = ref(false)
  let pendingUploadBatch: PendingUploadBatch | null = null

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

  const emptyUploadResult = (): UploadImagesResult => ({
    project: null,
    succeededProjectImageIds: [],
    failedProjectImageIds: [],
    succeededClientIds: [],
    failedClientIds: [],
    succeededItems: [],
    error: null
  })

  const batchToResult = (
    batch: PendingUploadBatch,
    project: ProjectFragment | null,
    error: unknown | null
  ): UploadImagesResult => ({
    project,
    succeededProjectImageIds: batch.succeededItems.map(item => item.projectImageId),
    failedProjectImageIds: batch.failedItems.map(item => item.projectImageId),
    succeededClientIds: batch.succeededItems.map(item => item.clientId),
    failedClientIds: batch.failedItems.map(item => item.clientId),
    succeededItems: batch.succeededItems,
    error
  })

  const mergeUploadResults = (
    first: UploadImagesResult,
    second: UploadImagesResult
  ): UploadImagesResult => ({
    project: second.project ?? first.project,
    succeededProjectImageIds: [
      ...first.succeededProjectImageIds,
      ...second.succeededProjectImageIds
    ],
    failedProjectImageIds: [
      ...first.failedProjectImageIds,
      ...second.failedProjectImageIds
    ],
    succeededClientIds: [
      ...first.succeededClientIds,
      ...second.succeededClientIds
    ],
    failedClientIds: [
      ...first.failedClientIds,
      ...second.failedClientIds
    ],
    succeededItems: [...first.succeededItems, ...second.succeededItems],
    error: second.error ?? first.error
  })

  const completePendingUploadBatch = async (): Promise<UploadImagesResult> => {
    const batch = pendingUploadBatch
    if (!batch) return emptyUploadResult()

    let updatedProject: ProjectFragment | null = null

    try {
      if (!batch.isFinalized && batch.succeededItems.length > 0) {
        updatedProject = await finalizeImageUploads({
          projectId: batch.projectId,
          projectImageIds: batch.succeededItems.map(item => item.projectImageId)
        })
        batch.isFinalized = true
      }

      if (!batch.isCleanedUp && batch.failedItems.length > 0) {
        updatedProject = await deleteImageUploads({
          projectId: batch.projectId,
          projectImageIds: batch.failedItems.map(item => item.projectImageId)
        })
        batch.isCleanedUp = true
      }
    } catch (error) {
      return batchToResult(batch, updatedProject, error)
    }

    pendingUploadBatch = null
    hasPendingImageOperations.value = false

    return batchToResult(batch, updatedProject, null)
  }

  const uploadImages = async ({
    uploadItems,
    projectId
  }: {
    uploadItems: ImageEditorItem[]
    projectId: string
  }) => {
    if (pendingUploadBatch && pendingUploadBatch.projectId !== projectId) {
      throw new Error('Pending image operations belong to a different project.')
    }

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

    let result = emptyUploadResult()
    const resumedClientIds = new Set<string>()

    if (pendingUploadBatch) {
      for (const item of [
        ...pendingUploadBatch.succeededItems,
        ...pendingUploadBatch.failedItems
      ]) {
        resumedClientIds.add(item.clientId)
      }

      const resumedResult = await completePendingUploadBatch()
      result = mergeUploadResults(result, resumedResult)

      if (resumedResult.error) return result
    }

    const remainingUploadItems = validUploadItems.filter(
      item => !resumedClientIds.has(item.clientId)
    )

    const items = imageEditorItemsToProjectImagePrepareItemInput(remainingUploadItems)

    if (items.length === 0) {
      return result
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
    } = await uploadImagesToStorage(instructions, remainingUploadItems)

    pendingUploadBatch = {
      projectId,
      succeededItems: instructions
        .filter(instruction => succeededProjectImageIds.includes(instruction.projectImageId))
        .map(instruction => ({
          clientId: instruction.clientId,
          projectImageId: instruction.projectImageId
        })),
      failedItems: instructions
        .filter(instruction => failedProjectImageIds.includes(instruction.projectImageId))
        .map(instruction => ({
          clientId: instruction.clientId,
          projectImageId: instruction.projectImageId
        })),
      isFinalized: succeededProjectImageIds.length === 0,
      isCleanedUp: failedProjectImageIds.length === 0
    }
    hasPendingImageOperations.value = true

    const currentResult = await completePendingUploadBatch()
    return mergeUploadResults(result, currentResult)
  }

  return {
    isProcessingImages,
    hasPendingImageOperations,
    deleteImageUploads,
    uploadImages
  }
}
