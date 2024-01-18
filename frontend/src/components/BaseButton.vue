<template>
  <router-link
    v-if="renderAsLink"
    :to="to">
    <button
      class="p-button base-button"
      :class="{ disabled }"
      :disabled="disabled">
      <slot />
    </button>
  </router-link>
  <button
    v-else
    class="p-button base-button"
    :class="{ disabled }"
    :disabled="disabled">
    <slot />
  </button>
</template>

<script>
export default {
  name: 'BaseButton',
  props: {
    disabled: {
      type: Boolean,
      required: false,
      default: false
    },
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
  padding: .5rem 1rem;
  font-size: 14px;
  letter-spacing: 1.25px;
  &.p-button {
    &:not(.disabled) {
      &:after {
        content: '';
        top: 0;
        left: 0;
        position: absolute;
        height: 100%;
        width: 100%;
        transition: background-color 1s;
      }
      &:hover:after {
        background-color: rgba($white, 0.25);
      }
    }
  }
}
</style>
