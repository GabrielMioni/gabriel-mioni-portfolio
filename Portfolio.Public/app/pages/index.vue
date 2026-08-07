<script setup lang="ts">
const config = useRuntimeConfig()
</script>

<template>
  <div>
    <section class="editorial-hero">
      <UContainer>
        <div class="editorial-hero-grid">
          <div>
            <div class="editorial-kicker">
              Public portfolio system · Version {{ config.public.appVersion }}
            </div>
            <h1 class="editorial-title">
              Gabriel<br>Mioni
            </h1>
            <p class="editorial-summary">
              Full-stack developer based in Portland, Oregon, with 15 years of experience building durable
              applications for the web.
            </p>
            <div class="hero-actions">
              <UButton
                label="View GitHub"
                to="https://github.com/gabrielmioni"
                target="_blank"
                icon="i-simple-icons-github"
                color="neutral"
                variant="solid"
                size="lg"
                class="rounded-none" />
              <UButton
                label="Make contact"
                to="mailto:gabrielmioni@gmail.com"
                trailing-icon="i-lucide-arrow-up-right"
                color="primary"
                variant="solid"
                size="lg"
                class="rounded-none" />
            </div>
            <ul
              class="technology-index"
              aria-label="Primary technologies">
              <li>Vue</li>
              <li>TypeScript</li>
              <li>Nuxt</li>
              <li>.NET</li>
              <li>C#</li>
              <li>GraphQL</li>
            </ul>
          </div>

          <ManualPlate />
        </div>
      </UContainer>
    </section>

    <section class="projects-section">
      <UContainer>
        <div class="projects-heading">
          <h2>Project studies</h2>
          <div class="editorial-rule" />
          <svg
            class="project-signal-trace"
            viewBox="0 0 96 24"
            aria-hidden="true">
            <path
              d="M1 12 H14 C18 12 18 4 22 4 S26 20 30 20 S34 7 38 7 S42 16 46 16 S50 10 54 10 S58 14 62 14 S66 12 70 12 H95"
              vector-effect="non-scaling-stroke" />
          </svg>
        </div>
        <ProjectsGrid />
      </UContainer>
    </section>
  </div>
</template>
