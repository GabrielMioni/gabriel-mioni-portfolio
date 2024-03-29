<script setup>
import profilePic from '@/assets/saturn-eating-his-son.png'
import ContactFormV2 from '@/views/Public/components/ContactFormV2.vue'
import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexRow from '@/components/flex/FlexRow.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'

</script>

<template>
  <div class="footer">
    <flex-container
      fluid
      class="pa-0 container-wrapper">
      <flex-row justify-center>
        <flex-column
          cols="8"
          class="justify-center">
          <flex-row>
            <flex-column
              cols="3"
              class="flex align-center justify-content-center">
              <img
                class="image"
                :src="profilePic">
            </flex-column>
            <flex-column>
              <contact-form-v2 />
            </flex-column>
          </flex-row>
        </flex-column>
      </flex-row>
    </flex-container>
  </div>
</template>

<style lang="scss" scoped>
.footer {
  background-color: #036870;
  .image {
    max-width: 200px;
    border-radius: 50%;
  }
}
</style>
