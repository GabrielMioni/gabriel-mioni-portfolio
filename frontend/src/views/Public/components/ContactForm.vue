<script setup>
import { ref } from 'vue'
import { makeRequiredRule } from '@/rules/index.js'

const name = ref('')
const email = ref('')
const message = ref('')
const formIsValid = ref(false)

const nameRules = makeRequiredRule('Name')
const emailRules = makeRequiredRule('Email')
const messageRules = makeRequiredRule('Message')
</script>

<template>
  <v-form
    v-model="formIsValid"
    class="contact-form">
    <v-card
      flat
      class="contact-form__card">
      <v-card-text class="pb-1">
        <v-text-field
          v-model="name"
          class="mb-3"
          hide-details
          label="Name"
          required
          :rules="nameRules"
          variant="solo-filled" />
        <v-text-field
          v-model="email"
          class="mb-3"
          hide-details
          label="Email"
          required
          :rules="emailRules"
          variant="solo-filled"
          type="email" />
        <v-textarea
          v-model="message"
          label="Message"
          variant="solo-filled"
          :rules="messageRules"
          hide-details />
      </v-card-text>
      <v-card-actions>
        <v-spacer />
        <v-btn
          class="text-none text-subtitle-1 mr-2"
          color="#47c6c2"
          variant="flat"
          :disabled="!formIsValid">
          Submit
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-form>
</template>

<style lang="scss" scoped>
.contact-form {
  &__card {
    background-color: transparent;
  }
}
</style>
