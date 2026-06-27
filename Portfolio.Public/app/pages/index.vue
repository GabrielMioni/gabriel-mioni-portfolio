<template>
  <div>
    <UPageHero
      title="Gabriel Mioni"
      description="Fullstack developer. I build clean, maintainable web applications with Vue and TypeScript."
      :links="[{
        label: 'GitHub',
        to: 'https://github.com/gabrielmioni',
        target: '_blank',
        icon: 'i-simple-icons-github',
        size: 'xl',
        color: 'neutral',
        variant: 'subtle'
      }, {
        label: 'Contact',
        to: 'mailto:gabrielmioni@gmail.com',
        trailingIcon: 'i-lucide-mail',
        size: 'xl',
        variant: 'subtle'
      }]"
      :ui="{
        header: 'flex flex-col',
        headline: 'max-w-4xl',
        title: 'text-4xl sm:text-5xl',
        description: 'max-w-4xl mx-auto text-lg sm:text-xl',
        links: 'flex flex-col sm:flex-row gap-3 justify-center'
      }" />
    <UContainer class="py-12">
      <div class="flex items-center gap-4 mb-8">
        <span class="text-xs font-semibold uppercase tracking-widest text-primary">Projects</span>
        <div class="flex-1 h-px bg-stone-200 dark:bg-stone-700" />
      </div>
      <ProjectsGrid />
    </UContainer>
  </div>
</template>
