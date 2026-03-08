export type ImageUploadItem = {
  clientId: string,
  contentType: string,
  sizeThumb: number,
  sizeFull: number,
  altText: string,
  thumbFile: Blob | null,
  fullFile: Blob | null
}
