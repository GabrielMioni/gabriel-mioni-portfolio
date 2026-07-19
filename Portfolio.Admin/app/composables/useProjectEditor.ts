import { useQuery } from '@urql/vue'
import { useFragment } from '~/generated'
import {
  type ProjectFragment,
  type ProjectTagFragment,
  GetProjectByIdDocument,
  ProjectFragmentDoc
} from '~/generated/graphql'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import type { TagEditorItem } from '~/types/tags'
import { restoreEditorItem } from '~/utils/editorItems'
import {
  buildCreateProjectInput,
  buildEditProjectInput,
  cloneProjectEditorDraft,
  createEmptyProjectEditorDraft,
  isProjectEditorDraftDirty,
  projectFragmentToEditorDraft,
  type ProjectEditorDraft
} from '~/utils/projects/projectEditor'

export const useProjectEditor = () => {
  const route = useRoute()
  const router = useRouter()
  const { showSnackbar } = useSnackbarStore()

  const routeProjectId = computed(() => {
    const id = route.params?.id
    return typeof id === 'string' && id.length > 0 ? id : null
  })

  // Retain a successfully created ID if a later image or tag step fails.
  // A retry then edits the existing project instead of creating a duplicate.
  const createdProjectId = ref<string | null>(null)
  const projectId = computed(() => routeProjectId.value ?? createdProjectId.value)
  const isNewProject = computed(() => !projectId.value)

  const { createProject, editProject } = useProjectMutations()
  const { deleteImageUploads, uploadImages } = useProjectImageMutations()
  const { createProjectTags, updateProjectTags } = useProjectTagMutations()

  const { data, error, fetching, executeQuery } = useQuery({
    query: GetProjectByIdDocument,
    variables: computed(() => ({ id: routeProjectId.value ?? '' })),
    pause: computed(() => !routeProjectId.value)
  })

  watch(error, (currentError) => {
    if (!currentError) return
    console.error('Failed to fetch project data', currentError)
    showSnackbar('Failed to load project data', 'error')
  })

  const emptyDraft = createEmptyProjectEditorDraft()
  const draft = reactive<ProjectEditorDraft>(cloneProjectEditorDraft(emptyDraft))
  const baseline = ref<ProjectEditorDraft>(cloneProjectEditorDraft(emptyDraft))

  const projectDetailsModel = draft.form
  const imageItems = toRef(draft, 'imageItems')
  const linkItems = toRef(draft, 'linkItems')
  const tagItems = toRef(draft, 'tagItems')

  const project = computed(() => {
    const projectRef = data.value?.projectById
    return projectRef ? useFragment(ProjectFragmentDoc, projectRef) : null
  })

  const isInitialLoading = computed(() =>
    Boolean(routeProjectId.value) && fetching.value && !project.value
  )

  const isSubmitting = ref(false)
  const isSavingProject = computed(() => isSubmitting.value)

  const activeImageItems = computed(() => imageItems.value.filter(item => !item.isRemoved))
  const removedImageItems = computed(() => imageItems.value.filter(item => item.isRemoved))
  const activeLinkItems = computed(() => linkItems.value.filter(item => !item.isRemoved))
  const removedLinkItems = computed(() => linkItems.value.filter(item => item.isRemoved))

  const uploadItems = computed(() =>
    activeImageItems.value.filter((item): item is ImageEditorItem =>
      !item.id
    )
  )

  const deleteImageIds = computed(() =>
    removedImageItems.value
      .map(item => item.id)
      .filter((id): id is string => Boolean(id))
  )

  const hasTagUpdates = computed(() => {
    const tagKey = (tag: TagEditorItem) => tag.id ?? tag.value ?? tag.name
    const current = tagItems.value.map(tagKey).sort()
    const original = baseline.value.tagItems.map(tagKey).sort()
    return JSON.stringify(current) !== JSON.stringify(original)
  })

  const hasUpdates = computed(() =>
    isProjectEditorDraftDirty(draft, baseline.value)
  )

  const syncFromProject = (currentProject: ProjectFragment) => {
    const nextDraft = projectFragmentToEditorDraft(currentProject)

    Object.assign(draft.form, nextDraft.form)
    draft.imageItems = nextDraft.imageItems
    draft.linkItems = nextDraft.linkItems
    draft.tagItems = nextDraft.tagItems
    baseline.value = cloneProjectEditorDraft(nextDraft)
  }

  const refreshProject = async () => {
    if (!routeProjectId.value) return false

    const result = await executeQuery({ requestPolicy: 'network-only' })
    const refreshedRef = result.data?.value?.projectById

    if (!refreshedRef) {
      showSnackbar('Project saved, but failed to refresh project data.', 'warning')
      return false
    }

    syncFromProject(useFragment(ProjectFragmentDoc, refreshedRef))
    return true
  }

  const deleteRemovedImages = async (targetProjectId: string) => {
    if (deleteImageIds.value.length === 0) return

    await deleteImageUploads({
      projectId: targetProjectId,
      projectImageIds: deleteImageIds.value
    })
  }

  const resolvePendingTags = async (): Promise<ProjectTagFragment[]> => {
    const pending = tagItems.value.filter(tag => !tag.id)
    if (pending.length === 0) return []

    const resolved = await createProjectTags(pending)
    tagItems.value = tagItems.value.map((tag) => {
      if (tag.id) return tag
      const match = resolved.find(item => item.value === tag.value)
      return match
        ? { id: match.id, name: match.name, value: match.value }
        : tag
    })

    return resolved
  }

  const saveRelatedData = async (targetProjectId: string, updateTags: boolean) => {
    if (uploadItems.value.length > 0) {
      const uploadResult = await uploadImages({
        uploadItems: uploadItems.value,
        projectId: targetProjectId
      })

      const uploadedIdByClientId = new Map(
        uploadResult.succeededItems.map(item => [item.clientId, item.projectImageId])
      )
      imageItems.value = imageItems.value.map((item) => {
        const uploadedId = uploadedIdByClientId.get(item.clientId)
        return uploadedId ? { ...item, id: uploadedId } : item
      })

      if (uploadResult.failedClientIds.length > 0) {
        throw new Error(`Failed to upload ${uploadResult.failedClientIds.length} images.`)
      }
    }

    await deleteRemovedImages(targetProjectId)

    if (updateTags) {
      await resolvePendingTags()
      await updateProjectTags(targetProjectId, tagItems.value)
    }
  }

  const submitEditProject = async (targetProjectId: string): Promise<boolean> => {
    await editProject(buildEditProjectInput(targetProjectId, draft))

    await saveRelatedData(targetProjectId, hasTagUpdates.value)

    if (routeProjectId.value) {
      return refreshProject()
    }

    await router.replace(`/projects/${targetProjectId}`)
    return true
  }

  const submitCreateProject = async (): Promise<boolean> => {
    const result = await createProject(buildCreateProjectInput(draft))
    const newProjectId = result?.id

    if (!newProjectId) {
      throw new Error('Project was created without returning an ID.')
    }

    createdProjectId.value = newProjectId
    await saveRelatedData(newProjectId, tagItems.value.length > 0)
    await router.replace(`/projects/${newProjectId}`)
    return true
  }

  const submitProject = async () => {
    if (isSubmitting.value || !hasUpdates.value) return

    const wasNewProject = isNewProject.value
    isSubmitting.value = true

    try {
      let saveCompleted: boolean

      if (projectId.value) {
        saveCompleted = await submitEditProject(projectId.value)
      } else {
        saveCompleted = await submitCreateProject()
      }

      if (saveCompleted) {
        showSnackbar(wasNewProject ? 'Project created.' : 'Project saved.', 'success')
      }
    } catch (err) {
      console.error('Failed to save project', err)
      const message = wasNewProject && createdProjectId.value
        ? 'Project created, but some related data failed to save. Retry to continue.'
        : 'Failed to save project.'
      showSnackbar(message, 'error')
    } finally {
      isSubmitting.value = false
    }
  }

  const restoreImageItem = (clientId: string) => {
    imageItems.value = restoreEditorItem(clientId, imageItems.value)
  }

  const restoreLinkItem = (clientId: string) => {
    linkItems.value = restoreEditorItem(clientId, linkItems.value)
  }

  const syncedProjectId = ref<string | null>(null)
  watch(
    project,
    (currentProject) => {
      if (!currentProject || syncedProjectId.value === currentProject.id) return
      syncFromProject(currentProject)
      syncedProjectId.value = currentProject.id
    },
    { immediate: true }
  )

  return {
    projectDetailsModel,
    imageItems,
    linkItems,
    tagItems,
    activeImageItems,
    activeLinkItems,
    removedImageItems,
    removedLinkItems,
    isNewProject,
    isSavingProject,
    hasUpdates,
    isInitialLoading,
    projectId,
    restoreImageItem,
    restoreLinkItem,
    submitProject
  }
}
