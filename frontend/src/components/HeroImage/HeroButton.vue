<template>
  <v-btn
    color="transparent"
    elevation="0"
    rounded="0"
    class="hero-button">
    <span class="text-white pa-2">
      <slot />
    </span>
  </v-btn>
</template>

<style lang="scss" scoped>
.hero-button {
  border: 2px solid #fff;
}
</style>
