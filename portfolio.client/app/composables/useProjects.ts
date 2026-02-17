import { useQuery } from '@urql/vue'
import {
  type Project,
  type ProjectQueryProjectsArgs,
  // SortEnumType,
  GetProjectsDocument
} from '~/generated/graphql'

export const useProjects = (input: Ref<ProjectQueryProjectsArgs>) => {
  const variables = computed(() => ({
    skip: input.value.skip ?? 0,
    take: input.value.take ?? 5,
    includeUnpublished: input.value.includeUnpublished ?? true,
    // order: input.value.order ?? [{ createdAt: SortEnumType.Desc }],
    order: input.value.order ?? [],
    where: input.value.where
  }))

  const {
    data,
    fetching: fetchingProjects,
    error: projectError
  } = useQuery({
    query: GetProjectsDocument,
    variables
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
