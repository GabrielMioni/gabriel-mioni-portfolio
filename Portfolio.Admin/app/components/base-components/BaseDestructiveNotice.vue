<script setup lang="ts">
withDefaults(
  defineProps<{
    title: string
    label?: string
  }>(),
  {
    label: 'Permanent action'
  }
)
</script>

<template>
  <div class="base-destructive-notice">
    <v-icon
      class="base-destructive-notice__icon"
      icon="mdi-alert-outline"
      size="28" />
    <div>
      <p class="base-destructive-notice__label">{{ label }}</p>
      <h2>{{ title }}</h2>
      <div class="base-destructive-notice__content">
        <slot />
      </div>
    </div>
  </div>
</template>

<style scoped>
.base-destructive-notice {
  align-items: start;
  background: color-mix(in srgb, rgb(var(--v-theme-error)) 9%, transparent);
  border: 1px solid color-mix(in srgb, rgb(var(--v-theme-error)) 45%, transparent);
  border-inline-start: 5px solid rgb(var(--v-theme-error));
  display: grid;
  gap: 0.9rem;
  grid-template-columns: auto minmax(0, 1fr);
  padding: 1rem;
}

.base-destructive-notice__icon,
.base-destructive-notice__label {
  color: rgb(var(--v-theme-error));
}

.base-destructive-notice__label {
  font-family: var(--admin-font-mono);
  font-size: 0.68rem;
  font-weight: 700;
  letter-spacing: 0.09em;
  text-transform: uppercase;
}

.base-destructive-notice h2 {
  font-family: var(--admin-font-display);
  font-size: 1.35rem;
  font-weight: 850;
  line-height: 1.1;
  margin: 0.15rem 0 0.4rem;
}

.base-destructive-notice__content {
  color: rgb(var(--v-theme-muted));
  line-height: 1.5;
}
</style>
