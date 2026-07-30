export const MAX_PROJECT_IMAGES = 15
export const MAX_PROJECT_TAGS = 15

export const getRemainingCapacity = (
  currentCount: number,
  maximumCount: number
): number => Math.max(maximumCount - Math.max(currentCount, 0), 0)

export const takeItemsWithinCapacity = <T>(
  items: T[],
  currentCount: number,
  maximumCount: number
): T[] => items.slice(0, getRemainingCapacity(currentCount, maximumCount))
