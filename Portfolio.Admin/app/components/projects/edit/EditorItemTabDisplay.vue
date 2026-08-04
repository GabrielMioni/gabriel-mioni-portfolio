<script setup lang="ts" generic="T extends HasId & HasIsRemoved">
import type { HasId, HasIsRemoved } from '~/types/editor-items'

const props = defineProps<{
  label: string
  items?: T[]
}>()

const activeCount = computed(() =>
  props.items?.filter(item => !item.isRemoved).length ?? 0
)

const pendingCount = computed(() =>
  props.items?.filter(item => !item.id && !item.isRemoved).length ?? 0
)

const removedCount = computed(() =>
  props.items?.filter(item => item.isRemoved).length ?? 0
)

const accessibleLabel = computed(() => {
  const states = [`${props.label}: ${activeCount.value} active`]

  if (pendingCount.value > 0) {
    const additions = pendingCount.value === 1
      ? 'pending addition'
      : 'pending additions'
    states.push(`${pendingCount.value} ${additions}`)
  }

  if (removedCount.value > 0) {
    const removals = removedCount.value === 1
      ? 'pending removal'
      : 'pending removals'
    states.push(`${removedCount.value} ${removals}`)
  }

  return states.join(', ')
})
</script>

<template>
  <div>
    <span aria-hidden="true">
      {{ label }} (<span>{{ activeCount }}</span>)
      <span
        v-if="pendingCount > 0"
        class="text-success">
        +{{ pendingCount }}
      </span>
      <span
        v-if="removedCount > 0"
        class="text-error">
        −{{ removedCount }}
      </span>
    </span>
    <span class="d-sr-only">{{ accessibleLabel }}</span>
  </div>
</template>

<style scoped>

</style>
