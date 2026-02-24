<script setup lang="ts">
import type { Project } from '~/generated/graphql'
import { formatDate, formatTextWithEllipsis } from '~/utils/formatters'

const props = defineProps<{
  project: Project
}>()

const dash = '-'

const maybeText = (val?: string | null, ellipsis = true) => {
  if (!val) return dash
  return ellipsis ? formatTextWithEllipsis(val) : val
}

const view = computed(() => ({
  title: props.project.title,
  summary: maybeText(props.project.summary),
  body: maybeText(props.project.body),
  createdAt: props.project.createdAt ? formatDate(props.project.createdAt) : dash
}))

</script>

<template>
  <tr>
    <td v-text="view.title" />
    <td v-text="view.summary" />
    <td v-text="view.body" />
    <td v-text="view.createdAt" />
    <td>
      <ProjectStatus :project="project" />
    </td>
    <td class="text-end">
      <ProjectMenu :project-id="project.id"/>
    </td>
  </tr>
</template>

<style scoped>

</style>
