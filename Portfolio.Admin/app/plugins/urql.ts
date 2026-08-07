import urql, { createClient, fetchExchange, type ClientOptions } from '@urql/vue'
import { cacheExchange } from '@urql/exchange-graphcache'
import { adminCacheUpdates } from '~/utils/graphql/cacheUpdates'


export default defineNuxtPlugin((nuxtApp) => {
  const config = useRuntimeConfig()

  const clientOptions: ClientOptions = {
    url: config.public.graphQlBase, // '/graphql'
    exchanges: [
      cacheExchange({
        keys: {
          ProjectsCollectionSegment: () => null,
          CollectionSegmentInfo: () => null
        },
        updates: adminCacheUpdates
      }),
      fetchExchange
    ],
    fetchOptions: {
      credentials: 'include'
    }
  }

  const client = createClient(clientOptions)

  nuxtApp.vueApp.use(urql, client)
})
