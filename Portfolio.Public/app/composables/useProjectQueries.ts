import {
  GetPublishedProjectsDocument,
  PublicProjectFragmentDoc
} from '~/generated/graphql'
import { useQuery } from '@urql/vue'
import { useFragment } from '~/generated'

export const useProjectQueries = () => {
  const {
    data,
    fetching: fetchingProjects
  } = useQuery({
    query: GetPublishedProjectsDocument,
    requestPolicy: 'cache-and-network',
    variables: {
      skip: 0,
      take: 10
    }
  })

  const projects = computed(() => {
    const raw = data.value?.publishedProjects?.items || []

    return raw.map(item => useFragment(PublicProjectFragmentDoc, item))
  })

  return {
    projects,
    fetchingProjects
  }
}
