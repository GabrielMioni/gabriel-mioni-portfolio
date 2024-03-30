<script setup>
import { ref } from 'vue'
import { makeRequiredRule } from '@/rules/index.js'

import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexRow from '@/components/flex/FlexRow.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'

import BaseButton from '@/components/BaseButton.vue'
import BaseForm from '@/components/inputs/BaseForm.vue'
import TextArea from '@/components/TextArea.vue'
import TextField from '@/components/inputs/TextField/TextField.vue'

const name = ref('')
const email = ref('')
const message = ref('')

const nameRule = makeRequiredRule('Name')
const emailRule = makeRequiredRule('Email')
const messageRule = makeRequiredRule('Message')

</script>

<template>
  <base-form v-slot="{ isValid }">
    <card>
      <template #content>
        <flex-container fluid>
          <flex-row>
            <flex-column>
              <text-field
                v-model="name"
                label="Name"
                field-name="name"
                float-label
                small
                :rules="[nameRule]" />
            </flex-column>
          </flex-row>
          <flex-row>
            <flex-column>
              <text-field
                v-model="email"
                label="Email"
                field-name="email"
                float-label
                small
                :rules="[emailRule]" />
            </flex-column>
          </flex-row>
          <flex-row>
            <flex-column>
              <text-area
                v-model="message"
                label="Message"
                field-name="message"
                float-label
                :rules="[messageRule]" />
            </flex-column>
          </flex-row>
        </flex-container>
      </template>
      <template #footer>
        <flex-container class="py-0">
          <flex-row>
            <flex-column>
              <span class="flex justify-content-end">
                <base-button :disabled="!isValid">
                  Submit
                </base-button>
              </span>
            </flex-column>
          </flex-row>
        </flex-container>
      </template>
    </card>
  </base-form>
</template>

<script>
export default {
  name: 'ContactFormV2'
}
</script>

<style scoped lang="scss">
::v-deep(.p-card-content) {
  padding: 1rem 0;
}
::v-deep(.p-card .p-card-footer) {
  padding: 0;
}
</style>
