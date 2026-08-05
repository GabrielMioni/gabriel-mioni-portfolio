<template>
  <div>
    <section class="editorial-hero">
      <UContainer>
        <div class="editorial-hero-grid">
          <div>
            <div class="editorial-kicker">
              Public portfolio system · Version 1.0.2
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

          <figure>
            <div class="manual-plate">
              <span class="manual-plate-label">Plate A–01</span>
              <div class="manual-plate-mark">
                <img
                  src="/owl-icon.svg"
                  alt="Geometric owl mark"
                  class="manual-plate-owl">
              </div>
            </div>
            <figcaption class="manual-plate-caption">
              <span>Fig. 01 — Systems practice</span>
              <span>Portland, Oregon</span>
            </figcaption>
          </figure>
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
