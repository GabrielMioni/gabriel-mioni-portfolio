import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { useProjectTagMutations } from '~/composables/useProjectTagMutations'
import type { TagEditorItem } from '~/types/tags'

const mocks = vi.hoisted(() => ({
  createTags: vi.fn(),
  executeUpdateTags: vi.fn(),
  updatingTags: { value: false }
}))

beforeEach(() => {
  vi.stubGlobal('useTagMutations', () => ({
    createTags: mocks.createTags
  }))
  vi.stubGlobal('useApiMutation', () => ({
    executeMutation: mocks.executeUpdateTags,
    fetching: mocks.updatingTags
  }))
})

afterEach(() => {
  mocks.createTags.mockReset()
  mocks.executeUpdateTags.mockReset()
  mocks.updatingTags.value = false
  vi.unstubAllGlobals()
})

describe('useProjectTagMutations', () => {
  it('updates the project without recreating existing tags', async () => {
    const tagItems: TagEditorItem[] = [
      { id: 'tag-1', name: 'Vue', value: 'vue' },
      { id: 'tag-2', name: 'GraphQL', value: 'graphql' }
    ]
    mocks.executeUpdateTags.mockResolvedValue({
      project: { id: 'project-1' }
    })

    const { saveProjectTags } = useProjectTagMutations()
    const tagItemsRef = ref(tagItems)
    await saveProjectTags('project-1', tagItemsRef)

    expect(mocks.createTags).not.toHaveBeenCalled()
    expect(mocks.executeUpdateTags).toHaveBeenCalledWith({
      input: {
        projectId: 'project-1',
        tagIds: ['tag-1', 'tag-2']
      }
    })
    expect(tagItemsRef.value).toEqual(tagItems)
  })

  it('creates pending tags before updating the project', async () => {
    const existingTag: TagEditorItem = {
      id: 'tag-1',
      name: 'Vue',
      value: 'vue'
    }
    const pendingTags: TagEditorItem[] = [
      { id: null, name: 'GraphQL', value: 'graphql' },
      { id: null, name: 'ASP.NET', value: null }
    ]
    mocks.createTags.mockResolvedValue([
      { id: 'tag-3', name: 'ASP.NET', value: 'asp.net' },
      { id: 'tag-2', name: 'GraphQL', value: 'graphql' }
    ])
    mocks.executeUpdateTags.mockResolvedValue({
      project: { id: 'project-1' }
    })

    const { saveProjectTags } = useProjectTagMutations()
    const tagItemsRef = ref([existingTag, ...pendingTags])
    await saveProjectTags('project-1', tagItemsRef)

    expect(mocks.createTags).toHaveBeenCalledWith(pendingTags)
    expect(mocks.executeUpdateTags).toHaveBeenCalledWith({
      input: {
        projectId: 'project-1',
        tagIds: ['tag-1', 'tag-2', 'tag-3']
      }
    })
    expect(tagItemsRef.value).toEqual([
      existingTag,
      { id: 'tag-2', name: 'GraphQL', value: 'graphql' },
      { id: 'tag-3', name: 'ASP.NET', value: 'asp.net' }
    ])
  })

  it('does not update the project when a pending tag is not returned', async () => {
    mocks.createTags.mockResolvedValue([])

    const { saveProjectTags } = useProjectTagMutations()
    const tagItems = ref<TagEditorItem[]>([
      { id: null, name: 'GraphQL', value: 'graphql' }
    ])

    await expect(saveProjectTags('project-1', tagItems)).rejects.toThrow(
      'Created tag "GraphQL" was not returned.'
    )
    expect(mocks.executeUpdateTags).not.toHaveBeenCalled()
  })

  it('rejects an update response that does not return the project', async () => {
    mocks.executeUpdateTags.mockResolvedValue({ project: null })

    const { saveProjectTags } = useProjectTagMutations()

    await expect(saveProjectTags('project-1', ref([]))).rejects.toThrow(
      'Tag update returned no project.'
    )
  })

  it('retains created tag IDs when the project update fails', async () => {
    mocks.createTags.mockResolvedValue([
      { id: 'tag-1', name: 'GraphQL', value: 'graphql' }
    ])
    mocks.executeUpdateTags.mockResolvedValue({ project: null })
    const tagItems = ref<TagEditorItem[]>([
      { id: null, name: 'GraphQL', value: 'graphql' }
    ])

    const { saveProjectTags } = useProjectTagMutations()

    await expect(saveProjectTags('project-1', tagItems)).rejects.toThrow(
      'Tag update returned no project.'
    )
    expect(tagItems.value).toEqual([
      { id: 'tag-1', name: 'GraphQL', value: 'graphql' }
    ])
  })
})
