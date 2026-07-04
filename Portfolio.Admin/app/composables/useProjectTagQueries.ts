import { useQuery } from '@urql/vue'
import { GetTagsDocument, ProjectTagFragmentDoc, type ProjectTagFragment } from '~/generated/graphql'
import { useFragment } from '~/generated'

export const useProjectTagQueries = () => {
  const { data, fetching: fetchingTags } = useQuery({
    query: GetTagsDocument,
    requestPolicy: 'cache-and-network'
  })

  const allTags = computed<ProjectTagFragment[]>(() =>
    (data.value?.tags ?? []).map(t => useFragment(ProjectTagFragmentDoc, t))
  )

  return { allTags, fetchingTags }
}
