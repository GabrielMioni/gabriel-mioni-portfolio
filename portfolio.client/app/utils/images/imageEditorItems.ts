import type { ImageEditorItem } from '~/types/images/ImageEditorItem'
import type { ProjectImageFragment, ProjectImagePrepareItemInput } from '~/generated/graphql'

export const imageFragmentToEditorItem = (
  imageFragment: ProjectImageFragment
): ImageEditorItem => {
  return {
    id: imageFragment.id,
    altText: imageFragment.altText ?? '',
    clientId: crypto.randomUUID(),
    contentType: imageFragment.contentType ?? 'unknown',
    createdAt: imageFragment.createdAt,
    height: imageFragment.height ?? -1,
    isRemoved: false,
    width: imageFragment.width ?? -1,
    fileName: null,
    fullKey: imageFragment.fullKey,
    thumbKey: imageFragment.thumbKey,
    sizeThumb: 0,
    sizeFull: imageFragment.sizeBytes ? parseInt(imageFragment.sizeBytes) : -1,
    sort: imageFragment.sortOrder
  }
}

export const imageEditorItemsToProjectImagePrepareItemInput = (
  uploadItems: ImageEditorItem[]
): ProjectImagePrepareItemInput[] => {
  return uploadItems
    .map((item): ProjectImagePrepareItemInput | null => {
      const fullFile = item.fullFile
      const thumbFile = item.thumbFile

      if (!fullFile || !thumbFile) return null

      return {
        altText: item.altText,
        clientId: item.clientId,
        fullContentType: fullFile.type,
        fullSizeBytes: fullFile.size,
        height: item.height,
        width: item.width,
        thumbContentType: thumbFile.type,
        thumbSizeBytes: thumbFile.size
      }
    })
    .filter((item): item is ProjectImagePrepareItemInput => item !== null)
}

