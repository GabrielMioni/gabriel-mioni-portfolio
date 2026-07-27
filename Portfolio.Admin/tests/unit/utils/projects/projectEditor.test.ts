import { describe, expect, it } from 'vitest'
import {
  cloneProjectEditorDraft,
  createEmptyProjectEditorDraft,
  isProjectEditorDraftDirty
} from '~/utils/projects/projectEditor'
import { ProjectStatus } from '~/generated/graphql'

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
})
