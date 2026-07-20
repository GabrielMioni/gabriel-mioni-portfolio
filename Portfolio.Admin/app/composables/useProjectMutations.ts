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
  } = useApiMutation(
    CreateProjectDocument,
    data => data.createProject,
    'Failed to create project.'
  )

  const {
    executeMutation: executeEditProject,
    fetching: editing
  } = useApiMutation(
    EditProjectDocument,
    data => data.editProject,
    'Failed to save project.'
  )

  const {
    executeMutation: executeDeleteProject,
    fetching: deleting
  } = useApiMutation(
    DeleteProjectDocument,
    data => data.deleteProject,
    'Failed to delete project.'
  )

  const createProject = async (input: CreateProjectInput) => {
    const payload = await executeCreateProject({ input })

    if (!payload.project) {
      throw new Error('Project creation returned no project.')
    }

    return payload.project
  }

  const deleteProject = async (input: DeleteProjectInput) => {
    const payload = await executeDeleteProject({ input })

    if (!payload.deletedProjectId) {
      throw new Error('Project deletion returned no project ID.')
    }

    return payload.deletedProjectId
  }

  const editProject = async (input: EditProjectInput) => {
    const payload = await executeEditProject({ input })

    if (!payload.project) {
      throw new Error('Project save returned no project.')
    }

    return payload.project
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
