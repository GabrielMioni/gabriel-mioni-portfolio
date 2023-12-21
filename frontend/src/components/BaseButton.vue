<template>
  <router-link
    v-if="renderAsLink"
    :to="to">
    <button class="p-button base-button">
      <slot />
    </button>
  </router-link>
  <button
    v-else
    class="p-button base-button">
    <slot />
  </button>
</template>

<script>
export default {
  name: 'BaseButton',
  props: {
    to: {
      type: String,
      required: false,
      default: null
    }
  },
  computed: {
    renderAsLink () {
      return this.to && this.to.length > 0
    }
  }
}
</script>

<style lang="scss" scoped>
$white: #ffffff;
.base-button {
  background-color: rgba($white, 0.1);
  padding: .5rem 1rem;
  font-size: 14px;
  letter-spacing: 1.25px;
  &.p-button {
    &:hover {
      background-color: rgba($white, 0.2);
    }
  }
}
</style>
