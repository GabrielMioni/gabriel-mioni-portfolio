<script setup>
import image from '@/assets/nature-forest-trees-fog.jpeg'
import HeroButton from '@/components/HeroButton.vue'
</script>

<template>
  <v-parallax
    :src="image"
    class="hero-image"
    height="300"
    scale="3">
    <v-row
      class="fill-height"
      align="center"
      justify="center">
      <v-col>
        <v-row
          justify="center"
          class="text-white text-center">
          <div class="hero-text">
            Web developer.<br>
            Available for hire.
          </div>
        </v-row>
        <v-row justify="center">
          <v-col cols="auto">
            <hero-button>
              My Projects
            </hero-button>
          </v-col>
          <v-col cols="auto">
            <hero-button>
              Contact Me
            </hero-button>
          </v-col>
        </v-row>
      </v-col>
    </v-row>
  </v-parallax>
</template>

<script>
export default {
  name: 'HeroImage'
}
</script>

<style lang="scss" scoped>
.hero-image {
  .hero-text {
    font-size: 2rem;
    line-height: 1.25;
    z-index: 1;
  }
  &::before {
    content: "";
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.25);
  }
}
</style>
