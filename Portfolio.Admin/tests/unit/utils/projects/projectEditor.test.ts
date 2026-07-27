import { describe, expect, it } from 'vitest'
import {
  cloneProjectEditorDraft,
  createEmptyProjectEditorDraft,
  isProjectEditorDraftDirty
} from '~/utils/projects/projectEditor'
import { normalizeEditorItemsSortOrder } from '~/utils/editorItems'
import { ProjectStatus } from '~/generated/graphql'
import type { ImageEditorItem } from '~/types/images/ImageEditorItem'

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

describe('isProjectEditorDraftDirty', () => {
  it('returns false when the draft matches the baseline', () => {
    const baseline = createEmptyProjectEditorDraft()
    const draft = cloneProjectEditorDraft(baseline)

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(false)
  })

  it('ignores surrounding whitespace in the title and summary', () => {
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
  ] as const)('treats updated $field as a meaningful change', ({ field, value }) => {
    const baseline = createEmptyProjectEditorDraft()
    const draft = cloneProjectEditorDraft(baseline)

    draft.form[field] = value

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('treats body whitespace as a meaningful change', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.form.body = 'Project body'

    const draft = cloneProjectEditorDraft(baseline)
    draft.form.body = 'Project body '

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('treats updated body as a meaningful change', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.form.body = 'Project body'

    const draft = cloneProjectEditorDraft(baseline)
    draft.form.body = 'Updated project body'

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('treats updated status as a meaningful change', () => {
    const baseline = createEmptyProjectEditorDraft()

    const draft = cloneProjectEditorDraft(baseline)
    draft.form.status = ProjectStatus.Published

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('treats adding an unsaved image as a meaningful change', () => {
    const baseline = createEmptyProjectEditorDraft()
    const draft = cloneProjectEditorDraft(baseline)

    draft.imageItems.push(createImageEditorItem())

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('treats removing an existing image as a meaningful change', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.imageItems.push(createImageEditorItem({ id: 'existing-image-id' }))

    const draft = cloneProjectEditorDraft(baseline)
    draft.imageItems[0]!.isRemoved = true

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('treats changes to image alt text as meaningful change', () => {
    const baseline = createEmptyProjectEditorDraft()
    baseline.imageItems.push(createImageEditorItem({
      id: 'existing-image-id',
      altText: 'Unchanged alt text'
    }))

    const draft = cloneProjectEditorDraft(baseline)
    draft.imageItems[0]!.altText = 'Updated alt text'

    expect(isProjectEditorDraftDirty(draft, baseline)).toBe(true)
  })

  it('treats reordering images as a meaningful change', () => {
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
})
