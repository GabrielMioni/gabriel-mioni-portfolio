import { useMutation } from '@urql/vue'
import {
  type CreateProjectInput,
  type DeleteProjectInput,
  type EditProjectInput,
  CreateProjectDocument,
  DeleteProjectDocument,
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

  const {
    executeMutation: executeDeleteProject,
    fetching: deleting
  } = useMutation(DeleteProjectDocument)

  const createProject = async (input: CreateProjectInput) => {
    const response = await executeCreateProject({ input })

    if (response.error) throw response.error

    return response.data?.createProject
  }

  const deleteProject = async (input: DeleteProjectInput) => {
    const response = await executeDeleteProject({ input })

    if (response.error) throw response.error

    const payload = response.data?.deleteProject
    if (!payload) throw new Error('Delete project returned no payload.')

    return payload
  }

  const editProject = async (input: EditProjectInput) => {
    const response = await executeEditProject({ input })

    if (response.error) throw response.error

    return response.data?.editProject
  }

  return {
    createProject,
    creating,
    deleteProject,
    deleting,
    editProject,
    editing
  }
}
