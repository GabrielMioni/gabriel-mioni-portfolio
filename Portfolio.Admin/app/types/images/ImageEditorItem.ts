import type {
  BaseEditorItem
} from '~/types/editor-items'

export type ImageEditorItem = BaseEditorItem & {
  contentType: string
  createdAt?: string | null
  fileName?: string | null
  fullKey?: string | null
  thumbKey?: string | null
  sizeThumb: number
  sizeFull: number
  altText: string
  thumbFile?: Blob | null
  fullFile?: Blob | null
  height: number
  width: number
}
