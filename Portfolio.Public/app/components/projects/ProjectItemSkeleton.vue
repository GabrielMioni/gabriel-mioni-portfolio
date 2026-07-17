<template>
  <UCard :ui="{ header: 'p-0' }">
    <template #header>
      <div class="h-[250px] content-center">
        <USkeleton class="w-full h-full" />
      </div>
    </template>
    <div class="space-y-2">
      <USkeleton class="h-6 w-2/3" />
      <USkeleton class="h-4 w-full" />
      <USkeleton class="h-4 w-5/6" />
      <div class="flex gap-2 pt-1">
        <USkeleton
          v-for="n in 3"
          :key="n"
          class="size-4 rounded-full" />
      </div>
    </div>
  </UCard>
</template>
