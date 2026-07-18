import { useFragment } from '~/generated'
import {
  type CreateProjectInput,
  type CreateProjectLinkInput,
  type EditProjectImageInput,
  type EditProjectInput,
  type EditProjectLinkInput,
  type ProjectFragment,
  ProjectImageFragmentDoc,
  ProjectLinkFragmentDoc,
  ProjectTagFragmentDoc,
  ProjectStatus
} from '~/generated/graphql'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import type { TagEditorItem } from '~/types/tags'
import type { ProjectBaseForm } from '~/types/ui/form'
import { normalizeEditorItemsSortOrder } from '~/utils/editorItems'
import { imageFragmentToEditorItem } from '~/utils/images/imageEditorItems'
import { linkFragmentToEditorItem } from '~/utils/links/linkEditorItems'

export type ProjectEditorDraft = {
  form: ProjectBaseForm
  imageItems: ImageEditorItem[]
  linkItems: LinkEditorItem[]
  tagItems: TagEditorItem[]
}

export const createEmptyProjectEditorDraft = (): ProjectEditorDraft => ({
  form: {
    title: '',
    summary: '',
    body: '',
    status: ProjectStatus.Draft
  },
  imageItems: [],
  linkItems: [],
  tagItems: []
})

export const cloneProjectEditorDraft = (
  draft: ProjectEditorDraft
): ProjectEditorDraft => ({
  form: { ...draft.form },
  imageItems: draft.imageItems.map(item => ({ ...item })),
  linkItems: draft.linkItems.map(item => ({ ...item })),
  tagItems: draft.tagItems.map(item => ({ ...item }))
})

export const projectFragmentToEditorDraft = (
  project: ProjectFragment
): ProjectEditorDraft => {
  const imageFragments = useFragment(ProjectImageFragmentDoc, project.images)
  const linkFragments = useFragment(ProjectLinkFragmentDoc, project.links)

  return {
    form: {
      title: project.title ?? '',
      summary: project.summary ?? '',
      body: project.body ?? '',
      status: project.status ?? ProjectStatus.Draft
    },
    imageItems: normalizeEditorItemsSortOrder(
      imageFragments
        .map(imageFragmentToEditorItem)
        .sort((a, b) => a.sort - b.sort)
    ),
    linkItems: normalizeEditorItemsSortOrder(
      linkFragments
        .map(linkFragmentToEditorItem)
        .sort((a, b) => a.sort - b.sort)
    ),
    tagItems: useFragment(ProjectTagFragmentDoc, project.tags)
      .map(tag => ({ id: tag.id, name: tag.name, value: tag.value }))
  }
}

const comparableDraft = (draft: ProjectEditorDraft) => ({
  form: {
    title: draft.form.title.trim(),
    summary: draft.form.summary.trim(),
    body: draft.form.body,
    status: draft.form.status
  },
  images: draft.imageItems
    .filter(item => !item.isRemoved)
    .map(item => ({
      key: item.id ?? item.clientId,
      altText: item.altText,
      sort: item.sort
    }))
    .sort((a, b) => a.sort - b.sort),
  links: draft.linkItems
    .filter(item => !item.isRemoved)
    .map(item => ({
      key: item.id ?? item.clientId,
      text: item.text,
      url: item.url,
      type: item.type,
      sort: item.sort
    }))
    .sort((a, b) => a.sort - b.sort),
  tags: draft.tagItems
    .map(tag => tag.id ?? tag.value ?? tag.name)
    .sort()
})

export const isProjectEditorDraftDirty = (
  draft: ProjectEditorDraft,
  baseline: ProjectEditorDraft
): boolean => (
  JSON.stringify(comparableDraft(draft)) !== JSON.stringify(comparableDraft(baseline))
)

export const buildCreateProjectInput = (
  draft: ProjectEditorDraft
): CreateProjectInput => ({
  title: draft.form.title,
  summary: draft.form.summary,
  body: draft.form.body,
  status: draft.form.status,
  links: draft.linkItems
    .filter(item => !item.isRemoved)
    .map((item): CreateProjectLinkInput => ({
      linkText: item.text,
      linkType: item.type,
      sortOrder: item.sort,
      url: item.url
    }))
})

export const buildEditProjectInput = (
  projectId: string,
  draft: ProjectEditorDraft
): EditProjectInput => ({
  id: projectId,
  title: draft.form.title,
  summary: draft.form.summary,
  body: draft.form.body,
  status: draft.form.status,
  images: draft.imageItems
    .filter(item => !item.isRemoved)
    .map(item => ({
      projectImageId: item.id,
      altText: item.altText,
      sortOrder: item.sort
    }))
    .filter((item): item is EditProjectImageInput => item.projectImageId != null),
  links: draft.linkItems
    .filter(item => !item.isRemoved)
    .map((item): EditProjectLinkInput => ({
      id: item.id ?? null,
      linkText: item.text,
      linkType: item.type,
      sortOrder: item.sort,
      url: item.url
    }))
})
