import type { ProjectStatus } from '~/generated/graphql'

export type ProjectBaseForm = {
  title: string
  summary: string
  body: string
  status: ProjectStatus
}

export type ProjectForm = ProjectBaseForm & {
  id: string
}
