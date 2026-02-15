import type { SortBy } from '~/types/ui/datatable'
import {
  type ProjectSortInput,
  SortEnumType
} from '~/generated/graphql'

const toSortEnum = (order: unknown): SortEnumType => {
  if (typeof order === 'boolean') return order ? SortEnumType.Asc : SortEnumType.Desc
  if (typeof order === 'string') return order.toLowerCase() === 'desc' ? SortEnumType.Desc : SortEnumType.Asc
  return SortEnumType.Asc
}

export const toGraphqlSort = (sortBy: SortBy): ProjectSortInput[] =>
  (sortBy ?? []).map(s => ({
    [s.key as keyof ProjectSortInput]: toSortEnum(s.order)
  }))
