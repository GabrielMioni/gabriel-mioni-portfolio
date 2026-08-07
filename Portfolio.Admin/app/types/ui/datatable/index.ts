import type { VDataTableServer } from 'vuetify/components'

export const TABLE_ITEMS_PER_PAGE_OPTIONS = [10, 25, 50]
export const MAX_TABLE_ITEMS_PER_PAGE = 50

type Props = InstanceType<typeof VDataTableServer>['$props']

export type SortBy = Props['sortBy']
export type GroupBy = Props['groupBy']
export type Search = Props['search']

export type Header<K extends string = string> = {
  title: string
  key: K
  sortable?: boolean
  align?: 'start' | 'center' | 'end'
  width?: number | string
}

export type TableOptions = {
  page: number
  itemsPerPage: number
  search?: Search
  sortBy: SortBy
  groupBy: GroupBy
}
