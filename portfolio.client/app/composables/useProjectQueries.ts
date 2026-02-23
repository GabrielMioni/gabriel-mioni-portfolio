import { useQuery } from '@urql/vue'
import {
  type GetProjectsQueryVariables,
  type Project,
  GetProjectsDocument
} from '~/generated/graphql'

export const useProjectQueries = (variables: Ref<GetProjectsQueryVariables>) => {
  const {
    data,
    fetching: fetchingProjects,
    error: projectError
  } = useQuery({
    query: GetProjectsDocument,
    variables,
    requestPolicy: 'cache-and-network'
  })

  const projects = computed(() =>
    (data.value?.projects?.items ?? []).filter(
      (p): p is Project => p != null
    )
  )

  const pageInfo = computed(() =>
    data.value?.projects?.pageInfo ?? {
      hasNextPage: false,
      hasPreviousPage: false
    }
  )

  const totalCount = computed(() =>
    data?.value?.projects?.totalCount ?? 0
  )

  return {
    projects,
    pageInfo,
    totalCount,
    fetchingProjects,
    projectError
  }
}
