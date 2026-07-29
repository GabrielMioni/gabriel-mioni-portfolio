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
  type GetPublishedTagsQuery,
  type PublicProjectFragment
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

type PublishedProjectsPage = NonNullable<
  GetPublishedProjectsQuery['publishedProjects']
>

const createProject = (
  id: string,
  title: string
): PublicProjectFragment => ({
  id,
  title,
  summary: null,
  body: null,
  publishedAt: null,
  images: [],
  links: [],
  tags: []
})

type SetProjectPageOptions = {
  projects: PublicProjectFragment[]
  hasNextPage: boolean
  totalCount?: number
}

const setProjectPage = ({
  projects,
  hasNextPage,
  totalCount = projects.length
}: SetProjectPageOptions) => {
  projectsData.value = {
    publishedProjects: {
      totalCount,
      items: projects as unknown as NonNullable<PublishedProjectsPage['items']>,
      pageInfo: {
        hasNextPage,
        hasPreviousPage: false
      }
    }
  }
}

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

  it('exposes the tags returned by the tags query', () => {
    const { projectQueries, wrapper } = mountProjectQueries()
    const tags = [
      {
        id: 'tag-vue',
        name: 'Vue',
        value: 'vue'
      },
      {
        id: 'tag-typescript',
        name: 'TypeScript',
        value: 'typescript'
      }
    ]

    tagsData.value = {
      publishedTags: tags
    }

    expect(projectQueries.availableTags.value).toEqual(tags)

    wrapper.unmount()
  })

  it('replaces the first project page and appends later pages', async () => {
    const firstPageProject = createProject('project-one', 'Project One')
    const secondPageProject = createProject('project-two', 'Project Two')
    const { projectQueries, wrapper } = mountProjectQueries()

    setProjectPage({
      projects: [firstPageProject],
      hasNextPage: true,
      totalCount: 2
    })
    await nextTick()

    expect(projectQueries.projects.value).toEqual([firstPageProject])
    expect(projectQueries.hasNextPage.value).toBe(true)

    projectQueries.loadMore()
    setProjectPage({
      projects: [secondPageProject],
      hasNextPage: false,
      totalCount: 2
    })
    await nextTick()

    expect(projectQueries.projects.value).toEqual([
      firstPageProject,
      secondPageProject
    ])
    expect(projectQueries.hasNextPage.value).toBe(false)

    wrapper.unmount()
  })
})
