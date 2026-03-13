import { useMutation } from '@urql/vue'
import {
  type CreateProjectInput,
  type EditProjectInput,
  CreateProjectDocument,
  EditProjectDocument
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

  return {
    createProject,
    creating,
    editProject,
    editing
  }
}
