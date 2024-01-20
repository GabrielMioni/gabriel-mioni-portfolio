<script setup>
import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'
import FlexRow from '@/components/flex/FlexRow.vue'

</script>

<template>
  <div>
    <main>
      <router-view v-slot="{ Component }">
        <flex-container fluid>
          <flex-row>
            <flex-column :cols="12">
              <transition name="fade">
                <component :is="Component" />
              </transition>
            </flex-column>
          </flex-row>
        </flex-container>
      </router-view>
    </main>
  </div>
</template>

<script>
export default {
  name: 'AdminLayout'
}
</script>
