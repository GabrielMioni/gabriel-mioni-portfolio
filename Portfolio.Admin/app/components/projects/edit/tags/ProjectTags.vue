<script setup lang="ts">
import type { TagEditorItem } from '~/types/tags'
import { generateTagValue } from '~/utils/tags'

const assignedTags = defineModel<TagEditorItem[]>('assignedTags', { default: () => [] })

const { allTags, fetchingTags } = useProjectTagQueries()

const tagItems = computed<TagEditorItem[]>(() =>
  allTags.value.map(t => ({ id: t.id, name: t.name, value: t.value }))
)

const removeTag = (index: number) => {
  assignedTags.value = assignedTags.value.filter((_, i) => i !== index)
}

const resolveTag = (v: string): TagEditorItem => {
  const value = generateTagValue(v)
  const existing = allTags.value.find(t => t.value === value)
  return existing ?? { id: null, name: v, value }
}

const onUpdateModelValue = (values: (TagEditorItem | string)[]) => {
  const normalized = values.map(v =>
    typeof v === 'string' ? resolveTag(v) : v
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
      @update:model-value="onUpdateModelValue">
      <template #chip="{ item, index }">
        <v-chip
          :key="`tag-chip-${index}`"
          :value="item.raw.value"
          :color="item.raw?.id ? 'primary' : 'secondary'"
          :variant="item.raw?.id ? 'flat' : 'outlined'"
          :prepend-icon="item.raw?.id ? undefined : 'mdi-plus'"
          class="ma-1"
          closable
          label
          small
          @mousedown.stop
          @click:close="removeTag(index)">
          {{ item.raw.name }}
        </v-chip>
      </template>
    </v-combobox>
  </div>
</template>
