export type ImageUploadItem = {
  id?: string | null
  clientId: string
  contentType: string
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
