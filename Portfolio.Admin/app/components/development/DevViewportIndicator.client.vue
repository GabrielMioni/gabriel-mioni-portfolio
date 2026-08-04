<script setup lang="ts">
const { name, width, height } = useDisplay()
</script>

<template>
  <div
    class="dev-viewport-indicator bg-surface text-on-surface"
    aria-hidden="true">
    <strong class="text-primary">{{ name }}</strong>
    <span>{{ width }} &times; {{ height }} px</span>
  </div>
</template>

<style scoped>
.dev-viewport-indicator {
  position: fixed;
  right: 0.75rem;
  bottom: 0.75rem;
  z-index: 10000;
  display: flex;
  gap: 0.5rem;
  align-items: center;
  padding: 0.375rem 0.625rem;
  font-family: monospace;
  font-size: 0.75rem;
  line-height: 1;
  pointer-events: none;
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  border-radius: 0.375rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.18);
  backdrop-filter: blur(4px);
}

.dev-viewport-indicator strong {
  text-transform: uppercase;
}
</style>
