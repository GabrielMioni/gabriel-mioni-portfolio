import { defineNuxtPlugin, useRuntimeConfig } from '#app'
import { Client, fetchExchange, provideClient } from '@urql/vue'

export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig()

  const urql = new Client({
    url: config.public.graphQlBase,
    exchanges: [fetchExchange],
    fetchOptions: {
      credentials: 'include'
    }
  })

  provideClient(urql)
})
