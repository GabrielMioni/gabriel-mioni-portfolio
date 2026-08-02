<script setup lang="ts">
definePageMeta({ layout: 'auth' })

const route = useRoute()
const { public: { authBase } } = useRuntimeConfig()

const errorMessages: Record<string, string> = {
  github_account_not_allowed: 'This GitHub account does not have access to the admin application.',
  github_authentication_failed: 'GitHub sign-in could not be completed. Please try again.'
}

const authenticationError = computed(() => {
  const errorCode = route.query.error

  return typeof errorCode === 'string'
    ? errorMessages[errorCode]
    : undefined
})

const signInWithGitHub = () => {
  const normalizedAuthBase = authBase.replace(/\/+$/, '')
  const loginUrl = new URL(`${normalizedAuthBase}/auth/github/login`)
  const returnUrl = route.query.returnUrl

  if (typeof returnUrl === 'string') {
    loginUrl.searchParams.set('returnUrl', returnUrl)
  }

  window.location.assign(loginUrl)
}
</script>

<template>
  <v-container class="fill-height">
    <v-row
      align="center"
      justify="center">
      <v-col
        cols="12"
        sm="8"
        md="5"
        lg="4">
        <v-card>
          <v-card-title class="pt-6 text-center">
            Portfolio Admin
          </v-card-title>
          <v-card-text class="text-center">
            <p class="mb-6 text-medium-emphasis">
              Sign in with the GitHub account authorized to manage this portfolio.
            </p>

            <v-alert
              v-if="authenticationError"
              class="mb-6 text-left"
              type="error"
              variant="tonal">
              {{ authenticationError }}
            </v-alert>

            <v-btn
              block
              color="primary"
              prepend-icon="mdi-github"
              size="large"
              @click="signInWithGitHub">
              Sign in with GitHub
            </v-btn>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>
