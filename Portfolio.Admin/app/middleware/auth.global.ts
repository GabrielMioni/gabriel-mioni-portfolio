import { getFetchErrorStatus } from '~/utils/http'

export default defineNuxtRouteMiddleware(async (to) => {
  if (to.path === '/login') {
    return
  }

  const { apiFetch } = useApiFetch()

  try {
    await apiFetch('/me')
  } catch (error) {
    const status = getFetchErrorStatus(error)

    if (status !== 401 && status !== 403) {
      throw error
    }

    return navigateTo({
      path: '/login',
      query: {
        returnUrl: to.fullPath,
        ...(status === 403 ? { error: 'account_not_authorized' } : {})
      }
    }, { replace: true })
  }
})
