<script setup>
import { useToast } from 'primevue/usetoast'
import eventBus from '@/services/EventBus.js'
import { onBeforeMount, onBeforeUnmount } from 'vue'

const toast = useToast()

const displayToastMessage = ({ severity, summary, message, life }) => {
  toast.add({ severity, summary, detail: message, life })
}

onBeforeMount(() => {
  eventBus.$on('toast', ({ severity, summary, message, life }) => {
    displayToastMessage({ severity, summary, message, life })
  })
})

onBeforeUnmount(() => {
  eventBus.$off('toast', displayToastMessage)
})

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
