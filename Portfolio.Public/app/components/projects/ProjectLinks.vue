<script setup lang="ts">
import { PublicProjectLinkFragmentDoc } from '~/generated/graphql'
import { useFragment, type FragmentType } from '~/generated'

const props = defineProps<{
  links: FragmentType<typeof PublicProjectLinkFragmentDoc>[]
}>()

const resolvedLinks = computed(() =>
  props.links.map(l => useFragment(PublicProjectLinkFragmentDoc, l))
)
</script>

<template>
  <div class="project-links">
    <a
      v-for="link in resolvedLinks"
      :key="link.id"
      :href="link.url"
      class="project-link folio-focus-ring hover:text-dark-ink"
      target="_blank"
      rel="noopener noreferrer">
      <UIcon
        :name="linkIcon[link.linkType] ?? 'i-lucide-link'"
        class="project-link__type-icon"
        aria-hidden="true" />
      <span>{{ link.linkText }}</span>
      <UIcon
        name="i-lucide-arrow-up-right"
        class="project-link__external-icon"
        aria-hidden="true" />
    </a>
  </div>
</template>

<style scoped>
.project-links {
  display: flex;
  flex-wrap: wrap;
  gap: .5rem;
}

.project-link {
  display: inline-flex;
  gap: .45rem;
  align-items: center;
  min-height: 2.15rem;
  padding: .4rem .55rem;
  border: 1px solid var(--folio-ink);
  color: var(--folio-ink);
  background: transparent;
  font-family: var(--folio-font-mono);
  font-size: .68rem;
  font-weight: 700;
  letter-spacing: .025em;
  line-height: 1;
  text-decoration: none;
  transition: background-color 140ms ease, box-shadow 140ms ease, translate 140ms ease;
}

.project-link:hover {
  background: var(--folio-amber);
  box-shadow: 3px 3px 0 var(--folio-ink);
  translate: -2px -2px;
}

.project-link__type-icon {
  width: 1rem;
  height: 1rem;
}

.project-link__external-icon {
  width: .8rem;
  height: .8rem;
  margin-left: .1rem;
}
</style>
