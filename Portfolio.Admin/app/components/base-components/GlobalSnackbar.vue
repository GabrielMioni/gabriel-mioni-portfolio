<script setup lang="ts">
import { storeToRefs } from 'pinia'

const snackbarStore = useSnackbarStore()
const { messages } = storeToRefs(snackbarStore)
</script>

<template>
  <v-snackbar-queue
    v-model="messages"
    class="admin-snackbar-queue"
    location="bottom end"
    timeout="3000"
    closable />
</template>

<style scoped>
.admin-snackbar-queue :deep(.v-snackbar__wrapper) {
  border: 1px solid currentColor;
  border-radius: 0;
  box-shadow: 6px 6px 0 color-mix(in srgb, rgb(var(--v-theme-ink)) 22%, transparent);
  font-family: var(--admin-font-ui);
}

.admin-snackbar-queue :deep(.v-snackbar__content) {
  line-height: 1.4;
}
</style>
