<script setup lang="ts">
import type { TagEditorItem } from '~/types/tags'
import { generateTagValue } from '~/utils/tags'

const assignedTags = defineModel<TagEditorItem[]>('assignedTags', { default: () => [] })

const props = withDefaults(
  defineProps<{
      disableExisting?: boolean
    }>(),
  {
    disableExisting: false
  }
)

const { allTags, fetchingTags } = useProjectTagQueries()
const { showSnackbar } = useSnackbarStore()

const search = ref('')

const tagItems = computed<TagEditorItem[]>(() => {
  const all = allTags.value.map(t => ({ id: t.id, name: t.name, value: t.value }))
  if (props.disableExisting && !search.value.trim()) return []
  return all
})

const removeTag = (index: number) => {
  assignedTags.value = assignedTags.value.filter((_, i) => i !== index)
}

const resolveTag = (v: string): TagEditorItem => {
  const value = generateTagValue(v)
  const existing = allTags.value.find(t => t.value === value)
  return existing ?? { id: null, name: v, value }
}

const onUpdateModelValue = (values: (TagEditorItem | string)[]) => {
  if (props.disableExisting) {
    const duplicate = values.find(
      v => typeof v === 'string' &&
      allTags.value.some(t => t.value === generateTagValue(v as string))
    )
    if (duplicate) {
      showSnackbar(`"${duplicate}" already exists`, 'warning')
      return
    }
  }
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
      v-model:search="search"
      :model-value="assignedTags"
      :items="tagItems"
      :hide-no-data="false"
      :loading="fetchingTags"
      item-title="name"
      return-object
      label="Tags"
      chips
      multiple
      variant="filled"
      hide-details
      @update:model-value="onUpdateModelValue">
      <template #item="{ item, props: itemProps }">
        <v-list-item
          v-bind="itemProps"
          :disabled="disableExisting && !!item.raw.id"
          :subtitle="disableExisting && item.raw.id ? 'Already exists' : undefined" />
      </template>

      <template #no-data>
        <v-list-item>
          <template #prepend>
            <v-icon
              icon="mdi-plus"
              class="mr-2" />
          </template>
          <v-list-item-title>
            Press enter to create tag
          </v-list-item-title>
        </v-list-item>
      </template>

      <template #chip="{ item, index }">
        <v-tooltip
          :disabled="!!item.raw?.id"
          class="fs-10"
          content-class="font-italic"
          text="Pending"
          location="top">
          <template #activator="{ props }">
            <v-chip
              v-bind="props"
              :key="`tag-chip-${index}`"
              :value="item.raw.value"
              :color="item.raw?.id ? 'primary' : 'warning'"
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
        </v-tooltip>
      </template>
    </v-combobox>
  </div>
</template>
