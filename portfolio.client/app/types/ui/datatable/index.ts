import type { VDataTable } from 'vuetify/components'

export type SortBy = InstanceType<typeof VDataTable>['$props']['sortBy']
export type GroupBy = InstanceType<typeof VDataTable>['$props']['groupBy']

export interface UpdateOptions {
  page: number
  itemsPerPage: number
  sortBy: SortBy
  groupBy: GroupBy
}
