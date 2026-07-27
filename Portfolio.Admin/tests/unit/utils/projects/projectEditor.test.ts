import { describe, expect, it } from 'vitest'
import {
  cloneProjectEditorDraft,
  createEmptyProjectEditorDraft,
  isProjectEditorDraftDirty
} from '~/utils/projects/projectEditor'
import {
  normalizeEditorItemsSortOrder,
  removeEditorItem
} from '~/utils/editorItems'
import {
  ProjectLinkType,
  ProjectStatus
} from '~/generated/graphql'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import type { TagEditorItem } from '~/types/tags'

const createImageEditorItem = (
  overrides: Partial<ImageEditorItem> = {}
): ImageEditorItem => ({
  id: null,
  clientId: 'image-client-id',
  isRemoved: false,
  sort: 0,
  contentType: 'image/jpeg',
  fileName: 'portfolio-project.jpg',
  sizeThumb: 20_000,
  sizeFull: 120_000,
  altText: 'Portfolio project',
  height: 800,
  width: 1_200,
  ...overrides
})

const createLinkEditorItem = (
  overrides: Partial<LinkEditorItem> = {}
): LinkEditorItem => ({
  id: null,
  clientId: 'link-client-id',
  isRemoved: false,
  sort: 0,
  url: 'https://example.com',
  text: 'Example',
  type: ProjectLinkType.External,
  ...overrides
})

const createTagEditorItem = (
  overrides: Partial<TagEditorItem> = {}
): TagEditorItem => ({
  id: null,
  name: 'Design',
  value: 'design',
  ...overrides
})

