<template>
  <button class="p-button base-button">
    <slot />
  </button>
</template>

<script>
export default {
  name: 'BaseButton'
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
