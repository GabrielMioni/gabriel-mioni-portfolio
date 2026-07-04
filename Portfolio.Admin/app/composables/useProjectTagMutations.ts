import { useMutation } from '@urql/vue'
import {
  type ProjectTagFragment,
  CreateProjectTagsDocument,
  UpdateProjectTagsDocument,
  ProjectTagFragmentDoc
} from '~/generated/graphql'
import { useFragment } from '~/generated'
import type { TagEditorItem } from '~/types/tags'

export const useProjectTagMutations = () => {
  const {
    executeMutation: executeCreateMany,
    fetching: creatingTags
  } = useMutation(CreateProjectTagsDocument)

  const {
    executeMutation: executeUpdateTags,
    fetching: updatingTags
  } = useMutation(UpdateProjectTagsDocument)

  const createProjectTags = async (tagItems: TagEditorItem[]): Promise<ProjectTagFragment[]> => {
    const response = await executeCreateMany({ input: { names: tagItems.map(t => t.name) } })
    if (response.error) throw response.error
    return (response.data?.createProjectTags ?? [])
      .map(t => useFragment(ProjectTagFragmentDoc, t))
  }

  const updateProjectTags = async (projectId: string, tagItems: TagEditorItem[]): Promise<void> => {
    const tagIds = tagItems.map(t => t.id).filter((id): id is string => Boolean(id))
    const response = await executeUpdateTags({ input: { projectId, tagIds } })
    if (response.error) throw response.error
  }

  return { createProjectTags, updateProjectTags, creatingTags, updatingTags }
}
