<template>
  <div>
    <tool-bar />
    <hero-image />
    <v-container>
      <v-row style="justify-content: center">
        <v-col :cols="8">
          <router-view v-slot="{ Component }">
            <transition name="fade">
              <component :is="Component" />
            </transition>
          </router-view>
        </v-col>
      </v-row>
    </v-container>
    <footer-section />
  </div>
</template>

<script>
import ToolBar from '@/views/Public/components/ToolBar.vue'
import HeroImage from '@/views/Public/components/HeroImage/HeroImage.vue'
import FooterSection from '@/views/Public/components/FooterSection.vue'

export default {
  components: {
    ToolBar,
    HeroImage,
    FooterSection,
  },
}
</script>

<style lang="scss" scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.15s;
}
.fade-enter, .fade-leave-to {
  opacity: 0;
}
</style>
