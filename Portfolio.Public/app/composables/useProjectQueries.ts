import {
  GetPublishedProjectsDocument,
  GetPublishedTagsDocument,
  PublicProjectFragmentDoc,
  type PublicProjectFragment
} from '~/generated/graphql'
import { useQuery } from '@urql/vue'
import { useFragment } from '~/generated'
import {
  computed,
  ref,
  watch,
  type Ref
} from 'vue'

const PAGE_SIZE = 9

export const useProjectQueries = (tagValues?: Ref<string[]>) => {
  const skip = ref(0)
  const projects = ref<PublicProjectFragment[]>([])

  const { data: tagsData } = useQuery({ query: GetPublishedTagsDocument })

  const availableTags = computed(() => tagsData.value?.publishedTags ?? [])

  if (tagValues) {
    watch(tagValues, () => {
      skip.value = 0
    })
  }

  const {
    data,
    fetching: fetchingProjects
  } = useQuery({
    query: GetPublishedProjectsDocument,
    variables: computed(() => ({
      skip: skip.value,
      take: PAGE_SIZE,
      tagValues: tagValues?.value.length ? tagValues.value : undefined
    }))
  })

  watch(data, (newData) => {
    const items = newData?.publishedProjects?.items ?? []
    const fragments = items.map(item => useFragment(PublicProjectFragmentDoc, item))
    if (skip.value === 0) {
      projects.value = fragments
    } else {
      projects.value = [...projects.value, ...fragments]
    }
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
    loadMore,
    availableTags
  }
}
