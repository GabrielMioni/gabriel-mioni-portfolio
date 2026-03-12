import { useMutation } from '@urql/vue'
import {
  type CreateProjectInput,
  type EditProjectInput,
  type ProjectImageUploadRequestInput,
  CreateProjectDocument,
  EditProjectDocument,
  PrepareProjectImageUploadsDocument
} from '~/generated/graphql'
import type { ImageUploadItem } from '~/types/images/ImageUploadItem'

export const useProjectMutations = () => {
  const {
    executeMutation: executeCreateProject,
    fetching: creating
  } = useMutation(CreateProjectDocument)

  const {
    executeMutation: executeEditProject,
    fetching: editing
  } = useMutation(EditProjectDocument)

  const {
    executeMutation: prepareImagesUploadMutation,
    fetching: preparingImages
  } = useMutation(PrepareProjectImageUploadsDocument)

  const createProject = async (input: CreateProjectInput) => {
    const response = await executeCreateProject({ input })

    if (response.error) throw response.error

    return response.data?.createProject
  }

  const editProject = async (input: EditProjectInput) => {
    const response = await executeEditProject({ input })

    if (response.error) throw response.error

    return response.data?.editProject
  }

  const uploadImages = async ({ uploadItems, projectId }: {
    uploadItems: ImageUploadItem[],
    projectId: string
  }) => {
    const items = uploadItems
      .map((item): ProjectImageUploadRequestInput | null => {
        const fullFile = item.fullFile
        const thumbFile = item.thumbFile

        if (!fullFile || !thumbFile) return null

        return {
          altText: item.altText,
          clientId: item.clientId,
          fullContentType: fullFile.type,
          fullSizeBytes: fullFile.size,
          thumbContentType: thumbFile.type,
          thumbSizeBytes: thumbFile.size
        }
      })
      .filter((item): item is ProjectImageUploadRequestInput => item !== null)

    const response = await prepareImagesUploadMutation({
      input: {
        projectId,
        items
      }
    })

    if (response.error) throw response.error

    const instructions = response.data?.prepareProjectImageUploads.items

    if (!instructions || instructions.length <= 0) throw new Error('No instructions available')

    const uploads = instructions.map(async (instruction) => {
      const matchingItem = uploadItems.find(item => item.clientId === instruction.clientId)

      if (!matchingItem?.fullFile || !matchingItem.thumbFile) {
        throw new Error(`Missing files for clientId ${instruction.clientId}`)
      }

      await Promise.all([
        fetch(instruction.full.uploadUrl, {
          method: 'PUT',
          headers: {
            'Content-Type': matchingItem.fullFile.type
          },
          body: matchingItem.fullFile
        }),
        fetch(instruction.thumb.uploadUrl, {
          method: 'PUT',
          headers: {
            'Content-Type': matchingItem.thumbFile.type
          },
          body: matchingItem.thumbFile
        })
      ])
    })

    await Promise.all(uploads)
  }

  return {
    createProject,
    creating,
    editProject,
    editing,
    uploadImages,
    preparingImages
  }
}
