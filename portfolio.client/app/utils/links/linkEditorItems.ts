import type { ProjectLinkFragment } from '~/generated/graphql'
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'

export const linkFragmentToEditorItem = (
  linkFragment: ProjectLinkFragment
): LinkEditorItem => {
  return {
    id: linkFragment.id,
    clientId: crypto.randomUUID(),
    createdAt: linkFragment.createdAt,
    url: linkFragment.url,
    text: linkFragment.linkText,
    type: linkFragment.linkType,
    sort: linkFragment.sortOrder
  }
}
