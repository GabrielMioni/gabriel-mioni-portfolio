import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import {
  defineComponent,
  nextTick,
  ref,
  type ComputedRef,
  type Ref
} from 'vue'
import {
  GetPublishedProjectsDocument,
  GetPublishedTagsDocument,
  type GetPublishedProjectsQuery,
  type GetPublishedProjectsQueryVariables,
  type GetPublishedTagsQuery
} from '~/generated/graphql'
import { useProjectQueries } from '~/composables/useProjectQueries'

const urqlMocks = vi.hoisted(() => ({
  useQuery: vi.fn()
}))

vi.mock('@urql/vue', () => ({
  useQuery: urqlMocks.useQuery
}))

const tagsData = ref<GetPublishedTagsQuery>()
const projectsData = ref<GetPublishedProjectsQuery>()
const fetchingProjects = ref(false)

type MountProjectQueriesOptions = {
  tagValues?: Ref<string[]>
}

const mountProjectQueries = ({
  tagValues = ref<string[]>([])
}: MountProjectQueriesOptions = {}) => {
  let projectQueries!: ReturnType<typeof useProjectQueries>
  const wrapper = mount(defineComponent({
    setup: () => {
      projectQueries = useProjectQueries(tagValues)

      return () => null
    }
  }))
  const projectQueryOptions = urqlMocks.useQuery.mock.calls
    .map(([options]) => options)
    .find(({ query }) => query === GetPublishedProjectsDocument)

  if (!projectQueryOptions) {
    throw new Error('Expected the published-projects query to be registered.')
  }

  return {
    projectQueries,
    tagValues,
    variables: projectQueryOptions.variables as ComputedRef<
      GetPublishedProjectsQueryVariables
    >,
    wrapper
  }
}

beforeEach(() => {
  tagsData.value = undefined
  projectsData.value = undefined
  fetchingProjects.value = false
  urqlMocks.useQuery.mockImplementation(({ query }) => {
    if (query === GetPublishedTagsDocument) {
      return { data: tagsData }
    }

    return {
      data: projectsData,
      fetching: fetchingProjects
    }
  })
})

afterEach(() => {
  urqlMocks.useQuery.mockReset()
})

describe('useProjectQueries', () => {
  it('starts at the first page and advances by the page size', () => {
    const {
      projectQueries,
      variables,
      wrapper
    } = mountProjectQueries()

    expect(variables.value).toEqual({
      skip: 0,
      take: 9,
      tagValues: undefined
    })

    projectQueries.loadMore()

    expect(variables.value).toEqual({
      skip: 9,
      take: 9,
      tagValues: undefined
    })

    wrapper.unmount()
  })

  it('returns to the first page when selected tags change', async () => {
    const {
      projectQueries,
      tagValues,
      variables,
      wrapper
    } = mountProjectQueries()
    projectQueries.loadMore()

    tagValues.value = ['vue', 'typescript']
    await nextTick()

    expect(variables.value).toEqual({
      skip: 0,
      take: 9,
      tagValues: ['vue', 'typescript']
    })

    wrapper.unmount()
  })
})
