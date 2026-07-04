import { useQuery } from '@urql/vue'
import { useFragment } from '~/generated'
import {
  type CreateProjectInput,
  type CreateProjectLinkInput,
  type EditProjectImageInput,
  type EditProjectInput,
  type EditProjectLinkInput,
  type ProjectFragment,
  GetProjectByIdDocument,
  ProjectFragmentDoc,
  ProjectImageFragmentDoc,
  ProjectLinkFragmentDoc,
  ProjectTagFragmentDoc,
  ProjectStatus
} from '~/generated/graphql'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import type { TagEditorItem } from '~/types/tags'
import {
  checkIfEditorItemsUpdated,
  normalizeEditorItemsSortOrder,
  restoreEditorItem
} from '~/utils/editorItems'
import { imageFragmentToEditorItem } from '~/utils/images/imageEditorItems'
import { isLikelyValidHttpUrl } from '~/utils/links'
import { linkFragmentToEditorItem } from '~/utils/links/linkEditorItems'

export const useProjectEditor = () => {
  const route = useRoute()
  const router = useRouter()

  const projectId = computed(() => {
    const id = route.params?.id

    return typeof id === 'string' && id.length > 0
      ? id
      : null
  })

  const isExistingProject = computed(() => Boolean(projectId.value))
  const isNewProject = computed(() => !projectId.value)

  const {
    editing,
    createProject,
    editProject
  } = useProjectMutations()

  const {
    isProcessingImages,
    deleteImageUploads,
    uploadImages
  } = useProjectImageMutations()

  const { showSnackbar } = useSnackbarStore()

  const {
    data,
    error,
    fetching,
    executeQuery
  } = useQuery({
    query: GetProjectByIdDocument,
    variables: computed(() => ({
      id: projectId.value ?? ''
    })),
    pause: computed(() => !projectId.value)
  })

  watch(error, (currentError) => {
    if (currentError) {
      console.error('Failed to fetch project data', currentError)
      showSnackbar('Failed to load project data', 'error')
    }
  })

  const projectLinksIsValid = ref(false)
  const originalProject = ref<ProjectFragment | null>(null)
  const originalImageItems = ref<ImageEditorItem[]>([])
  const originalLinkItems = ref<LinkEditorItem[]>([])
  const hasInitialized = ref(false)

  const projectDetailsModel = reactive({
    title: '',
    summary: '',
    body: '',
    status: ProjectStatus.Draft
  })

  const { createProjectTags, updateProjectTags } = useProjectTagMutations()

  const imageItems = ref<ImageEditorItem[]>([])
  const linkItems = ref<LinkEditorItem[]>([])
  const tagItems = ref<TagEditorItem[]>([])
  const originalTagIds = ref<string[]>([])

  const project = computed(() => {
    const ref = data.value?.projectById

    return ref
      ? useFragment(ProjectFragmentDoc, ref)
      : null
  })

  const isInitialLoading = computed(() =>
    isExistingProject.value && fetching.value && !project.value
  )

  const isSavingProject = computed(() =>
    editing.value || isProcessingImages.value
  )

  const activeImageItems = computed(() =>
    imageItems.value.filter(item => !item.isRemoved)
  )

  const removedImageItems = computed(() =>
    imageItems.value.filter(item => item.isRemoved)
  )

  const uploadItems = computed(() =>
    activeImageItems.value.filter((image): image is ImageEditorItem => !image.id)
  )

  const activeLinkItems = computed(() =>
    linkItems.value.filter(item => !item.isRemoved)
  )

  const removedLinkItems = computed(() =>
    linkItems.value.filter(item => item.isRemoved)
  )

  const newLinkItems = computed(() =>
    activeLinkItems.value.filter((link): link is LinkEditorItem =>
      !link.id &&
      link.text.trim().length > 0 &&
      isLikelyValidHttpUrl(link.url)
    )
  )

  const deleteImageIds = computed(() =>
    removedImageItems.value
      .map(item => item.id)
      .filter((id): id is string => Boolean(id))
  )

  const createProjectInput = computed<CreateProjectInput | null>(() => {
    return {
      title: projectDetailsModel.title,
      summary: projectDetailsModel.summary,
      body: projectDetailsModel.body,
      status: projectDetailsModel.status,
      links: activeLinkItems.value
        .filter((i): i is LinkEditorItem =>
          i.text.trim().length > 0 && isLikelyValidHttpUrl(i.url)
        )
        .map((i): CreateProjectLinkInput => ({
          linkText: i.text,
          linkType: i.type,
          sortOrder: i.sort,
          url: i.url
        }))
    }
  })

  const editProjectInput = computed<EditProjectInput | null>(() => {
    if (!projectId.value) return null

    return {
      id: projectId.value,
      title: projectDetailsModel.title,
      summary: projectDetailsModel.summary,
      body: projectDetailsModel.body,
      status: projectDetailsModel.status,
      images: activeImageItems.value
        .map((i) => ({
          projectImageId: i.id,
          altText: i.altText,
          sortOrder: i.sort
        }))
        .filter((image): image is EditProjectImageInput =>
          image.projectImageId != null
        ),
      links: activeLinkItems.value
        .filter((i): i is LinkEditorItem =>
          i.text.trim().length > 0 && isLikelyValidHttpUrl(i.url)
        )
        .map((i): EditProjectLinkInput => ({
          id: i.id ?? null,
          linkText: i.text,
          linkType: i.type,
          sortOrder: i.sort,
          url: i.url
        }))
    }
  })

  const hasExistingImageUpdates = computed(() =>
    checkIfEditorItemsUpdated(
      originalImageItems.value,
      activeImageItems.value,
      item => ({
        id: item.id!,
        altText: item.altText,
        sort: item.sort
      })
    )
  )

  const hasExistingLinkUpdates = computed(() =>
    checkIfEditorItemsUpdated(
      originalLinkItems.value,
      activeLinkItems.value,
      item => ({
        id: item.id!,
        text: item.text,
        url: item.url,
        type: item.type,
        sort: item.sort
      })
    )
  )

  const hasTagUpdates = computed(() => {
    const hasPending = tagItems.value.some(t => !t.id)
    const currentIds = tagItems.value.filter(t => t.id).map(t => t.id as string).sort().join(',')
    const original = [...originalTagIds.value].sort().join(',')
    return hasPending || currentIds !== original
  })

  const hasFieldUpdates = computed(() => {
    if (!originalProject.value) {
      return (
        projectDetailsModel.title.trim().length > 0 ||
        projectDetailsModel.summary.trim().length > 0 ||
        projectDetailsModel.body.trim().length > 0 ||
        projectDetailsModel.status !== ProjectStatus.Draft
      )
    }

    return (
      projectDetailsModel.title !== originalProject.value.title ||
      projectDetailsModel.summary !== originalProject.value.summary ||
      projectDetailsModel.body !== originalProject.value.body ||
      projectDetailsModel.status !== originalProject.value.status
    )
  })

  const hasUpdates = computed(() => {
    if (isNewProject.value) {
      return (
        hasFieldUpdates.value ||
        uploadItems.value.length > 0 ||
        newLinkItems.value.length > 0 ||
        tagItems.value.length > 0
      )
    }

    if (!project.value || !originalProject.value) return false

    return (
      hasFieldUpdates.value ||
      uploadItems.value.length > 0 ||
      removedImageItems.value.length > 0 ||
      hasExistingImageUpdates.value ||
      hasExistingLinkUpdates.value ||
      newLinkItems.value.length > 0 ||
      hasTagUpdates.value
    )
  })

  const syncFromProject = (
    currentProject: ProjectFragment
  ) => {
    originalProject.value = currentProject

    projectDetailsModel.title = currentProject.title ?? ''
    projectDetailsModel.summary = currentProject.summary ?? ''
    projectDetailsModel.body = currentProject.body ?? ''
    projectDetailsModel.status = currentProject.status ?? ProjectStatus.Draft

    const projectImageFragments = useFragment(
      ProjectImageFragmentDoc,
      currentProject.images
    )

    const mappedImageItems = normalizeEditorItemsSortOrder(
      projectImageFragments
        .map(imageFragmentToEditorItem)
        .sort((a, b) => a.sort - b.sort)
    )

    imageItems.value = mappedImageItems
    originalImageItems.value = mappedImageItems.map(item => ({ ...item }))

    const projectLinkFragments = useFragment(
      ProjectLinkFragmentDoc,
      currentProject.links
    )

    const mappedLinkItems = normalizeEditorItemsSortOrder(
      projectLinkFragments
        .map(linkFragmentToEditorItem)
        .sort((a, b) => a.sort - b.sort)
    )

    linkItems.value = mappedLinkItems
    originalLinkItems.value = mappedLinkItems.map(item => ({ ...item }))

    tagItems.value = useFragment(ProjectTagFragmentDoc, currentProject.tags)
      .map(t => ({ id: t.id, name: t.name, value: t.value }))
    originalTagIds.value = tagItems.value.map(t => t.id as string)
  }

  const refreshProject = async () => {
    if (!projectId.value) return false

    try {
      const result = await executeQuery({
        requestPolicy: 'network-only'
      })

      const refreshedProjectRef = result.data?.value?.projectById

      if (!refreshedProjectRef) {
        showSnackbar('Project saved, but failed to refresh project data.', 'warning')
        return false
      }

      const refreshedProject = useFragment(ProjectFragmentDoc, refreshedProjectRef)
      syncFromProject(refreshedProject)

      return true
    } catch (error) {
      console.error('Failed to refresh project', error)
      showSnackbar('Failed to refresh project', 'error')

      return false
    }
  }

  const deleteRemovedImages = async () => {
    if (!projectId.value || deleteImageIds.value.length === 0) return true

    try {
      await deleteImageUploads({
        projectId: projectId.value,
        projectImageIds: deleteImageIds.value
      })

      return true
    } catch (error) {
      console.error('Failed to delete removed images', error)
      showSnackbar(`Failed to delete remove ${deleteImageIds.value.length} images`, 'error')

      return false
    }
  }

  const resolvePendingTags = async (): Promise<boolean> => {
    const pending = tagItems.value.filter(t => !t.id)
    if (pending.length === 0) return true

    try {
      const resolved = await createProjectTags(pending)

      tagItems.value = tagItems.value.map(t => {
        if (t.id) return t
        const match = resolved.find(r => r.value === t.value)
        return match ? { id: match.id, name: match.name, value: match.value } : t
      })

      return true
    } catch (error) {
      console.error('Failed to create new tags', error)
      showSnackbar('Failed to create new tags', 'error')
      return false
    }
  }

  const submitEditProject = async () => {
    if (!projectId.value || !editProjectInput.value || !hasUpdates.value) return false

    try {
      await editProject(editProjectInput.value)

      if (uploadItems.value.length > 0) {
        await uploadImages({
          uploadItems: uploadItems.value,
          projectId: projectId.value
        })
      }

      const imagesDeleted = await deleteRemovedImages()
      if (!imagesDeleted) return false

      if (hasTagUpdates.value) {
        const tagsResolved = await resolvePendingTags()
        if (!tagsResolved) return false

        await updateProjectTags(projectId.value, tagItems.value)
      }

      const projectRefreshed = await refreshProject()
      if (!projectRefreshed) return false

      showSnackbar('Project saved.', 'success')
      return true
    } catch (error) {
      console.error('Failed to edit project', error)
      showSnackbar('Failed to save project', 'error')
      return false
    }
  }

  const submitCreateProject = async () => {
    if (!hasUpdates.value || !createProjectInput.value) return false

    try {
      const result = await createProject(createProjectInput.value)
      const newProjectId = result?.id

      if (!newProjectId) {
        console.error('Project created but no project ID was returned.', result)
        showSnackbar('Project created, but failed to retrieve the new project ID.', 'error')

        return false
      }

      if (uploadItems.value.length > 0) {
        await uploadImages({
          uploadItems: uploadItems.value,
          projectId: newProjectId
        })
      }

      if (tagItems.value.length > 0) {
        const tagsResolved = await resolvePendingTags()
        if (!tagsResolved) return false

        await updateProjectTags(newProjectId, tagItems.value)
      }

      await router.push(`/projects/${newProjectId}`)

      showSnackbar('Project created.', 'success')

      return true
    } catch (error) {
      console.error('Failed to create project', error)
      showSnackbar('Failed to create project', 'error')

      return false
    }
  }

  const submitProject = async () => {
    try {
      if (isNewProject.value) {
        await submitCreateProject()
        return
      }

      await submitEditProject()
    } catch (error) {
      console.error('Failed to save project', error)
      showSnackbar('Failed to save project', 'error')
    }
  }

  const restoreImageItem = (clientId: string) => {
    imageItems.value = restoreEditorItem(clientId, imageItems.value)
  }

  const restoreLinkItem = (clientId: string) => {
    linkItems.value = restoreEditorItem(clientId, linkItems.value)
  }

  watch(
    project,
    (currentProject) => {
      if (!currentProject || hasInitialized.value) return

      syncFromProject(currentProject)
      hasInitialized.value = true
    },
    { immediate: true }
  )

  return {
    // refs
    projectDetailsModel,
    imageItems,
    linkItems,
    tagItems,

    // computed
    activeImageItems,
    activeLinkItems,
    removedImageItems,
    removedLinkItems,
    isNewProject,
    isSavingProject,
    hasUpdates,
    isInitialLoading,
    projectId,
    projectLinksIsValid,

    // methods
    restoreImageItem,
    restoreLinkItem,
    submitProject
  }
}
