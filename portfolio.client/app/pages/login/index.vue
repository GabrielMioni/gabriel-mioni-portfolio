<script setup lang="ts">
const { email: emailRule, required: requiredRule } = useValidation()

const email = ref('')
const password = ref('')
const isValid = ref(false)

const { apiFetch } = useApiFetch()

const login = async () => {
  try {
    const loginResponse = await apiFetch('/auth/login?useCookies=true', {
      body: {
        email: email.value,
        password: password.value
      }
    })
    console.log(loginResponse)
    const userResponse = await apiFetch('/user')
    console.log(userResponse)
  } catch (e) {
    console.error(e)
  }
}
</script>

<template>
  <v-container>
    <v-row
      align="center"
      justify="center">
      <v-col :cols="4">
        <v-card>
          <v-card-text>
            <v-form v-model="isValid">
              <v-container>
                <v-row>
                  <v-text-field
                    v-model="email"
                    :rules="[emailRule]"
                    label="email" />
                </v-row>
                <v-row>
                  <v-text-field
                    v-model="password"
                    :rules="[requiredRule]"
                    type="password"
                    label="password"/>
                </v-row>
              </v-container>
            </v-form>
          </v-card-text>
          <v-card-actions>
            <v-spacer />
            <v-btn
              class="bg-primary"
              :disabled="!isValid"
              @click="login">
              Sign In
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>

</style>

