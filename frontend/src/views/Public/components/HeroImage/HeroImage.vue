<script setup>
import headerImage from '@/assets/nature-forest-trees-fog.jpeg'
</script>

<template>
  <div class="hero-image">
    <img
      :src="headerImage"
      class="hero-image absolute top-0 left-0 h-full w-full"
      alt="hero-image">
    <div class="absolute top-0 left-0 h-full w-full flex hero-text text-white text-center align-items-center justify-content-center">
      <slot />
    </div>
  </div>
</template>

<style lang="scss" scoped>
.hero-image {
  height: 300px;
  position: relative;
  overflow: hidden;
  img {
    object-fit: cover;
    object-position: center;
  }
  .hero-text {
    font-size: 2rem;
    line-height: 1.25;
    z-index: 2;
  }
  &::before {
    content: "";
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.25);
    z-index: 2;
  }
}
</style>
