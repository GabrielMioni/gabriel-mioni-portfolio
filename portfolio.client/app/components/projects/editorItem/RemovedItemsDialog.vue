<script setup lang="ts" generic="T extends { clientId: string }">
const dialog = defineModel<boolean>()

const props = defineProps<{
  title: string
  items: T[]
}>()

defineEmits<{
  (e: 'add', clientId: string): void
}>()

const itemCount = computed(() => props.items.length)

watch(
  itemCount,
  (val) => {
    if (val <= 0) {
      dialog.value = false
    }
  }
)
</script>

<template>
  <BaseDialog
    v-model="dialog"
    hide-toolbar>
    <template #card-title>
      <div class="d-flex align-center">
        <span class="fs-14 font-weight-bold">
          {{ title }}
        </span>
        <v-spacer />
        <v-btn
          icon="mdi-close"
          flat
          small
          density="compact"
          @click="dialog = false" />
      </div>
    </template>
    <div>
      <slot
        v-for="item in items"
        :key="item.clientId"
        name="item"
        :item="item"
        :restore="() => $emit('add', item.clientId)" />
    </div>
    <template #actions>
      <v-spacer />
      <v-btn
        variant="flat"
        @click="dialog = false">
        Close
      </v-btn>
    </template>
  </BaseDialog>
</template>
