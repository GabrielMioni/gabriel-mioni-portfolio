type FetchErrorResponse = {
  response?: {
    status?: number
  }
}

export const getFetchErrorStatus = (error: unknown): number | undefined => {
  if (typeof error !== 'object' || error === null) {
    return undefined
  }

  return (error as FetchErrorResponse).response?.status
}
