import { useQuery } from '@urql/vue'
import {
  type ProjectQueryProjectsArgs,
  GetProjectsDocument,
  SortEnumType
} from '~/generated/graphql'

export const useProjects = (input: ProjectQueryProjectsArgs) => {
  const {
    data,
    fetching: fetchingProjects,
    error: projectError
  } = useQuery({
    query: GetProjectsDocument,
    variables: {
      includeUnpublished: input.includeUnpublished ?? true,
      first: input.first,
      after: input.after,
      before: input.before,
      order: [{ createdAt: SortEnumType.Desc }]
    }
  })

  const projects = computed(() =>
    data ? data?.value?.projects?.nodes : []
  )

  const pageInfo = computed(() =>
    data ? data?.value?.projects?.pageInfo : null
  )

  return {
    projects,
    pageInfo,
    fetchingProjects,
    projectError
  }
}
