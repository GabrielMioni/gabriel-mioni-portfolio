type HasClientId = {
  clientId: string
}

type HasId = {
  id?: string | null
}

type HasSort = {
  sort: number
}

export const findEditorItemAndIndexByClientId = <T extends HasClientId>(
  clientId: string,
  items: T[]
): { item: T, index: number } | null => {
  const index = items.findIndex(item => item.clientId === clientId)

  if (index < 0) return null

  return {
    item: items[index]!,
    index
  }
}

export const checkIfEditorItemsUpdated = <
  T extends HasId & HasSort,
  M extends Record<string, unknown>
>(
    original: T[],
    updated: T[],
    mapItem: (item: T) => M
  ): boolean => {
  const currentExisting = original
    .filter((item): item is T & { id: string } => Boolean(item.id))
    .map(mapItem)
    .sort((a, b) => {
      const aSort = typeof a.sort === 'number' ? a.sort : 0
      const bSort = typeof b.sort === 'number' ? b.sort : 0
      return aSort - bSort
    })

  const updatedExisting = updated
    .filter((item): item is T & { id: string } => Boolean(item.id))
    .map(mapItem)
    .sort((a, b) => {
      const aSort = typeof a.sort === 'number' ? a.sort : 0
      const bSort = typeof b.sort === 'number' ? b.sort : 0
      return aSort - bSort
    })

  if (currentExisting.length !== updatedExisting.length) return true

  for (let i = 0; i < currentExisting.length; i++) {
    const currentItem = currentExisting[i]
    const updatedItem = updatedExisting[i]

    if (!currentItem || !updatedItem) return true

    const keys = Object.keys(currentItem) as (keyof M)[]
    for (const key of keys) {
      if (currentItem[key] !== updatedItem[key]) {
        return true
      }
    }
  }

  return false
}

export const normalizeEditorItemsSortOrder = <T extends { sort: number }>(
  items: T[]
): T[] => {
  return items.map((item, index) => ({
    ...item,
    sort: index
  }))
}

export const restoreEditorItem = <T extends { clientId: string, sort: number }>(
  clientId: string,
  removedItems: Ref<T[]>,
  activeItems: Ref<T[]>
) => {
  const result = findEditorItemAndIndexByClientId(clientId, removedItems.value)
  if (!result) return

  const { item, index } = result

  const nextRemovedItems = [...removedItems.value]
  nextRemovedItems.splice(index, 1)
  removedItems.value = nextRemovedItems

  activeItems.value.push({
    ...item,
    sort: activeItems.value.length
  })
}
