import urql, { createClient, fetchExchange, type ClientOptions } from '@urql/vue'
import { cacheExchange } from '@urql/exchange-graphcache'

export default defineNuxtPlugin((nuxtApp) => {
  const config = useRuntimeConfig()

  const clientOptions: ClientOptions = {
    url: config.public.graphQlBase,
    exchanges: [
      cacheExchange(),
      fetchExchange
    ]
  }

  const client = createClient(clientOptions)

  nuxtApp.vueApp.use(urql, client)
})
