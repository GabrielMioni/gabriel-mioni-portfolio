import type { VDataTableServer } from 'vuetify/components'

export type SortBy = InstanceType<typeof VDataTableServer>['$props']['sortBy']
export type GroupBy = InstanceType<typeof VDataTableServer>['$props']['groupBy']
export type Items = InstanceType<typeof VDataTableServer>['$props']['items']
type Search = InstanceType<typeof VDataTableServer>['$props']['search']

export interface TableOptions {
  page: number
  itemsPerPage: number
  search: Search,
  sortBy: SortBy
  groupBy: GroupBy
}
