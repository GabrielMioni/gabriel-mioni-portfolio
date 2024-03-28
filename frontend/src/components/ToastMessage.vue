<script setup>
import { useToast } from 'primevue/usetoast'
import eventBus from '@/services/EventBus.js'

console.log(eventBus)

const toast = useToast()

eventBus.$on('toast', ({ severity, summary, message }) => {
  console.log('toast event received', { severity, summary, message })
  toast.add({ severity, summary, detail: message })
})

eventBus.$off('toast')

</script>

<template>
  <Toast class="toast-message" />
</template>

<script>
export default {
  name: 'ToastMessage'
}
</script>

<style scoped lang="scss">

</style>
