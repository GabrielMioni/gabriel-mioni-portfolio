import type { ProjectStatus } from '~/generated/graphql'

export type ProjectForm = {
  id: string
  title: string
  summary: string
  body: string
  status: ProjectStatus
}
