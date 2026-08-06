<template>
  <UCard
    as="article"
    :ui="{ header: 'p-0' }"
    class="project-skeleton"
    aria-hidden="true">
    <template #header>
      <div class="project-skeleton__image-frame editorial-corner-field">
        <div class="project-skeleton__image aspect-video">
          <div class="project-skeleton__block size-full" />
        </div>
      </div>
    </template>
    <div class="project-skeleton__copy">
      <div class="project-skeleton__block project-skeleton__label" />
      <div class="project-skeleton__block project-skeleton__title" />
      <div class="project-skeleton__summary">
        <div class="project-skeleton__block" />
        <div class="project-skeleton__block" />
        <div class="project-skeleton__block" />
      </div>
      <div class="project-skeleton__tags">
        <div
          v-for="n in 3"
          :key="n"
          class="project-skeleton__block project-skeleton__tag" />
      </div>
    </div>
  </UCard>
</template>

<style scoped>
.project-skeleton {
  height: 100%;
  overflow: hidden;
  border: 1px solid color-mix(
    in srgb,
    var(--folio-ink) 15%,
    transparent
  );
  border-radius: 0;
  background: var(--folio-paper-raised);
}

.project-skeleton__image-frame {
  display: grid;
  min-height: 15rem;
  padding: 1rem;
  border-bottom: 1px solid var(--folio-ink);
  place-items: center;
}

.project-skeleton__image {
  width: 100%;
  overflow: hidden;
  border: 1px solid var(--folio-ink);
}

.project-skeleton__copy {
  display: grid;
  gap: .65rem;
}

.project-skeleton__block {
  border-radius: 0;
  background-image: linear-gradient(
    90deg,
    color-mix(in srgb, var(--folio-rule) 38%, var(--folio-paper-raised)) 0%,
    color-mix(in srgb, var(--folio-rule) 38%, var(--folio-paper-raised)) 38%,
    color-mix(in srgb, var(--folio-cyan) 24%, var(--folio-paper-raised)) 50%,
    color-mix(in srgb, var(--folio-rule) 38%, var(--folio-paper-raised)) 62%,
    color-mix(in srgb, var(--folio-rule) 38%, var(--folio-paper-raised)) 100%
  );
  background-position: 100% 0;
  background-size: 240% 100%;
  animation: project-skeleton-scan 1.8s linear infinite;
}

.project-skeleton__label {
  width: 6.5rem;
  height: .55rem;
}

.project-skeleton__title {
  width: 68%;
  height: 1.6rem;
}

.project-skeleton__summary {
  display: grid;
  gap: .45rem;
  margin-top: .1rem;
}

.project-skeleton__summary .project-skeleton__block {
  height: .7rem;
}

.project-skeleton__summary .project-skeleton__block:nth-child(2) {
  width: 92%;
}

.project-skeleton__summary .project-skeleton__block:nth-child(3) {
  width: 72%;
}

.project-skeleton__tags {
  display: flex;
  gap: .5rem;
  padding-top: .25rem;
}

.project-skeleton__tag {
  width: 1rem;
  height: 1rem;
  border-radius: 50%;
}

@keyframes project-skeleton-scan {
  to {
    background-position: -100% 0;
  }
}

@media (prefers-reduced-motion: reduce) {
  .project-skeleton__block {
    animation: none;
  }
}
</style>
