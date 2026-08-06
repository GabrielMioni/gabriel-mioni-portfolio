<script setup lang="ts">
defineProps<{
  eyebrow: string
  title: string
  description: string
  count: number
}>()
</script>

<template>
  <header class="admin-page-header">
    <div class="admin-page-header__copy">
      <p class="admin-page-header__eyebrow">
        {{ eyebrow }}
      </p>
      <div class="admin-page-header__title-row">
        <h1>{{ title }}</h1>
        <span class="admin-page-header__count">
          {{ count }} {{ count === 1 ? 'record' : 'records' }}
        </span>
      </div>
      <p class="admin-page-header__description">
        {{ description }}
      </p>
    </div>
    <div
      v-if="$slots.actions"
      class="admin-page-header__actions">
      <slot name="actions" />
    </div>
  </header>
</template>

<style scoped>
.admin-page-header {
  align-items: end;
  border-bottom: 1px solid rgb(var(--v-theme-ink));
  display: flex;
  gap: 2rem;
  justify-content: space-between;
  margin-bottom: 1.25rem;
  padding: 1.75rem 0 1.25rem;
}

.admin-page-header__copy {
  min-width: 0;
}

.admin-page-header__eyebrow,
.admin-page-header__count {
  font-family: var(--admin-font-mono);
  font-size: 0.7rem;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
}

.admin-page-header__eyebrow {
  color: rgb(var(--v-theme-rust));
  margin-bottom: 0.4rem;
}

.admin-page-header__title-row {
  align-items: baseline;
  display: flex;
  flex-wrap: wrap;
  gap: 0.65rem 1rem;
}

.admin-page-header h1 {
  font-family: var(--admin-font-display);
  font-size: clamp(2.4rem, 5vw, 4rem);
  font-stretch: condensed;
  font-weight: 900;
  letter-spacing: -0.035em;
  line-height: 0.95;
}

.admin-page-header__count {
  color: rgb(var(--v-theme-muted));
}

.admin-page-header__description {
  color: rgb(var(--v-theme-muted));
  font-family: var(--admin-font-body);
  font-size: 1rem;
  margin-top: 0.65rem;
}

.admin-page-header__actions {
  flex: none;
  padding-bottom: 0.15rem;
}

@media (max-width: 599px) {
  .admin-page-header {
    align-items: stretch;
    flex-direction: column;
    gap: 1rem;
  }

  .admin-page-header__actions :deep(.v-btn) {
    width: 100%;
  }
}
</style>
