<script setup>
defineProps({
  fluid: {
    type: Boolean,
    required: false,
    default: false
  },
  fillHeight: {
    type: Boolean,
    required: false,
    default: false
  }
})
</script>

<template>
  <div
    class="flex-container"
    :class="[
      { 'flex-container__fluid' : fluid },
      { 'flex-container__fill-height' : fillHeight }]
    ">
    <slot />
  </div>
</template>

<script>
export default {
  name: 'FlexContainer',
}
</script>

<style lang="scss" scoped>
.flex-container {
  width: 100%;
  padding: 1rem;
  margin-left: auto;
  margin-right: auto;
  @media (min-width: 960px) {
    max-width: 900px;
  }
  @media (min-width: 1280px) {
    max-width: 1200px;
  }
  @media (min-width: 1920px) {
    max-width: 1800px;
  }
  @media (min-width: 2560px) {
    .v-container {
      max-width: 2400px;
    }
  }
  &__fluid {
    max-width: 100%
  }
  &__fill-height {
    align-items: center;
    display: flex;
    flex-wrap: wrap;
  }
}
</style>
