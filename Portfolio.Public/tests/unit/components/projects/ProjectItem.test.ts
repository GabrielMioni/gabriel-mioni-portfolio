import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import { defineComponent } from 'vue'
import type { PublicProjectFragment } from '~/generated/graphql'
import ProjectItem from '~/components/projects/ProjectItem.vue'

const CardStub = defineComponent({
  props: {
    as: {
      type: String,
      default: 'div'
    }
  },
  template: `
    <component :is="as">
      <slot name="header" />
      <slot />
      <slot name="footer" />
    </component>
  `
})

const createProject = (
  overrides: Partial<PublicProjectFragment> = {}
): PublicProjectFragment => ({
  id: 'project-one',
  title: 'Accessible Project',
  summary: 'A concise project summary.',
  body: 'Additional project details.',
  publishedAt: null,
  images: [],
  links: [],
  tags: [],
  ...overrides
})

const mountProjectItem = (project: PublicProjectFragment) => mount(ProjectItem, {
  props: { project },
  global: {
    stubs: {
      UCard: CardStub,
      StorageImage: true,
      ProjectLinks: true,
      ProjectTagIcons: true
    }
  }
})

describe('ProjectItem', () => {
  it('uses a labeled button to open a project study', async () => {
    const project = createProject()
    const wrapper = mountProjectItem(project)
    const article = wrapper.get('article')
    const openButton = article.get('button')
    const title = article.get('h3')
    const summary = article.get('.project-summary')

    expect(openButton.element.tabIndex).toBe(0)
    expect(openButton.attributes('aria-labelledby')).toBe(title.attributes('id'))
    expect(openButton.attributes('aria-describedby')).toBe(summary.attributes('id'))

    await openButton.trigger('click')
    await openButton.trigger('keydown', { key: 'Enter' })
    await openButton.trigger('keydown', { key: ' ' })

    expect(wrapper.emitted('select')).toEqual([
      [project.id],
      [project.id],
      [project.id]
    ])
  })

  it('does not add an open control when no project study is available', () => {
    const wrapper = mountProjectItem(createProject({
      body: null,
      images: []
    }))

    expect(wrapper.find('article').exists()).toBe(true)
    expect(wrapper.find('button').exists()).toBe(false)
  })
})
