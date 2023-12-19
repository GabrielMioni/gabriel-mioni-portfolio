<template>
  <div>
    <tool-bar />
    <hero-image />
    <flex-container>
      <flex-row justify-center>
        <flex-column cols="8">
          <router-view v-slot="{ Component }">
            <transition name="fade">
              <component :is="Component" />
            </transition>
          </router-view>
        </flex-column>
      </flex-row>
    </flex-container>
    <footer-section />
  </div>
</template>

<script>
import ToolBar from '@/views/Public/components/ToolBar.vue'
import HeroImage from '@/views/Public/components/HeroImage/HeroImage.vue'
import FooterSection from '@/views/Public/components/FooterSection.vue'
import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'
import FlexRow from '@/components/flex/FlexRow.vue'

export default {
  components: {
    ToolBar,
    HeroImage,
    FooterSection,
    FlexContainer,
    FlexRow,
    FlexColumn
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
