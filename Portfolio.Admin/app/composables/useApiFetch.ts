type FetchOptions = NonNullable<Parameters<typeof $fetch>[1]>

type ApiFetchOptions<TBody> = Omit<FetchOptions, 'body'> & {
  body?: TBody
}

const normalizePath = (apiBase: string, path: string) => {
  if (/^https?:\/\//i.test(path)) return path

  if (!path.startsWith('/')) {
    throw new Error(`apiFetch path must start with "/". Got: "${path}"`)
  }

  const base = (apiBase || '/api').replace(/\/+$/, '') // no trailing slash
  return `${base}${path}`.replace(/\/+/g, '/')
}

export const useApiFetch = () => {
  const { public: { apiBase } } = useRuntimeConfig()

  const apiFetch = async <TResponse = unknown, TBody = unknown> (
    path: string,
    options: ApiFetchOptions<TBody> = {}
  ): Promise<TResponse> => {
    const url = normalizePath(apiBase, path)

    const { body, method, ...rest } = options
    const hasBody = body !== undefined

    return await $fetch<TResponse>(url, {
      ...rest,
      credentials: 'include',
      method: method ?? (hasBody ? 'POST' : 'GET'),
      ...(hasBody ? { body } : {})
    })
  }

  return { apiFetch }
}
