<script setup lang="ts">
definePageMeta({ layout: 'auth' })

const route = useRoute()
const { public: { authBase } } = useRuntimeConfig()

const errorMessages: Record<string, string> = {
  account_not_authorized: 'This account is signed in but is not authorized to access the admin application.',
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
  const loginUrl = new URL(
    `${normalizedAuthBase}/auth/github/login`,
    window.location.origin
  )
  const returnUrl = route.query.returnUrl

  if (typeof returnUrl === 'string') {
    loginUrl.searchParams.set('returnUrl', returnUrl)
  }

  window.location.assign(loginUrl)
}
</script>

<template>
  <v-container
    class="admin-login fill-height"
    fluid>
    <v-row
      align="center"
      justify="center">
      <v-col
        cols="12"
        sm="8"
        md="5"
        lg="4">
        <v-card
          class="admin-login__card"
          elevation="0">
          <div class="admin-login__heading">
            <div class="admin-login__mark">
              <span
                aria-hidden="true"
                class="admin-login__owl" />
            </div>
            <div>
              <p class="admin-login__label">Authorized access</p>
              <h1>Portfolio admin</h1>
            </div>
          </div>
          <v-card-text class="admin-login__body">
            <p class="admin-login__system-label">
              Management console / GitHub authentication
            </p>
            <p class="admin-login__description">
              Sign in with the GitHub account authorized to manage this portfolio.
            </p>

            <v-alert
              v-if="authenticationError"
              class="admin-login__error"
              type="error"
              variant="tonal">
              {{ authenticationError }}
            </v-alert>

            <v-btn
              block
              color="primary"
              class="admin-login__button"
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

<style scoped>
.admin-login {
  background:
    linear-gradient(
      135deg,
      transparent 74%,
      color-mix(in srgb, rgb(var(--v-theme-cyan)) 17%, transparent) 74%
    ),
    rgb(var(--v-theme-paper));
  min-height: 100vh;
  padding: clamp(1.25rem, 4vw, 3rem);
}

.admin-login__card {
  background: rgb(var(--v-theme-paper-raised));
  border: 1px solid rgb(var(--v-theme-ink));
  border-radius: 0;
  border-top: 7px solid rgb(var(--v-theme-cyan));
  box-shadow: 12px 12px 0 color-mix(in srgb, rgb(var(--v-theme-ink)) 22%, transparent);
}

.admin-login__heading {
  align-items: center;
  border-bottom: 1px solid rgb(var(--v-theme-rule));
  display: flex;
  gap: 1rem;
  padding: 1.5rem;
}

.admin-login__mark {
  align-items: center;
  background: rgb(var(--v-theme-amber));
  border: 1px solid rgb(var(--v-theme-ink));
  display: flex;
  flex: 0 0 3.5rem;
  height: 3.5rem;
  justify-content: center;
}

.admin-login__owl {
  background: rgb(var(--v-theme-on-warning));
  height: 2rem;
  mask: url('/owl-icon.svg') center / contain no-repeat;
  width: 2rem;
  -webkit-mask: url('/owl-icon.svg') center / contain no-repeat;
}

.admin-login__label,
.admin-login__system-label {
  font-family: var(--admin-font-mono);
  font-size: 0.68rem;
  font-weight: 700;
  letter-spacing: 0.09em;
  text-transform: uppercase;
}

.admin-login__label {
  color: rgb(var(--v-theme-rust));
  margin-bottom: 0.2rem;
}

.admin-login h1 {
  color: rgb(var(--v-theme-ink));
  font-family: var(--admin-font-display);
  font-size: clamp(1.8rem, 5vw, 2.4rem);
  font-weight: 850;
  letter-spacing: -0.03em;
  line-height: 1;
}

.admin-login__body {
  padding: 1.5rem;
}

.admin-login__system-label {
  color: rgb(var(--v-theme-rust));
  margin-bottom: 0.65rem;
}

.admin-login__description {
  color: rgb(var(--v-theme-muted));
  font-family: var(--admin-font-body);
  line-height: 1.6;
  margin-bottom: 1.5rem;
}

.admin-login__error {
  border-radius: 0;
  margin-bottom: 1.5rem;
  text-align: left;
}

.admin-login__button {
  border-radius: 0;
  font-weight: 700;
}

@media (max-width: 599px) {
  .admin-login__heading {
    align-items: flex-start;
  }

  .admin-login__card {
    box-shadow: 7px 7px 0 color-mix(in srgb, rgb(var(--v-theme-ink)) 22%, transparent);
  }
}
</style>
