import type { ProjectLinkType } from '~/generated/graphql'

export type LinkEditorItem = {
  id?: string | null
  clientId: string
  url: string
  text: string
  type: ProjectLinkType
  sort: number
}
