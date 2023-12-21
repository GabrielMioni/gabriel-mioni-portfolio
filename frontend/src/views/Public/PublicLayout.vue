<template>
  <div>
    <tool-bar>
      <template #start>
        Gabriel Mioni
      </template>
      <template #end>
        <div class="flex">
          <flex-column>
            <base-button>
              PROJECTS
            </base-button>
          </flex-column>
          <flex-column>
            <base-button>
              Contact
            </base-button>
          </flex-column>
        </div>
      </template>
    </tool-bar>
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
import BaseButton from '@/components/BaseButton.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'
import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexRow from '@/components/flex/FlexRow.vue'
import FooterSection from '@/views/Public/components/FooterSection.vue'
import HeroImage from '@/views/Public/components/HeroImage/HeroImage.vue'
import ToolBar from '@/views/Public/components/ToolBar.vue'

export default {
  components: {
    BaseButton,
    FlexColumn,
    FlexContainer,
    FlexRow,
    FooterSection,
    HeroImage,
    ToolBar
  }
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
