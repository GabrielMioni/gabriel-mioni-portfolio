import { UpdateProjectTagsDocument } from '~/generated/graphql'
import type { TagEditorItem } from '~/types/tags'

export const useProjectTagMutations = () => {
  const {
    executeMutation: executeUpdateTags,
    fetching: updatingTags
  } = useApiMutation(
    UpdateProjectTagsDocument,
    data => data.updateProjectTags,
    'Failed to update project tags.'
  )

  const updateProjectTags = async (projectId: string, tagItems: TagEditorItem[]): Promise<void> => {
    const tagIds = tagItems.map(t => t.id).filter((id): id is string => Boolean(id))
    const payload = await executeUpdateTags({ input: { projectId, tagIds } })

    if (!payload.project) {
      throw new Error('Tag update returned no project.')
    }
  }

  return {
    updateProjectTags,
    updatingTags
  }
}
