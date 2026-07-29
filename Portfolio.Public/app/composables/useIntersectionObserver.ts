import {
  onMounted,
  onUnmounted,
  watch,
  type Ref
} from 'vue'

export const useIntersectionObserver = (
  target: Ref<Element | null | undefined>,
  callback: IntersectionObserverCallback,
  options?: IntersectionObserverInit
) => {
  let observer: IntersectionObserver | null = null

  onMounted(() => {
    observer = new IntersectionObserver(callback, options)
    if (target.value) observer.observe(target.value)
  })

  onUnmounted(() => {
    observer?.disconnect()
    observer = null
  })

  watch(target, (el, prev) => {
    if (prev) observer?.unobserve(prev)
    if (el) observer?.observe(el)
  })
}
