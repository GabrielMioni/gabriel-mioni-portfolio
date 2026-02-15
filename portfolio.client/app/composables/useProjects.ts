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
      skip: input.skip || 0,
      take: input.take || 5,
      includeUnpublished: input.includeUnpublished || true,
      where: input.where,
      order: input.order || [{ createdAt: SortEnumType.Desc }]
    }
  })

  const projects = computed(() =>
    data ? data?.value?.projects?.items : []
  )

  const pageInfo = computed(() =>
    data ? data?.value?.projects?.pageInfo : null
  )

  const totalCount = computed(() =>
    data ? data?.value?.projects?.totalCount : 0
  )

  return {
    projects,
    pageInfo,
    totalCount,
    fetchingProjects,
    projectError
  }
}
