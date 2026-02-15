import { useQuery } from '@urql/vue'
import {
  type Project,
  type ProjectQueryProjectsArgs,
  SortEnumType,
  GetProjectsDocument
} from '~/generated/graphql'

export const useProjects = (input: ProjectQueryProjectsArgs) => {
  const {
    data,
    fetching: fetchingProjects,
    error: projectError
  } = useQuery({
    query: GetProjectsDocument,
    variables: {
      skip: input.skip ?? 0,
      take: input.take ?? 5,
      includeUnpublished: input.includeUnpublished ?? true,
      order: input.order ?? [{ createdAt: SortEnumType.Desc }],
      where: input.where
    }
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
