<script setup>
import { useToast } from 'primevue/usetoast'
import eventBus from '@/services/EventBus.js'

const toast = useToast()

eventBus.$on('toast', ({ severity, summary, message, life = 3000 }) => {
  toast.add({ severity, summary, detail: message, life })
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
