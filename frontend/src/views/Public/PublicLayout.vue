<template>
  <div>
    <tool-bar />
    <hero-image />
    <router-view v-slot="{ Component }">
      <transition name="fade">
        <component :is="Component" />
      </transition>
    </router-view>
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
