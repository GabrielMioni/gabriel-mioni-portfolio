<script setup lang="ts">
import type { Project } from '~/generated/graphql'
import { formatDate, formatTextWithEllipsis } from '~/utils/formatters'

const props = defineProps<{
  project: Project
  toggleExpand?: () => void
  isExpanded?: boolean
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
  <tr :class="isExpanded ? 'expanded' : undefined">
    <td>
      <div class="d-flex align-center">
        <v-icon
          size="small"
          class="mr-2"
          @click.stop="props.toggleExpand?.()"
          @keydown.enter.stop="props.toggleExpand?.()">
          {{ isExpanded ? 'mdi-chevron-down' : 'mdi-chevron-right' }}
        </v-icon>
        <div>
          <div
            class="font-weight-bold"
            v-text="view.title" />
          <div class="text-grey-darken-1 fs-12">
            Created: <span v-text="view.createdAt" />
          </div>
        </div>
      </div>
    </td>
    <td v-text="view.summary" />
    <td>
      <ProjectStatus :project="project" />
    </td>
    <td class="text-end">
      <ProjectMenu :project-id="project.id"/>
    </td>
  </tr>
</template>

<style scoped>
tr.expanded td {
  padding: 16px 24px;
}
</style>
