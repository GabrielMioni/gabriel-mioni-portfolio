import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import {
  defineComponent,
  h,
  nextTick,
  ref,
  type Ref,
  type VNodeChild
} from 'vue'
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

type MountObserverComponentOptions = {
  target: Ref<Element | null | undefined>
  callback?: IntersectionObserverCallback
  options?: IntersectionObserverInit
  render?: () => VNodeChild
}

const mountObserverComponent = ({
  target,
  callback = vi.fn(),
  options,
  render = () => null
}: MountObserverComponentOptions) => mount(defineComponent({
  setup: () => {
    useIntersectionObserver(target, callback, options)

    return render
  }
}))

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
    const wrapper = mountObserverComponent({
      target,
      callback,
      options,
      render: () => h('div', { ref: target })
    })

    expect(observerMocks.create).toHaveBeenCalledWith(callback, options)
    expect(observerMocks.observe).toHaveBeenCalledWith(wrapper.element)

    wrapper.unmount()
  })

  it('moves observation when the target element changes', async () => {
    const firstTarget = document.createElement('div')
    const nextTarget = document.createElement('div')
    const target = ref<Element | null>(firstTarget)
    const wrapper = mountObserverComponent({ target })
    observerMocks.observe.mockClear()

    target.value = nextTarget
    await nextTick()

    expect(observerMocks.unobserve).toHaveBeenCalledWith(firstTarget)
    expect(observerMocks.observe).toHaveBeenCalledWith(nextTarget)

    wrapper.unmount()
  })

  it('disconnects the observer when the component unmounts', () => {
    const target = ref<Element | null>(document.createElement('div'))
    const wrapper = mountObserverComponent({ target })

    wrapper.unmount()

    expect(observerMocks.disconnect).toHaveBeenCalledOnce()
  })
})
