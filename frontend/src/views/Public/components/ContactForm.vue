<script setup>
import { ref } from 'vue'
import { makeRequiredRule } from '@/rules/index.js'

import BaseForm from '@/components/inputs/BaseForm.vue'
import TextField from '@/components/inputs/TextField/TextField.vue'
import TextArea from '@/components/inputs/TextArea.vue'

import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexRow from '@/components/flex/FlexRow.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'
import BaseButton from '@/components/BaseButton.vue'

const name = ref('')
const email = ref('')
const message = ref('')
const formIsValid = ref(false)

const nameRule = makeRequiredRule('Name')
const emailRule = makeRequiredRule('Email')
const messageRule = makeRequiredRule('Message')
</script>

<template>
  <base-form
    v-model="formIsValid"
    class="contact-form">
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
          <flex-row>
            <flex-column />
            <flex-column :cols="1">
              <base-button :disabled="!formIsValid">
                Submit
              </base-button>
            </flex-column>
          </flex-row>
        </flex-container>
      </template>
    </card>
  </base-form>
</template>

<style lang="scss" scoped>
.contact-form {
  &__card {
    background-color: transparent;
  }
}
</style>
