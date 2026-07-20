import {
  type ProjectTagFragment,
  CreateProjectTagsDocument,
  UpdateProjectTagsDocument,
  RenameProjectTagDocument,
  RemoveTagFromProjectsDocument,
  DeleteProjectTagDocument,
  ProjectTagFragmentDoc
} from '~/generated/graphql'
import { useFragment } from '~/generated'
import type { TagEditorItem } from '~/types/tags'

export const useProjectTagMutations = () => {
  const {
    executeMutation: executeCreateMany,
    fetching: creatingTags
  } = useApiMutation(
    CreateProjectTagsDocument,
    data => data.createProjectTags,
    'Failed to create project tags.'
  )

  const {
    executeMutation: executeUpdateTags,
    fetching: updatingTags
  } = useApiMutation(
    UpdateProjectTagsDocument,
    data => data.updateProjectTags,
    'Failed to update project tags.'
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

  const createProjectTags = async (tagItems: TagEditorItem[]): Promise<ProjectTagFragment[]> => {
    const payload = await executeCreateMany({
      input: { names: tagItems.map(tag => tag.name) }
    })

    if (!payload.tags) {
      throw new Error('Tag creation returned no tags.')
    }

    return payload.tags
      .map(t => useFragment(ProjectTagFragmentDoc, t))
  }

  const updateProjectTags = async (projectId: string, tagItems: TagEditorItem[]): Promise<void> => {
    const tagIds = tagItems.map(t => t.id).filter((id): id is string => Boolean(id))
    const payload = await executeUpdateTags({ input: { projectId, tagIds } })

    if (!payload.project) {
      throw new Error('Tag update returned no project.')
    }
  }

  const renameProjectTag = async (id: string, name: string): Promise<ProjectTagFragment> => {
    const payload = await executeRename({ input: { id, name } })

    if (!payload.tag) {
      throw new Error('Tag rename returned no tag.')
    }

    return useFragment(ProjectTagFragmentDoc, payload.tag)
  }

  const removeTagFromProjects = async (tagId: string, projectIds: string[]): Promise<string[]> => {
    const payload = await executeRemoveFromProjects({ input: { tagId, projectIds } })

    if (!payload.projectIds) {
      throw new Error('Tag removal returned no project IDs.')
    }

    return payload.projectIds
  }

  const deleteProjectTag = async (id: string): Promise<string> => {
    const payload = await executeDelete({ input: { id } })

    if (!payload.deletedTagId) {
      throw new Error('Tag deletion returned no tag ID.')
    }

    return payload.deletedTagId
  }

  return {
    createProjectTags, updateProjectTags, creatingTags, updatingTags,
    renameProjectTag, removeTagFromProjects, renamingTag, removingFromProjects,
    deleteProjectTag, deletingTag
  }
}
