import type { Directive } from 'vue'

const focusFirstInput: Directive<HTMLElement> = {
  mounted (el) {
    requestAnimationFrame(() => {
      el.querySelector<HTMLElement>('input:not([type="hidden"]), textarea')?.focus()
    })
  }
}

export default defineNuxtPlugin((nuxtApp) => {
  nuxtApp.vueApp.directive('focus-first-input', focusFirstInput)
})
