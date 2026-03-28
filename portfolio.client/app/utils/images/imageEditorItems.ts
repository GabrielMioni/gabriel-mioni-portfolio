import type { ImageEditorItem } from '~/types/images/ImageEditorItem'

export const findImageEditorItemAndIndexByClientId = (
  clientId: string,
  imageItems: ImageEditorItem[]
): { item: ImageEditorItem, index: number } | null => {
  const index = imageItems.findIndex(
    removedItem => removedItem.clientId === clientId
  )
  if (index === -1) return null

  const item = imageItems[index]
  if (!item) return null

  return {
    item,
    index
  }
}
