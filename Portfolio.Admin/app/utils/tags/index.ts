export const generateTagValue = (name: string): string =>
  name
    .toLowerCase()
    .replace(/[\s-]+/g, '_')
    .replace(/[^\w]/g, '')
    .replace(/_+/g, '_')
    .replace(/^_+|_+$/g, '')
