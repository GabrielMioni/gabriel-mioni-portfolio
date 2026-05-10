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
  ProjectStatus
} from '~/generated/graphql'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
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
    // TODO: add createProject
    // createProject
  } = useProjectMutations()

  const {
    isProcessingImages,
    deleteImageUploads,
    uploadImages
  } = useProjectImageMutations()

  const {
    data,
    fetching,
    executeQuery
    // TODO: handle error
    // error
  } = useQuery({
    query: GetProjectByIdDocument,
    variables: computed(() => ({
      id: projectId.value ?? ''
    })),
    pause: computed(() => !projectId.value)
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

  const imageItems = ref<ImageEditorItem[]>([])
  const linkItems = ref<LinkEditorItem[]>([])

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
        newLinkItems.value.length > 0
      )
    }

    if (!project.value || !originalProject.value) return false

    return (
      hasFieldUpdates.value ||
      uploadItems.value.length > 0 ||
      removedImageItems.value.length > 0 ||
      hasExistingImageUpdates.value ||
      hasExistingLinkUpdates.value ||
      newLinkItems.value.length > 0
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
  }

  const refreshProject = async () => {
    if (!projectId.value) return

    const result = await executeQuery({
      requestPolicy: 'network-only'
    })

    const refreshedProjectRef = result.data?.value?.projectById

    if (!refreshedProjectRef) return

    const refreshedProject = useFragment(ProjectFragmentDoc, refreshedProjectRef)
    syncFromProject(refreshedProject)
  }

  const deleteRemovedImages = async () => {
    if (!projectId.value || deleteImageIds.value.length === 0) return

    await deleteImageUploads({
      projectId: projectId.value,
      projectImageIds: deleteImageIds.value
    })
  }

  const submitEditProject = async () => {
    if (!projectId.value || !editProjectInput.value || !hasUpdates.value) return

    await editProject(editProjectInput.value)

    if (uploadItems.value.length > 0) {
      await uploadImages({
        uploadItems: uploadItems.value,
        projectId: projectId.value
      })
    }

    await deleteRemovedImages()
    await refreshProject()
  }

  const submitCreateProject = async () => {
    try {
      if (!hasUpdates.value || !createProjectInput.value) return
      const result = await createProject(createProjectInput.value)

      const newProjectId = result?.id

      if (!newProjectId) {
        console.error('Failed to retrieve new project ID after creation')
      }

      if (uploadItems.value.length > 0 && newProjectId) {
        await uploadImages({
          uploadItems: uploadItems.value,
          projectId: newProjectId
        })
      }

      await router.push(`/projects/${newProjectId}`)
    } catch (error) {
      console.error('Failed to create project', error)
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
    projectId,
    project,
    isExistingProject,
    isNewProject,

    projectDetailsModel,
    imageItems,
    linkItems,

    projectLinksIsValid,

    activeImageItems,
    removedImageItems,
    uploadItems,

    activeLinkItems,
    removedLinkItems,
    newLinkItems,

    deleteImageIds,

    fetching,
    isInitialLoading,
    isSavingProject,

    hasFieldUpdates,
    hasExistingImageUpdates,
    hasExistingLinkUpdates,
    hasUpdates,

    editProjectInput,

    refreshProject,
    submitProject,
    submitEditProject,
    submitCreateProject,

    restoreImageItem,
    restoreLinkItem
  }
}
