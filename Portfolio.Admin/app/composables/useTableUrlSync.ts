import type { ComputedRef } from 'vue'
import type { TableOptions } from '~/types/ui/datatable'

const getPositiveInt = (value: unknown, fallback: number): number => {
  if (typeof value !== 'string') return fallback
  const parsed = Number(value)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback
}

const getString = (value: unknown, fallback = ''): string =>
  typeof value === 'string' ? value : fallback

export const useTableUrlSync = (options?: {
  defaultItemsPerPage?: number
  defaultSort?: { key: string; order: 'asc' | 'desc' }
  extra?: ComputedRef<Record<string, string>>
}) => {
  const route = useRoute()
  const router = useRouter()

  const defaultItemsPerPage = options?.defaultItemsPerPage ?? 10
  const defaultSort = options?.defaultSort

  const getInitialOptions = (): TableOptions => ({
    page: getPositiveInt(route.query.page, 1),
    itemsPerPage: getPositiveInt(route.query.itemsPerPage, defaultItemsPerPage),
    sortBy: route.query.sortKey
      ? [{ key: getString(route.query.sortKey, defaultSort?.key ?? ''), order: getString(route.query.sortOrder, defaultSort?.order ?? 'asc') as 'asc' | 'desc' }]
      : defaultSort ? [defaultSort] : [],
    groupBy: [],
    search: getString(route.query.search).trim()
  })

  const tableOptions = ref<TableOptions>(getInitialOptions())
  const search = ref<string>(tableOptions.value.search ?? '')

  const syncRoute = async () => {
    const opts = tableOptions.value
    const next: Record<string, string> = {
      page: String(opts.page),
      itemsPerPage: String(opts.itemsPerPage)
    }

    const sort = opts.sortBy?.[0]
    if (sort) {
      next.sortKey = sort.key
      next.sortOrder = sort.order === 'desc' ? 'desc' : 'asc'
    }

    const trimmed = opts.search?.trim() ?? ''
    if (trimmed) next.search = trimmed

    Object.assign(next, options?.extra?.value ?? {})

    const q = route.query
    const same = Object.keys(next).length === Object.keys(q).length &&
      Object.keys(next).every(k => next[k] === String(q[k] ?? ''))

    if (same) return
    await router.replace({ query: next })
  }

  const updateTableOptions = async (opts: TableOptions) => {
    tableOptions.value = { ...tableOptions.value, ...opts, search: tableOptions.value.search }
    await syncRoute()
  }

  watchDebounced(
    search,
    async (val) => {
      tableOptions.value = { ...tableOptions.value, search: val.trim(), page: 1 }
      await syncRoute()
    },
    { debounce: 350, maxWait: 1000 }
  )

  if (options?.extra) {
    watch(options.extra, syncRoute)
  }

  return {
    tableOptions,
    search,
    updateTableOptions
  }
}
