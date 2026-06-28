<script setup>
useHead({
  meta: [
    { name: 'viewport', content: 'width=device-width, initial-scale=1' }
  ],
  link: [
    { rel: 'icon', href: '/favicon.ico' }
  ],
  htmlAttrs: {
    lang: 'en'
  }
})

const title = 'Gabriel Mioni — Fullstack Developer'
const description = 'Fullstack developer with 15 years of experience building web applications with Vue, TypeScript, and .NET.'

useSeoMeta({
  title,
  description,
  ogTitle: title,
  ogDescription: description,
  twitterCard: 'summary'
})
</script>

<template>
  <UApp>
    <UHeader>
      <template #left>
        <NuxtLink to="/">
          <AppLogo class="w-auto h-6 shrink-0" />
        </NuxtLink>
      </template>

      <template #right>
        <UColorModeButton />
      </template>
    </UHeader>

    <UMain>
      <NuxtPage />
    </UMain>

    <UFooter>
      <template #left>
        <p class="text-sm text-muted">
          Gabriel Mioni • © {{ new Date().getFullYear() }}
        </p>
      </template>

      <template #right>
        <UButton
          to="https://github.com/gabrielmioni"
          target="_blank"
          icon="i-simple-icons-github"
          aria-label="GitHub"
          color="neutral"
          variant="ghost" />
      </template>
    </UFooter>
  </UApp>
</template>
