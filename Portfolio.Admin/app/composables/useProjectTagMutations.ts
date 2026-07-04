import { useMutation } from '@urql/vue'
import {
  CreateProjectTagsDocument,
  ProjectTagFragmentDoc,
  type CreateProjectTagsMutationVariables,
  type ProjectTagFragment
} from '~/generated/graphql'
import { useFragment } from '~/generated'

export const useProjectTagMutations = () => {
  const {
    executeMutation: executeCreateMany,
    fetching: creatingTags
  } = useMutation(CreateProjectTagsDocument)

  const createProjectTags = async (variables: CreateProjectTagsMutationVariables): Promise<ProjectTagFragment[]> => {
    const response = await executeCreateMany(variables)
    if (response.error) throw response.error
    return (response.data?.createProjectTags ?? [])
      .map(t => useFragment(ProjectTagFragmentDoc, t))
  }

  return { createProjectTags, creatingTags }
}
