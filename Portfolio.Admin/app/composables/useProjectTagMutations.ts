import { useMutation } from '@urql/vue'
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
  } = useMutation(CreateProjectTagsDocument)

  const {
    executeMutation: executeUpdateTags,
    fetching: updatingTags
  } = useMutation(UpdateProjectTagsDocument)

  const { executeMutation: executeRename, fetching: renamingTag } = useMutation(RenameProjectTagDocument)
  const { executeMutation: executeRemoveFromProjects, fetching: removingFromProjects } = useMutation(RemoveTagFromProjectsDocument)
  const { executeMutation: executeDelete, fetching: deletingTag } = useMutation(DeleteProjectTagDocument)

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

  const renameProjectTag = async (id: string, name: string): Promise<ProjectTagFragment | null> => {
    const response = await executeRename({ input: { id, name } })
    if (response.error) throw response.error
    const data = response.data?.renameProjectTag
    return data ? useFragment(ProjectTagFragmentDoc, data) : null
  }

  const removeTagFromProjects = async (tagId: string, projectIds: string[]): Promise<string[]> => {
    const response = await executeRemoveFromProjects({ input: { tagId, projectIds } })
    if (response.error) throw response.error
    return response.data?.removeTagFromProjects ?? []
  }

  const deleteProjectTag = async (id: string): Promise<string | null> => {
    const response = await executeDelete({ input: { id } })
    if (response.error) throw response.error
    return response.data?.deleteProjectTag ?? null
  }

  return {
    createProjectTags, updateProjectTags, creatingTags, updatingTags,
    renameProjectTag, removeTagFromProjects, renamingTag, removingFromProjects,
    deleteProjectTag, deletingTag
  }
}
