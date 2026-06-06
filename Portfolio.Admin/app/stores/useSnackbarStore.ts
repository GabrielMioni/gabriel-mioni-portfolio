import type { SnackbarMessage, SnackbarColor } from '~/types/ui/SnackbarMessage'

export const useSnackbarStore = defineStore('snackbar', () => {
  const messages = ref<SnackbarMessage[]>([])

  const showSnackbar = (text: string, color: SnackbarColor = 'info') => {
    messages.value.push({ text, color })
  }

  return {
    messages,
    showSnackbar
  }
})
