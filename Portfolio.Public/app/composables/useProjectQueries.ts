import {
  GetPublishedProjectsDocument,
  PublicProjectFragmentDoc,
  type PublicProjectFragment
} from '~/generated/graphql'
import { useQuery } from '@urql/vue'
import { useFragment } from '~/generated'

const PAGE_SIZE = 9

export const useProjectQueries = () => {
  const skip = ref(0)
  const projects = ref<PublicProjectFragment[]>([])

  const {
    data,
    fetching: fetchingProjects
  } = useQuery({
    query: GetPublishedProjectsDocument,
    requestPolicy: 'network-only',
    variables: computed(() => ({ skip: skip.value, take: PAGE_SIZE }))
  })

  // accumulator
  watch(data, (newData) => {
    const items = newData?.publishedProjects?.items ?? []
    projects.value = [
      ...projects.value,
      ...items.map(item => useFragment(PublicProjectFragmentDoc, item))
    ]
  })

  const hasNextPage = computed(() =>
    data.value?.publishedProjects?.pageInfo.hasNextPage ?? false
  )

  const loadMore = () => {
    skip.value += PAGE_SIZE
  }

  return {
    projects,
    fetchingProjects,
    hasNextPage,
    loadMore
  }
}
