export type ImageUploadItem = {
  clientId: string,
  contentType: string,
  fileName: string,
  sizeThumb: number,
  sizeFull: number,
  altText: string,
  thumbFile: Blob | null,
  fullFile: Blob | null
}
