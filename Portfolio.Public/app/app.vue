<script setup>
useHead({
  meta: [
    { name: 'viewport', content: 'width=device-width, initial-scale=1' }
  ],
  link: [
    { rel: 'icon', type: 'image/svg+xml', href: '/owl-icon.svg' }
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
      <template #top>
        <div class="flex items-center gap-4 px-4 sm:px-6 lg:px-8 py-4">
          <div class="flex-1 h-px bg-stone-200 dark:bg-stone-700" />
          <img
            src="/owl-icon.svg"
            alt="Owl"
            class="size-10">
          <div class="flex-1 h-px bg-stone-200 dark:bg-stone-700" />
        </div>
      </template>
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
