import type { ProjectLinkType } from '~/generated/graphql'

export type LinkEditorItem = {
  id?: string | null
  clientId: string
  createdAt?: string | null
  url: string
  text: string
  type: ProjectLinkType
  sort: number
}
