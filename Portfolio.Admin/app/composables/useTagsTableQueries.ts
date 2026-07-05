import { useQuery } from '@urql/vue'
import {
  type GetTagSummariesQueryVariables,
  type ProjectTagSummarySortInput,
  type ProjectTagSummaryFilterInput,
  GetTagSummariesDocument,
  SortEnumType
} from '~/generated/graphql'
import type { SortBy } from '~/types/ui/datatable'

const toSortEnum = (order: unknown): SortEnumType => {
  if (typeof order === 'string') return order.toLowerCase() === 'desc' ? SortEnumType.Desc : SortEnumType.Asc
  if (typeof order === 'boolean') return order ? SortEnumType.Asc : SortEnumType.Desc
  return SortEnumType.Asc
}

export const toTagSortInput = (sortBy: SortBy): ProjectTagSummarySortInput[] =>
  (sortBy ?? []).map(s => ({ [s.key as keyof ProjectTagSummarySortInput]: toSortEnum(s.order) }))

export const toTagFilterInput = (search: string | undefined): ProjectTagSummaryFilterInput | undefined => {
  const trimmed = search?.trim()
  if (!trimmed) return undefined
  return {
    or: [
      { name: { contains: trimmed } },
      { value: { contains: trimmed } }
    ]
  }
}

export const useTagsTableQueries = (variables: Ref<GetTagSummariesQueryVariables>) => {
  const { data, fetching: fetchingTags, executeQuery: refetchTags } = useQuery({
    query: GetTagSummariesDocument,
    variables,
    requestPolicy: 'cache-and-network'
  })

  const tags = computed(() =>
    (data.value?.tagSummaries?.items ?? []).filter(t => t != null)
  )

  const totalCount = computed(() => data.value?.tagSummaries?.totalCount ?? 0)

  return { tags, totalCount, fetchingTags, refetchTags }
}
