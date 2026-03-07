import Pica from 'pica'

export type ImageOutputMimeType =
  | 'image/webp'
  | 'image/jpeg'
  | 'image/png'

const pica = new Pica()

export const getOutputMimeType = (file: File | Blob): ImageOutputMimeType => {
  switch (file.type) {
  case 'image/jpeg':
  case 'image/jpg':
    return 'image/jpeg'

  case 'image/png':
    return 'image/png'

  case 'image/webp':
    return 'image/webp'

  default:
    return 'image/webp'
  }
}

export async function resizeImageTo (
  file: File | Blob,
  maxWidth: number,
  maxHeight: number,
  mimeType: ImageOutputMimeType = 'image/webp',
  quality = 0.9
): Promise<Blob> {
  const bitmap = await createImageBitmap(file)

  try {
    const srcWidth = bitmap.width
    const srcHeight = bitmap.height

    const ratio = Math.min(
      maxWidth / srcWidth,
      maxHeight / srcHeight,
      1
    )

    const targetWidth = Math.round(srcWidth * ratio)
    const targetHeight = Math.round(srcHeight * ratio)

    const canvas = document.createElement('canvas')
    canvas.width = targetWidth
    canvas.height = targetHeight

    await pica.resize(bitmap, canvas)

    const blob = await (pica.toBlob(canvas, mimeType, quality) as Promise<Blob>)

    return blob
  } finally {
    bitmap.close()
  }
}
