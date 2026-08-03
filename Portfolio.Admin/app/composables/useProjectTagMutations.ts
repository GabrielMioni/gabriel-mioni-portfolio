import type { Ref } from 'vue'
import { UpdateProjectTagsDocument } from '~/generated/graphql'
import type { TagEditorItem } from '~/types/tags'
import { generateTagValue } from '~/utils/tags'

export const useProjectTagMutations = () => {
  const { createTags } = useTagMutations()

  const {
    executeMutation: executeUpdateTags
  } = useApiMutation(
    UpdateProjectTagsDocument,
    data => data.updateProjectTags,
    'Failed to update project tags.'
  )

  const persistPendingTags = async (
    tagItems: TagEditorItem[]
  ): Promise<TagEditorItem[]> => {
    const pendingTags = tagItems.filter(tag => !tag.id)
    if (pendingTags.length === 0) return tagItems

    const createdTags = await createTags(pendingTags)
    const createdTagsByValue = new Map(
      createdTags.map(tag => [tag.value, tag])
    )

    return tagItems.map((tag) => {
      if (tag.id) return tag

      const createdTag = createdTagsByValue.get(
        tag.value ?? generateTagValue(tag.name)
      )

      if (!createdTag) {
        throw new Error(`Created tag "${tag.name}" was not returned.`)
      }

      return {
        id: createdTag.id,
        name: createdTag.name,
        value: createdTag.value
      }
    })
  }

  const updateProjectTags = async (
    projectId: string,
    tagItems: TagEditorItem[]
  ): Promise<void> => {
    const tagIds = tagItems.map(t => t.id).filter((id): id is string => Boolean(id))
    const payload = await executeUpdateTags({ input: { projectId, tagIds } })

    if (!payload.project) {
      throw new Error('Tag update returned no project.')
    }
  }

  const saveProjectTags = async (
    projectId: string,
    tagItems: Ref<TagEditorItem[]>
  ): Promise<void> => {
    tagItems.value = await persistPendingTags(tagItems.value)
    await updateProjectTags(projectId, tagItems.value)
  }

  return {
    saveProjectTags
  }
}
