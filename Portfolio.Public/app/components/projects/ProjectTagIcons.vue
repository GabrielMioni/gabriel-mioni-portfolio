<script setup lang="ts">
import { PublicProjectTagFragmentDoc } from '~/generated/graphql'
import { useFragment, type FragmentType } from '~/generated'
import { tagIconMap } from '~/utils/tagIcons'

const props = defineProps<{
  tags: FragmentType<typeof PublicProjectTagFragmentDoc>[]
}>()

const tagIcons = computed(() =>
  props.tags
    .map(t => useFragment(PublicProjectTagFragmentDoc, t))
    .map(t => ({ name: t.name, icon: tagIconMap[t.value] }))
    .filter(t => t.icon)
)
</script>

<template>
  <div
    v-if="tagIcons.length"
    class="flex gap-2">
    <UTooltip
      v-for="tag in tagIcons"
      :key="tag.icon"
      :text="tag.name">
      <UIcon
        :name="tag.icon"
        class="size-4 text-stone-400 dark:text-stone-500" />
    </UTooltip>
  </div>
</template>
