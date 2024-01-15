<script setup>
import TextField from '@/components/inputs/TextField/TextField.vue'
import BaseForm from '@/components/inputs/BaseForm.vue'
import { ref } from 'vue'
import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexRow from '@/components/flex/FlexRow.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'
import BaseButton from '@/components/BaseButton.vue'
import TextArea from '@/components/inputs/TextArea.vue'

const name = ref('')
const nameTwo = ref('')
const isValid = ref(null)

const validationRule = (value) => {
  if (!value) {
    return 'Required!'
  }
  return true
}

const minimumChars = (value) => {
  if (value.trim().length < 8) {
    return 'Minimum characters is 8'
  }
  return true
}

const handleSubmit = ($event) => {
  console.log($event)
}

</script>

<template>
  <div>
    <base-form v-model="isValid">
      <flex-container>
        <flex-row>
          <flex-column>
            <text-field
              v-model="name"
              label="Main"
              field-name="main"
              float-label
              help-text="Blah"
              prepend-icon="pi-search"
              :rules="[
                validationRule,
                minimumChars
              ]" />
          </flex-column>
        </flex-row>
        <flex-row>
          <flex-column>
            <text-area
              v-model="nameTwo"
              label="Haha"
              field-name="second" />
          </flex-column>
        </flex-row>
        <flex-row>
          <flex-column>
            <base-button
              :disabled="!isValid"
              @click.prevent="handleSubmit">
              Submit
            </base-button>
          </flex-column>
        </flex-row>
      </flex-container>
    </base-form>
    <pre>
      {{ isValid }}
    </pre>
  </div>
</template>

<script>

export default {
  name: 'PrimeVue'
}
</script>

<style scoped>

</style>
