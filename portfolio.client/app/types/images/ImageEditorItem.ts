export type ImageEditorItem = {
  id?: string | null
  clientId: string
  contentType: string
  createdAt?: string | null
  fileName?: string | null
  fullKey?: string | null
  thumbKey?: string | null
  sort: number
  sizeThumb: number
  sizeFull: number
  altText: string
  thumbFile?: Blob | null
  fullFile?: Blob | null
  height: number
  width: number
}
