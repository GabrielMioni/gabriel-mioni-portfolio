import {
  onMounted,
  onUnmounted,
  ref,
  type Ref
} from 'vue'

enum Breakpoint {
  Xs = '(max-width: 479px)',
  Sm = '(max-width: 639px)',
  Md = '(max-width: 767px)',
  Lg = '(max-width: 1023px)',
  Xl = '(max-width: 1279px)',
  Xxl = '(max-width: 1535px)'
}

export const useMediaQuery = () => {
  const xs = ref(false)
  const sm = ref(false)
  const md = ref(false)
  const lg = ref(false)
  const xl = ref(false)
  const xxl = ref(false)
  const bindings: Array<{
    media: MediaQueryList
    handleChange: (event: MediaQueryListEvent) => void
  }> = []

  const bind = (target: Ref<boolean>, query: string) => {
    const media = window.matchMedia(query)
    const handleChange = (event: MediaQueryListEvent) => {
      target.value = event.matches
    }

    target.value = media.matches
    media.addEventListener('change', handleChange)
    bindings.push({ media, handleChange })
  }

  onMounted(() => {
    bind(xs, Breakpoint.Xs)
    bind(sm, Breakpoint.Sm)
    bind(md, Breakpoint.Md)
    bind(lg, Breakpoint.Lg)
    bind(xl, Breakpoint.Xl)
    bind(xxl, Breakpoint.Xxl)
  })

  onUnmounted(() => {
    for (const { media, handleChange } of bindings) {
      media.removeEventListener('change', handleChange)
    }
    bindings.length = 0
  })

  return { xs, sm, md, lg, xl, xxl }
}