describe('createEmptyProjectEditorDraft', () => {
  it('returns the expected default draft', () => {
    expect(createEmptyProjectEditorDraft()).toEqual({
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
  })
})

describe('cloneProjectEditorDraft', () => {
  it('returns an equal draft with independent references', () => {
    const source = createEmptyProjectEditorDraft()
    source.form.title = 'Portfolio project'
    source.imageItems.push(createImageEditorItem({ id: 'image-id' }))
    source.linkItems.push(createLinkEditorItem({ id: 'link-id' }))
    source.tagItems.push(createTagEditorItem({ id: 'tag-id' }))

    const clone = cloneProjectEditorDraft(source)

    expect(clone).toEqual(source)
    expect(clone).not.toBe(source)
    expect(clone.form).not.toBe(source.form)
    expect(clone.imageItems).not.toBe(source.imageItems)
    expect(clone.imageItems[0]).not.toBe(source.imageItems[0])
    expect(clone.linkItems).not.toBe(source.linkItems)
    expect(clone.linkItems[0]).not.toBe(source.linkItems[0])
    expect(clone.tagItems).not.toBe(source.tagItems)
    expect(clone.tagItems[0]).not.toBe(source.tagItems[0])
  })

  it('does not change the source when the clone is mutated', () => {
    const source = createEmptyProjectEditorDraft()
    source.form.title = 'Original title'
    source.imageItems.push(createImageEditorItem({
      id: 'image-id',
      altText: 'Original alt text'
    }))
    source.linkItems.push(createLinkEditorItem({
      id: 'link-id',
      text: 'Original link text'
    }))
    source.tagItems.push(createTagEditorItem({
      id: 'tag-id',
      name: 'Original tag name'
    }))

    const clone = cloneProjectEditorDraft(source)
    clone.form.title = 'Updated title'
    clone.imageItems[0]!.altText = 'Updated alt text'
    clone.linkItems[0]!.text = 'Updated link text'
    clone.tagItems[0]!.name = 'Updated tag name'

    expect(source.form.title).toBe('Original title')
    expect(source.imageItems[0]!.altText).toBe('Original alt text')
    expect(source.linkItems[0]!.text).toBe('Original link text')
    expect(source.tagItems[0]!.name).toBe('Original tag name')
  })
})

describe('isProjectEditorDraftDirty', () => {
  it('returns false when the draft matches the baseline', () => {
    const baseline = createEmptyProjectEditorDraft()
    const draft = cloneProjectEditorDraft(baseline)

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(false)
  })

  it('returns false when the title and summary differ only by surrounding whitespace', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.form.title = 'Portfolio project'
    baseline.form.summary = 'Project summary'

    const draft = cloneProjectEditorDraft(baseline)
    draft.form.title = '  Portfolio project  '
    draft.form.summary = '  Project summary  '

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(false)
  })

  it.each([
    { field: 'title', value: 'Updated title' },
    { field: 'summary', value: 'Updated summary' }
  ] as const)('returns true when $field changes', ({ field, value }) => {
    const baseline = createEmptyProjectEditorDraft()
    const draft = cloneProjectEditorDraft(baseline)

    draft.form[field] = value

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('returns true when body whitespace changes', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.form.body = 'Project body'

    const draft = cloneProjectEditorDraft(baseline)
    draft.form.body = 'Project body '

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('returns true when the body changes', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.form.body = 'Project body'

    const draft = cloneProjectEditorDraft(baseline)
    draft.form.body = 'Updated project body'

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('returns true when the status changes', () => {
    const baseline = createEmptyProjectEditorDraft()

    const draft = cloneProjectEditorDraft(baseline)
    draft.form.status = ProjectStatus.Published

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('returns true when an unsaved image is added', () => {
    const baseline = createEmptyProjectEditorDraft()
    const draft = cloneProjectEditorDraft(baseline)

    draft.imageItems.push(createImageEditorItem())

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('returns true when an existing image is removed', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.imageItems.push(createImageEditorItem({ id: 'existing-image-id' }))

    const draft = cloneProjectEditorDraft(baseline)
    draft.imageItems[0]!.isRemoved = true

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('returns true when image alt text changes', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.imageItems.push(createImageEditorItem({
      id: 'existing-image-id',
      altText: 'Unchanged alt text'
    }))

    const draft = cloneProjectEditorDraft(baseline)
    draft.imageItems[0]!.altText = 'Updated alt text'

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('returns true when images are reordered', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.imageItems.push(
      createImageEditorItem({
        id: 'first-image-id',
        clientId: 'first-image-client-id',
        sort: 0
      }),
      createImageEditorItem({
        id: 'second-image-id',
        clientId: 'second-image-client-id',
        sort: 1
      })
    )

    const draft = cloneProjectEditorDraft(baseline)
    draft.imageItems = normalizeEditorItemsSortOrder([
      draft.imageItems[1]!,
      draft.imageItems[0]!
    ])

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('returns false when only the image array order changes', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.imageItems.push(
      createImageEditorItem({
        id: 'first-image-id',
        clientId: 'first-image-client-id',
        sort: 0
      }),
      createImageEditorItem({
        id: 'second-image-id',
        clientId: 'second-image-client-id',
        sort: 1
      })
    )

    const draft = cloneProjectEditorDraft(baseline)
    draft.imageItems.reverse()

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(false)
  })

  it('returns false when an unsaved image is added and then removed', () => {
    const baseline = createEmptyProjectEditorDraft()
    const draft = cloneProjectEditorDraft(baseline)
    const unsavedImage = createImageEditorItem()

    draft.imageItems.push(unsavedImage)
    draft.imageItems = removeEditorItem(
      unsavedImage.clientId,
      draft.imageItems
    )

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(false)
  })

  it('returns true when an unsaved link is added', () => {
    const baseline = createEmptyProjectEditorDraft()
    const draft = cloneProjectEditorDraft(baseline)

    draft.linkItems.push(createLinkEditorItem())

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('returns true when an existing link is removed', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.linkItems.push(createLinkEditorItem({ id: 'existing-link-id' }))

    const draft = cloneProjectEditorDraft(baseline)
    draft.linkItems[0]!.isRemoved = true

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it.each([
    {
      field: 'text',
      update: (link: LinkEditorItem) => {
        link.text = 'Updated text'
      }
    },
    {
      field: 'URL',
      update: (link: LinkEditorItem) => {
        link.url = 'https://updated.example.com'
      }
    },
    {
      field: 'type',
      update: (link: LinkEditorItem) => {
        link.type = ProjectLinkType.Repository
      }
    }
  ])('returns true when link $field changes', ({ update }) => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.linkItems.push(createLinkEditorItem({ id: 'existing-link-id' }))

    const draft = cloneProjectEditorDraft(baseline)
    update(draft.linkItems[0]!)

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('returns true when links are reordered', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.linkItems.push(
      createLinkEditorItem({
        id: 'first-link-id',
        clientId: 'first-link-client-id',
        sort: 0
      }),
      createLinkEditorItem({
        id: 'second-link-id',
        clientId: 'second-link-client-id',
        sort: 1
      })
    )

    const draft = cloneProjectEditorDraft(baseline)
    draft.linkItems = normalizeEditorItemsSortOrder([
      draft.linkItems[1]!,
      draft.linkItems[0]!
    ])

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('returns false when only the link array order changes', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.linkItems.push(
      createLinkEditorItem({
        id: 'first-link-id',
        clientId: 'first-link-client-id',
        sort: 0
      }),
      createLinkEditorItem({
        id: 'second-link-id',
        clientId: 'second-link-client-id',
        sort: 1
      })
    )

    const draft = cloneProjectEditorDraft(baseline)
    draft.linkItems.reverse()

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(false)
  })

  it('returns false when an unsaved link is added and then removed', () => {
    const baseline = createEmptyProjectEditorDraft()
    const draft = cloneProjectEditorDraft(baseline)
    const unsavedLink = createLinkEditorItem()

    draft.linkItems.push(unsavedLink)
    draft.linkItems = removeEditorItem(
      unsavedLink.clientId,
      draft.linkItems
    )

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(false)
  })

  it('returns true when a tag is assigned', () => {
    const baseline = createEmptyProjectEditorDraft()
    const draft = cloneProjectEditorDraft(baseline)

    draft.tagItems.push(createTagEditorItem())

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('returns true when a tag is unassigned', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.tagItems.push(createTagEditorItem({ id: 'existing-tag-id' }))

    const draft = cloneProjectEditorDraft(baseline)
    draft.tagItems = []

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('returns false when only the tag array order changes', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.tagItems.push(
      createTagEditorItem({
        id: 'first-tag-id',
        name: 'Design',
        value: 'design'
      }),
      createTagEditorItem({
        id: 'second-tag-id',
        name: 'Development',
        value: 'development'
      })
    )

    const draft = cloneProjectEditorDraft(baseline)
    draft.tagItems.reverse()

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(false)
  })
})
