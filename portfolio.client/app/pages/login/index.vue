<script setup lang="ts">
const email = ref('')
const password = ref('')

const isValid = computed(() => email.value.length > 0 && password.value.length > 0)

const { apiFetch } = useApiFetch()

const login = async () => {
  try {
    const result = await apiFetch('/auth/login', {
      body: {
        email: email.value,
        password: password.value
      }
    })
    console.log(result)
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
            <v-form>
              <v-container>
                <v-row>
                  <v-text-field
                    v-model="email"
                    label="email" />
                </v-row>
                <v-row>
                  <v-text-field
                    v-model="password"
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

