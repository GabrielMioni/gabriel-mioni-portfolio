import type { SortBy } from '~/types/ui/datatable'
import {
  type ProjectSortInput,
  type ProjectFilterInput,
  type InputMaybe,
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

export const toGraphqlFilterInput = (
  search: string | null | undefined
): InputMaybe<ProjectFilterInput> => {
  const trimmed = search?.trim()
  if (!trimmed) return undefined

  return {
    or: [
      { title:   { contains: trimmed } },
      { summary: { contains: trimmed } },
      { body:    { contains: trimmed } }
    ]
  }
}


