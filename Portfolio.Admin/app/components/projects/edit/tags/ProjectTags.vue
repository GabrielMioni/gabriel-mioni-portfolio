<script setup lang="ts">
import type { TagEditorItem } from '~/types/tags'
import { generateTagValue } from '~/utils/tags'

const assignedTags = defineModel<TagEditorItem[]>('assignedTags', { default: () => [] })

const { allTags, fetchingTags } = useProjectTagQueries()

const tagItems = computed<TagEditorItem[]>(() =>
  allTags.value.map(t => ({ id: t.id, name: t.name, value: t.value }))
)

const onUpdateModelValue = (values: (TagEditorItem | string)[]) => {
  const normalized = values.map(v =>
    typeof v === 'string'
      ? { id: null, name: v, value: generateTagValue(v) }
      : v
  )

  const seen = new Set<string>()
  assignedTags.value = normalized.filter(v => {
    if (!v.value || seen.has(v.value)) return false
    seen.add(v.value)
    return true
  })
}
</script>

<template>
  <div class="pt-3">
    <v-combobox
      :model-value="assignedTags"
      :items="tagItems"
      :loading="fetchingTags"
      item-title="name"
      return-object
      label="Tags"
      chips
      multiple
      variant="filled"
      hide-details
      @update:model-value="onUpdateModelValue" />
  </div>
</template>
