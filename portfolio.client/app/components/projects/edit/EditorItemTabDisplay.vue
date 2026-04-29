<script setup lang="ts" generic="T extends HasId">
import type { HasId } from '~/types/editor-items'

const props = defineProps<{
  label: string
  items?: T[]
}>()

const activeCount = computed(() => props.items?.length ?? 0)

const pendingCount = computed(() =>
  props.items?.filter(item => !item.id).length ?? 0
)

</script>

<template>
  <div>
    {{ label }} (<span>{{ activeCount }}</span
    ><template v-if="pendingCount > 0"
    ><span> • </span
    ><span>{{ pendingCount }} pending</span></template
    >)
  </div>
</template>

<style scoped>

</style>
