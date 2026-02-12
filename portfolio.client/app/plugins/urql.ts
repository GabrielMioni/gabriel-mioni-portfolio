// import urql, {
//   Client,
//   fetchExchange,
//   type ClientOptions
// } from '@urql/vue'
//
// export default defineNuxtPlugin((nuxtApp) => {
//   const config = useRuntimeConfig()
//
//   const clientOptions: ClientOptions = {
//     url: config.public.graphQlBase,
//     exchanges: [fetchExchange],
//     fetchOptions: {
//       credentials: 'include'
//     }
//   }
//
//   const client = new Client(clientOptions)
//
//   nuxtApp.vueApp.use(urql, client)
//
//   return {
//     provide: {
//       urqlClient: client
//     }
//   }
// })
import { defineNuxtPlugin, useRuntimeConfig } from '#app'
import { createClient, fetchExchange } from '@urql/vue'

export default defineNuxtPlugin((nuxtApp) => {
  const config = useRuntimeConfig()

  const client = createClient({
    url: config.public.graphQlBase, // '/graphql' from your nuxt.config.ts
    exchanges: [fetchExchange],
    fetchOptions: {
      credentials: 'include'
    }
  })

  // IMPORTANT: this is the injection key urql composables read from
  nuxtApp.vueApp.provide('$urql', client)
})

