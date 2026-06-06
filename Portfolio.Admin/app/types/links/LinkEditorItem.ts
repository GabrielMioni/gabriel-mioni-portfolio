import type { ProjectLinkType } from '~/generated/graphql'
import type { BaseEditorItem } from '~/types/editor-items'

export type LinkEditorItem = BaseEditorItem & {
  createdAt?: string | null
  url: string
  text: string
  type: ProjectLinkType
}
