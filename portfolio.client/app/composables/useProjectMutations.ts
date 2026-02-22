import { useMutation } from '@urql/vue'
import {
  type EditProjectInput,
  EditProjectDocument
} from '~/generated/graphql'

export const useProjectMutations = () => {
  const {
    executeMutation,
    fetching: editing
  } = useMutation(EditProjectDocument)

  const editProject = async (input: EditProjectInput) => {
    const response = await executeMutation({ input })

    if (response.error) throw response.error

    return response.data?.editProject
  }

  return {
    editProject,
    editing
  }
}
