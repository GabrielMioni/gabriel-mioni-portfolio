<script setup>
import { ref } from 'vue'
import { makeRequiredRule } from '@/rules/index.js'

import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexRow from '@/components/flex/FlexRow.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'

import BaseButton from '@/components/BaseButton.vue'
import BaseFormV2 from '@/components/inputs/BaseFormV2.vue'
import TextAreaV2 from '@/components/TextAreaV2.vue'
import TextFieldV2 from '@/components/inputs/TextField/TextFieldV2.vue'

const name = ref('')
const email = ref('')
const message = ref('')

const nameRule = makeRequiredRule('Name')
const emailRule = makeRequiredRule('Email')
const messageRule = makeRequiredRule('Message')

</script>

<template>
  <base-form-v2 v-slot="{ isValid }">
    <card>
      <template #content>
        <flex-container fluid>
          <flex-row>
            <flex-column>
              <text-field-v2
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
              <text-field-v2
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
              <text-area-v2
                v-model="message"
                label="Message"
                field-name="message"
                float-label
                :rules="[messageRule]" />
            </flex-column>
          </flex-row>
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
  </base-form-v2>
</template>

<script>
export default {
  name: 'ContactFormV2'
}
</script>

<style scoped lang="scss">

</style>
