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
              Full-stack developer with 15 years of experience building durable applications for the web.
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
            <div
              class="technology-index"
              aria-label="Primary technologies">
              <span>Vue</span>
              <span>TypeScript</span>
              <span>Nuxt</span>
              <span>.NET</span>
              <span>C#</span>
              <span>GraphQL</span>
            </div>
          </div>

          <ManualPlate />
        </div>
      </UContainer>
    </section>

    <section class="projects-section">
      <UContainer>
        <div class="projects-heading">
          <h2>Selected systems</h2>
          <div class="editorial-rule" />
          <span class="editorial-kicker">Project index</span>
        </div>
        <ProjectsGrid />
      </UContainer>
    </section>
  </div>
</template>
