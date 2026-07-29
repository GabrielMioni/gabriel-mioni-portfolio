import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { defineComponent, h, ref } from 'vue'
import { useIntersectionObserver } from '~/composables/useIntersectionObserver'

const observerMocks = {
  create: vi.fn(),
  observe: vi.fn(),
  unobserve: vi.fn(),
  disconnect: vi.fn(),
  takeRecords: vi.fn(() => [])
}

const IntersectionObserverMock = vi.fn(function (
  callback: IntersectionObserverCallback,
  options?: IntersectionObserverInit
): IntersectionObserver {
  observerMocks.create(callback, options)

  return {
    root: null,
    rootMargin: '',
    scrollMargin: '',
    thresholds: [],
    observe: observerMocks.observe,
    unobserve: observerMocks.unobserve,
    disconnect: observerMocks.disconnect,
    takeRecords: observerMocks.takeRecords
  }
})

beforeEach(() => {
  vi.stubGlobal('IntersectionObserver', IntersectionObserverMock)
})

afterEach(() => {
  vi.clearAllMocks()
  vi.unstubAllGlobals()
})

describe('useIntersectionObserver', () => {
  it('observes the target element after mounting', () => {
    const target = ref<Element | null>(null)
    const callback: IntersectionObserverCallback = vi.fn()
    const options: IntersectionObserverInit = {
      rootMargin: '100px'
    }
    const TestComponent = defineComponent({
      setup: () => {
        useIntersectionObserver(target, callback, options)

        return () => h('div', { ref: target })
      }
    })

    const wrapper = mount(TestComponent)

    expect(observerMocks.create).toHaveBeenCalledWith(callback, options)
    expect(observerMocks.observe).toHaveBeenCalledWith(wrapper.element)

    wrapper.unmount()
  })
})
