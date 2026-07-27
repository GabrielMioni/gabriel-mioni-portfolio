import type {
  HasSort,
  BaseEditorItem
} from '~/types/editor-items'

export const normalizeEditorItemsSortOrder = <T extends HasSort>(
  items: T[]
): T[] => {
  return items.map((item, index) => ({
    ...item,
    sort: index
  }))
}

const setEditorItemRemovedState = <T extends BaseEditorItem>(
  clientId: string,
  items: T[],
  isRemoved: boolean
): T[] => {
  const index = items.findIndex(item => item.clientId === clientId)

  if (index < 0) {
    throw new Error(`Item with clientId ${clientId} not found`)
  }

  const item = items[index]!

  // If the item doesn't have an id, it means it's not saved yet. Just remove it from the list
  const nextItems = item.id
    ? items.map(currentItem =>
      currentItem.clientId === clientId
        ? { ...currentItem, isRemoved }
        : currentItem
    )
    : items.filter(currentItem => currentItem.clientId !== clientId)

  return normalizeEditorItemsSortOrder(nextItems)
}

export const removeEditorItem = <T extends BaseEditorItem> (
  clientId: string,
  items: T[]
): T[] => {
  return setEditorItemRemovedState(clientId, items, true)
}

export const restoreEditorItem = <T extends BaseEditorItem>
  (
    clientId: string,
    items: T[]
  ): T[] => {
  return setEditorItemRemovedState(clientId, items, false)
}
