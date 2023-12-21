<template>
  <toolbar class="tool-bar py-0">
    <template #start>
      <span class="tool-bar__title">
        <slot name="start" />
      </span>
    </template>
    <template #end>
      <slot name="end" />
    </template>
  </toolbar>
</template>

<script>
export default {
  name: 'ToolBarNew'
}
</script>

<style lang="scss" scoped>
.tool-bar {
  background-color: #47c6c2;
  border-radius: 0;
  color: #ffffff;
  &__title {
    font-size: 1.25rem;
  }
}
</style>
