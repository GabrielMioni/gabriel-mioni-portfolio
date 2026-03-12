import { useMutation } from '@urql/vue'
import {
  type CreateProjectInput,
  type EditProjectInput,
  type RequestProjectImageUploadsInput,
  CreateProjectDocument,
  EditProjectDocument,
  PrepareProjectImageUploadsDocument
} from '~/generated/graphql'

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

  const prepareImages = async (input: RequestProjectImageUploadsInput) => {
    const response = await prepareImagesUploadMutation({ input })

    if (response.error) throw response.error

    return response.data?.prepareProjectImageUploads
  }

  return {
    createProject,
    creating,
    editProject,
    editing,
    prepareImages,
    preparingImages
  }
}
