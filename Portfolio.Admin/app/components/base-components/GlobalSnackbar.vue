<script setup lang="ts">
import { storeToRefs } from 'pinia'

const snackbarStore = useSnackbarStore()
const { messages } = storeToRefs(snackbarStore)
</script>

<template>
  <v-snackbar-queue
    v-model="messages"
    timeout="3000"
    closable />
</template>

<style scoped>

</style>
