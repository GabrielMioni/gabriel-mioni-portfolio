import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { defineComponent } from 'vue'
import { useMediaQuery } from '~/composables/useMediaQuery'

const breakpointQueries = {
  xs: '(max-width: 479px)',
  sm: '(max-width: 639px)',
  md: '(max-width: 767px)',
  lg: '(max-width: 1023px)',
  xl: '(max-width: 1279px)',
  xxl: '(max-width: 1535px)'
} as const

type MediaQueryHarness = {
  media: MediaQueryList
  removeEventListener: ReturnType<typeof vi.fn>
  dispatchChange: (matches: boolean) => void
  listenerCount: () => number
}

const mediaQueryHarnesses = new Map<string, MediaQueryHarness>()
const matchMediaMock = vi.fn()

const createMediaQueryHarness = (
  query: string,
  initialMatches: boolean
): MediaQueryHarness => {
  const listeners = new Set<(event: MediaQueryListEvent) => void>()

  const addEventListener = vi.fn((
    type: string,
    listener: (event: MediaQueryListEvent) => void
  ) => {
    if (type === 'change') listeners.add(listener)
  })

  const removeEventListener = vi.fn((
    type: string,
    listener: (event: MediaQueryListEvent) => void
  ) => {
    if (type === 'change') listeners.delete(listener)
  })

  const mutableMedia = {
    matches: initialMatches,
    media: query,
    onchange: null,
    addEventListener,
    removeEventListener,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    dispatchEvent: vi.fn()
  }
  const media = mutableMedia as unknown as MediaQueryList

  return {
    media,
    removeEventListener,
    dispatchChange: (nextMatches) => {
      mutableMedia.matches = nextMatches
      const event = {
        matches: mutableMedia.matches,
        media: query
      } as MediaQueryListEvent
      for (const listener of listeners) listener(event)
    },
    listenerCount: () => listeners.size
  }
}

const mountMediaQueryComponent = () => {
  let mediaQueries!: ReturnType<typeof useMediaQuery>
  const wrapper = mount(defineComponent({
    setup: () => {
      mediaQueries = useMediaQuery()

      return () => null
    }
  }))

  return { mediaQueries, wrapper }
}

beforeEach(() => {
  mediaQueryHarnesses.clear()
  matchMediaMock.mockImplementation((query: string) => {
    const harness = createMediaQueryHarness(
      query,
      query === breakpointQueries.md
    )
    mediaQueryHarnesses.set(query, harness)
    return harness.media
  })
  vi.stubGlobal('matchMedia', matchMediaMock)
})

afterEach(() => {
  vi.clearAllMocks()
  vi.unstubAllGlobals()
})

describe('useMediaQuery', () => {
  it('initializes each breakpoint from its media query', () => {
    const { mediaQueries, wrapper } = mountMediaQueryComponent()

    expect({
      xs: mediaQueries.xs.value,
      sm: mediaQueries.sm.value,
      md: mediaQueries.md.value,
      lg: mediaQueries.lg.value,
      xl: mediaQueries.xl.value,
      xxl: mediaQueries.xxl.value
    }).toEqual({
      xs: false,
      sm: false,
      md: true,
      lg: false,
      xl: false,
      xxl: false
    })
    expect(matchMediaMock.mock.calls.map(([query]) => query)).toEqual(
      Object.values(breakpointQueries)
    )

    wrapper.unmount()
  })

  it('updates a breakpoint when its media query changes', () => {
    const { mediaQueries, wrapper } = mountMediaQueryComponent()
    const mdHarness = mediaQueryHarnesses.get(breakpointQueries.md)

    expect(mdHarness).toBeDefined()

    mdHarness!.dispatchChange(false)

    expect(mediaQueries.md.value).toBe(false)

    wrapper.unmount()
  })

  it('removes media query listeners when the component unmounts', () => {
    const { wrapper } = mountMediaQueryComponent()

    wrapper.unmount()

    for (const harness of mediaQueryHarnesses.values()) {
      expect(harness.removeEventListener).toHaveBeenCalledOnce()
      expect(harness.listenerCount()).toBe(0)
    }
  })
})
