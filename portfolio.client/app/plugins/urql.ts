import urql, { createClient, cacheExchange, fetchExchange, type ClientOptions } from '@urql/vue'

export default defineNuxtPlugin((nuxtApp) => {
  const config = useRuntimeConfig()

  const clientOptions: ClientOptions = {
    url: config.public.graphQlBase, // '/graphql'
    exchanges: [cacheExchange, fetchExchange],
    fetchOptions: {
      credentials: 'include'
    }
  }

  const client = createClient(clientOptions)

  nuxtApp.vueApp.use(urql, client)
})
