<script setup lang="ts">
import type { NuxtError } from '#app'

const props = defineProps<{
  error: NuxtError
}>()

const statusCode = computed(() => props.error.statusCode ?? 500)
const isNotFound = computed(() => statusCode.value === 404)

const heading = computed(() => isNotFound.value
  ? 'Page not found'
  : 'System error')

const description = computed(() => isNotFound.value
  ? 'The requested page is not present in this system. It may have been moved, renamed, or never filed.'
  : 'The public portfolio encountered an unexpected condition. Returning to the index may resolve it.')

const figureCaption = computed(() => isNotFound.value
  ? 'Response to missing material'
  : 'Unexpected system condition')

const returnToIndex = () => clearError({ redirect: '/' })

useHead(() => ({
  title: `${statusCode.value} — ${heading.value}`
}))
</script>

<template>
  <UApp>
    <div class="error-page">
      <header class="error-header">
        <NuxtLink
          to="/"
          class="folio-focus-ring error-home-link"
          aria-label="Return to the portfolio home page">
          <AppLogo class="h-6 w-auto" />
        </NuxtLink>

        <UColorModeButton />
      </header>

      <main class="error-main">
        <section class="error-record">
          <figure class="error-figure">
            <img
              src="/not-found-face.svg"
              class="error-face"
              alt="Hand-drawn face expressing skeptical disapproval">

            <figcaption>
              Fig. {{ statusCode }} — {{ figureCaption }}
            </figcaption>
          </figure>

          <div class="error-copy">
            <p class="editorial-kicker error-kicker">
              Public portfolio system / Error record
            </p>

            <div class="error-title">
              <p
                class="error-code"
                aria-hidden="true">
                {{ statusCode }}
              </p>

              <h1>{{ heading }}</h1>
            </div>
            <p class="error-description">
              {{ description }}
            </p>

            <button
              type="button"
              class="folio-focus-ring error-return"
              @click="returnToIndex">
              Return to project index
              <span aria-hidden="true">→</span>
            </button>
          </div>
        </section>
      </main>
    </div>
  </UApp>
</template>

<style scoped>
.error-page {
  min-height: 100vh;
  color: var(--folio-ink);
  background:
    linear-gradient(to right, transparent 0, transparent calc(100% - 1px), rgb(0 0 0 / 3%) calc(100% - 1px)),
    var(--folio-paper);
}

.error-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  min-height: 4rem;
  padding: .75rem clamp(1rem, 4vw, 3rem);
  border-bottom: 7px solid var(--folio-cyan);
  background: color-mix(in srgb, var(--folio-paper) 94%, transparent);
}

.error-home-link {
  display: inline-flex;
  align-items: center;
}

.error-main {
  display: grid;
  min-height: calc(100vh - 4.45rem);
  place-items: center;
  padding: clamp(2rem, 4vw, 3rem) clamp(1rem, 5vw, 4rem);
}

.error-record {
  display: grid;
  grid-template-columns: minmax(17rem, 1fr) minmax(16rem, .8fr);
  gap: clamp(3rem, 7vw, 7rem);
  align-items: center;
  width: min(100%, 70rem);
}

.error-copy {
  grid-column: 1;
  grid-row: 1;
  width: min(100%, 34rem);
  text-align: left;
}

.error-kicker {
  padding-bottom: .75rem;
  border-bottom: 1px solid var(--folio-ink);
}

.error-title {
  display: flex;
  gap: clamp(1rem, 3vw, 2rem);
  align-items: baseline;
  justify-content: flex-start;
  margin: clamp(1.25rem, 3vw, 2rem) 0 .75rem;
}

.error-code {
  margin: 0;
  color: var(--folio-rust);
  font-family: var(--folio-font-display);
  font-size: clamp(5rem, 12vw, 7.5rem);
  font-stretch: condensed;
  font-weight: 900;
  letter-spacing: -.085em;
  line-height: .75;
}

.error-copy h1 {
  margin: 0;
  font-family: var(--folio-font-display);
  font-size: clamp(2.5rem, 5vw, 4.5rem);
  font-stretch: condensed;
  font-weight: 900;
  letter-spacing: -.04em;
  line-height: .95;
}

.error-description {
  max-width: 32rem;
  color: var(--folio-muted);
  font-family: var(--folio-font-body);
  font-size: clamp(1.05rem, 2vw, 1.3rem);
  line-height: 1.55;
}

.error-return {
  display: inline-flex;
  gap: 1.25rem;
  align-items: center;
  justify-content: space-between;
  margin-top: 2rem;
  padding: .85rem 1rem;
  border: 1px solid var(--folio-ink);
  color: var(--folio-dark-ink);
  background: var(--folio-amber);
  box-shadow: 4px 4px 0 var(--folio-ink);
  font-family: var(--folio-font-mono);
  font-size: .76rem;
  font-weight: 700;
  letter-spacing: .05em;
  cursor: pointer;
  transition: translate 140ms ease, box-shadow 140ms ease;
}

.error-return:hover {
  box-shadow: 2px 2px 0 var(--folio-ink);
  translate: 2px 2px;
}

.error-figure {
  --error-card-paper: #d6d0b8;
  --error-card-ink: #25251f;
  --error-card-muted: #676251;

  display: flex;
  position: relative;
  grid-column: 2;
  grid-row: 1;
  flex-direction: column;
  align-items: center;
  margin: 0;
  width: min(100%, 22rem);
  padding: clamp(1.5rem, 4vw, 2.5rem) clamp(1.5rem, 4vw, 2.5rem) 0;
  border: 1px solid var(--error-card-ink);
  background: var(--error-card-paper);
  box-shadow: 10px 10px 0 var(--folio-rule);
  isolation: isolate;
  overflow: hidden;
}

.error-figure::before,
.error-figure::after {
  position: absolute;
  z-index: 0;
  inset: 0;
  pointer-events: none;
  content: '';
}

.error-figure::before {
  background: url('/pulp-paper-texture.svg') repeat;
  mix-blend-mode: multiply;
  opacity: .16;
}

.error-figure::after {
  box-shadow: inset 0 0 1.6rem rgb(37 37 31 / 16%);
}

.error-face {
  position: relative;
  z-index: 1;
  width: min(100%, 18rem);
  height: auto;
}

.error-figure figcaption {
  position: relative;
  z-index: 1;
  align-self: stretch;
  margin: 1.5rem clamp(-2.5rem, -4vw, -1.5rem) 0;
  padding: .8rem 1rem;
  border-top: 1px solid var(--error-card-ink);
  color: var(--error-card-muted);
  font-family: var(--folio-font-mono);
  font-size: .67rem;
  font-weight: 700;
  letter-spacing: .08em;
  text-align: center;
}

@media (max-width: 50rem) {
  .error-main {
    align-items: start;
  }

  .error-record {
    grid-template-columns: 1fr;
    gap: 2rem;
    width: min(100%, 36rem);
  }

  .error-figure {
    grid-column: 1;
    grid-row: 1;
    justify-self: center;
  }

  .error-copy {
    grid-column: 1;
    grid-row: 2;
    margin-inline: auto;
    text-align: center;
  }

  .error-title {
    justify-content: center;
  }

  .error-description {
    margin-inline: auto;
  }
}

@media (max-width: 30rem) {
  .error-title {
    display: block;
  }

  .error-copy h1 {
    margin-top: .75rem;
    font-size: 2.75rem;
  }
}
</style>
