type HasClientId = {
  clientId: string
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
