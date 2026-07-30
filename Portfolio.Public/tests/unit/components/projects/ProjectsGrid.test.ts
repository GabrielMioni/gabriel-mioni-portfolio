import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import {
  defineComponent,
  nextTick,
  ref,
  type Ref
} from 'vue'
import type { PublicProjectFragment } from '~/generated/graphql'
import ProjectsGrid from '~/components/projects/ProjectsGrid.vue'

const composableMocks = vi.hoisted(() => ({
  useProjectQueries: vi.fn(),
  useIntersectionObserver: vi.fn()
}))

vi.mock('~/composables/useProjectQueries', () => ({
  useProjectQueries: composableMocks.useProjectQueries
}))

vi.mock('~/composables/useIntersectionObserver', () => ({
  useIntersectionObserver: composableMocks.useIntersectionObserver
}))

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

const ButtonStub = defineComponent({
  props: {
    label: {
      type: String,
      required: true
    }
  },
  emits: ['click'],
  template: '<button type="button" @click="$emit(\'click\')">{{ label }}</button>'
})

const ProjectItemStub = defineComponent({
  props: {
    project: {
      type: Object,
      required: true
    }
  },
  template: '<article :data-project-id="project.id">{{ project.title }}</article>'
})

const mountProjectsGrid = () => mount(ProjectsGrid, {
  global: {
    stubs: {
      UContainer: {
        template: '<div><slot /></div>'
      },
      UButton: ButtonStub,
      UIcon: true,
      ProjectItem: ProjectItemStub,
      ProjectItemSkeleton: true,
      ProjectDialog: true,
      TransitionGroup: {
        template: '<div><slot /></div>'
      }
    }
  }
})

const projects = ref<PublicProjectFragment[]>([])
const fetchingProjects = ref(false)
const hasNextPage = ref(false)
const availableTags = ref([
  {
    id: 'tag-vue',
    name: 'Vue',
    value: 'vue'
  }
])
const loadMore = vi.fn()
let selectedTags: Ref<string[]>
let intersectionCallback: IntersectionObserverCallback

beforeEach(() => {
  projects.value = []
  fetchingProjects.value = false
  hasNextPage.value = false
  loadMore.mockReset()

  composableMocks.useProjectQueries.mockImplementation((tags: Ref<string[]>) => {
    selectedTags = tags

    return {
      projects,
      fetchingProjects,
      hasNextPage,
      loadMore,
      availableTags
    }
  })
  composableMocks.useIntersectionObserver.mockImplementation((
    _target: Ref<Element | null | undefined>,
    callback: IntersectionObserverCallback
  ) => {
    intersectionCallback = callback
  })
})

afterEach(() => {
  vi.clearAllMocks()
})

describe('ProjectsGrid', () => {
  it('loads the next page when the observer reaches the grid boundary', () => {
    hasNextPage.value = true
    const wrapper = mountProjectsGrid()

    intersectionCallback([
      { isIntersecting: true } as IntersectionObserverEntry
    ], {} as IntersectionObserver)

    expect(loadMore).toHaveBeenCalledOnce()

    wrapper.unmount()
  })

  it('filters the project collection when a tag is selected', async () => {
    const originalProject = createProject('project-one', 'Original Project')
    const filteredProject = createProject('project-vue', 'Vue Project')
    projects.value = [originalProject]
    const wrapper = mountProjectsGrid()
    const vueButton = wrapper
      .findAll('button')
      .find(button => button.text() === 'Vue')

    if (!vueButton) {
      throw new Error('Expected the Vue filter button to be rendered.')
    }

    await vueButton.trigger('click')

    expect(selectedTags.value).toEqual(['vue'])

    projects.value = [filteredProject]
    await nextTick()

    expect(wrapper.text()).not.toContain(originalProject.title)
    expect(wrapper.text()).toContain(filteredProject.title)

    wrapper.unmount()
  })
})
