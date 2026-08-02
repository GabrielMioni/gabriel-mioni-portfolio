import { getFetchErrorStatus } from '~/utils/http'

export default defineNuxtRouteMiddleware(async (to) => {
  const { public: { authRequired } } = useRuntimeConfig()

  if (!authRequired || to.path === '/login') {
    return
  }

  const { apiFetch } = useApiFetch()

  try {
    await apiFetch('/me')
  } catch (error) {
    if (getFetchErrorStatus(error) !== 401) {
      throw error
    }

    return navigateTo({
      path: '/login',
      query: { returnUrl: to.fullPath }
    }, { replace: true })
  }
})
