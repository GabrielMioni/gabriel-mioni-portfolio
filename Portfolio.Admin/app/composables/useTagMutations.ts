import {
  type ProjectTagFragment,
  CreateProjectTagsDocument,
  RenameProjectTagDocument,
  RemoveTagFromProjectsDocument,
  DeleteProjectTagDocument,
  ProjectTagFragmentDoc
} from '~/generated/graphql'
import { useFragment } from '~/generated'
import type { TagEditorItem } from '~/types/tags'

export const useTagMutations = () => {
  const {
    executeMutation: executeCreateMany,
    fetching: creatingTags
  } = useApiMutation(
    CreateProjectTagsDocument,
    data => data.createProjectTags,
    'Failed to create project tags.'
  )

  const {
    executeMutation: executeRename,
    fetching: renamingTag
  } = useApiMutation(
    RenameProjectTagDocument,
    data => data.renameProjectTag,
    'Failed to rename project tag.'
  )

  const {
    executeMutation: executeRemoveFromProjects,
    fetching: removingFromProjects
  } = useApiMutation(
    RemoveTagFromProjectsDocument,
    data => data.removeTagFromProjects,
    'Failed to remove the tag from projects.'
  )

  const {
    executeMutation: executeDelete,
    fetching: deletingTag
  } = useApiMutation(
    DeleteProjectTagDocument,
    data => data.deleteProjectTag,
    'Failed to delete project tag.'
  )

  const createTags = async (
    tagItems: TagEditorItem[]
  ): Promise<ProjectTagFragment[]> => {
    const payload = await executeCreateMany({
      input: { names: tagItems.map(tag => tag.name) }
    })

    if (!payload.tags) {
      throw new Error('Tag creation returned no tags.')
    }

    return payload.tags
      .map(tag => useFragment(ProjectTagFragmentDoc, tag))
  }

  const renameTag = async (
    id: string,
    name: string
  ): Promise<ProjectTagFragment> => {
    const payload = await executeRename({ input: { id, name } })

    if (!payload.tag) {
      throw new Error('Tag rename returned no tag.')
    }

    return useFragment(ProjectTagFragmentDoc, payload.tag)
  }

  const removeTagFromProjects = async (
    tagId: string,
    projectIds: string[]
  ): Promise<string[]> => {
    const payload = await executeRemoveFromProjects({
      input: { tagId, projectIds }
    })

    if (!payload.projectIds) {
      throw new Error('Tag removal returned no project IDs.')
    }

    return payload.projectIds
  }

  const deleteTag = async (id: string): Promise<string> => {
    const payload = await executeDelete({ input: { id } })

    if (!payload.deletedTagId) {
      throw new Error('Tag deletion returned no tag ID.')
    }

    return payload.deletedTagId
  }

  return {
    createTags,
    creatingTags,
    renameTag,
    renamingTag,
    removeTagFromProjects,
    removingFromProjects,
    deleteTag,
    deletingTag
  }
}
