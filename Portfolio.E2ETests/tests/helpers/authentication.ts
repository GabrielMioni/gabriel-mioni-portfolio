import type { Page } from '@playwright/test'

const apiOrigin = 'http://127.0.0.1:5218'
const authenticationToken = process.env.E2E_AUTH_TOKEN

if (!authenticationToken) {
  throw new Error('E2E_AUTH_TOKEN is required for authenticated browser tests.')
}

const signIn = async (page: Page, access: 'admin' | 'user') => {
  const response = await page.request.post(
    `${apiOrigin}/api/auth/e2e/login/${access}`,
    {
      headers: {
        'X-E2E-Auth-Token': authenticationToken
      }
    }
  )

  if (!response.ok()) {
    throw new Error(
      `E2E ${access} login failed with status ${response.status()}.`
    )
  }
}

export const signInAsAdmin = (page: Page) => signIn(page, 'admin')

export const signInAsUser = (page: Page) => signIn(page, 'user')
