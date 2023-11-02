<script setup>
import { ref } from 'vue'
import { makeRequiredRules, urlRule } from '@/rules/index.js'

const name = ref('')
const description = ref('')
const isValid = ref(false)
const repositoryUrl = ref('')

const nameRule = makeRequiredRules('Name')
const descriptionRule = makeRequiredRules('Description')

</script>

<template>
  <v-card>
    <v-card-text>
      <v-form v-model="isValid">
        <v-container fluid>
          <v-row>
            <v-col
              cols="12"
              class="pb-1">
              <v-text-field
                v-model="name"
                class="pb-1"
                label="Name"
                variant="solo-filled"
                :rules="[nameRule]" />
            </v-col>
          </v-row>
          <v-row>
            <v-col
              cols="12"
              class="pb-1">
              <v-text-field
                v-model="repositoryUrl"
                class="pb-1"
                label="Repo URL"
                variant="solo-filled"
                :rules="[urlRule]" />
            </v-col>
          </v-row>
          <v-row>
            <v-col
              cols="12"
              class="pb-1">
              <v-textarea
                v-model="description"
                label="Description"
                variant="solo-filled"
                :rules="[descriptionRule]" />
            </v-col>
          </v-row>
        </v-container>
      </v-form>
      <v-card-actions>
        <v-spacer />
        <v-btn
          variant="flat">
          Cancel
        </v-btn>
        <v-btn
          color="primary"
          variant="flat"
          :disabled="!isValid">
          Submit
        </v-btn>
      </v-card-actions>
    </v-card-text>
  </v-card>
</template>

<script>
export default {
  name: 'AddProject'
}
</script>

<style scoped>

</style>
