import { expect, test } from '@playwright/test'
import {
  signInAsAdmin,
  signInAsUser
} from './helpers/authentication.js'

const adminOrigin = 'http://127.0.0.1:3100'
const endpointAvailabilityQuery = {
  query: `
    query EndpointAvailability {
      __typename
    }
  `
}

test('an unauthenticated user is redirected to login', async ({ page }) => {
  await page.goto('/projects')

  await expect(page).toHaveURL((url) =>
    url.pathname === '/login'
    && url.searchParams.get('returnUrl') === '/projects'
  )
  await expect(
    page.getByRole('button', { name: 'Sign in with GitHub' })
  ).toBeVisible()
})

test('an authenticated admin can access the Admin API', async ({ page }) => {
  await signInAsAdmin(page)

  const sessionResponse = await page.request.get(`${adminOrigin}/api/me`)
  expect(sessionResponse.status()).toBe(200)
  await expect(sessionResponse.json()).resolves.toMatchObject({
    isAuthenticated: true,
    name: 'e2e-admin'
  })

  const graphQlResponse = await page.request.post(
    `${adminOrigin}/graphql/admin`,
    { data: endpointAvailabilityQuery }
  )
  expect(graphQlResponse.status()).toBe(200)
  await expect(graphQlResponse.json()).resolves.toMatchObject({
    data: { __typename: 'Query' }
  })

  await page.goto('/projects')
  await expect(
    page.getByRole('link', { name: 'Add Project' })
  ).toBeVisible()
})

test('an authenticated non-admin is denied access to the Admin app and API', async ({
  page
}) => {
  await signInAsUser(page)

  const sessionResponse = await page.request.get(`${adminOrigin}/api/me`)
  expect(sessionResponse.status()).toBe(403)

  const graphQlResponse = await page.request.post(
    `${adminOrigin}/graphql/admin`,
    { data: endpointAvailabilityQuery }
  )
  expect(graphQlResponse.status()).toBe(403)

  await page.goto('/projects')
  await expect(page).toHaveURL((url) =>
    url.pathname === '/login'
    && url.searchParams.get('returnUrl') === '/projects'
    && url.searchParams.get('error') === 'account_not_authorized'
  )
  await expect(page.getByText(
    'This account is signed in but is not authorized to access the admin application.'
  )).toBeVisible()
})

test('signing out removes access to authenticated routes', async ({ page }) => {
  await signInAsAdmin(page)
  await page.goto('/projects')

  await page.getByRole('button', { name: 'Sign out' }).click()

  await expect(page).toHaveURL('/login')

  const sessionResponse = await page.request.get(`${adminOrigin}/api/me`)
  expect(sessionResponse.status()).toBe(401)
})
