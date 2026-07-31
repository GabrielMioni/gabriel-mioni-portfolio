import { expect, test, type Page } from '@playwright/test'

const publicOrigin = 'http://127.0.0.1:3101'

const waitForPublishedProjectsResponse = (page: Page) =>
  page.waitForResponse((response) => {
    const url = new URL(response.url())

    return url.pathname === '/graphql'
      && url.searchParams.get('operationName') === 'GetPublishedProjects'
      && response.ok()
  })

test('a published project can be created, viewed, and deleted', async ({
  browser,
  page
}) => {
  const uniqueSuffix = crypto.randomUUID().slice(0, 8)
  const title = `E2E Project ${uniqueSuffix}`
  const summary = 'Created by the browser lifecycle test.'

  await page.goto('/projects/create')
  await page.getByLabel('Title').fill(title)
  await page.getByLabel('Summary').fill(summary)
  await page.getByLabel('Published').check()
  await page.getByRole('button', { name: 'Save', exact: true }).click()

  await expect(page).toHaveURL(/\/projects\/[0-9a-f-]+\/?$/)
  await expect(page.getByText('Project created.', { exact: true })).toBeVisible()

  const publicContext = await browser.newContext()
  const publicPage = await publicContext.newPage()
  const initialProjectsResponse = waitForPublishedProjectsResponse(publicPage)

  await publicPage.goto(publicOrigin)
  await initialProjectsResponse

  await expect(publicPage.getByText(title, { exact: true })).toBeVisible()
  await expect(publicPage.getByText(summary, { exact: true })).toBeVisible()

  await page.getByRole('button', { name: 'Delete', exact: true }).click()
  const confirmationDialog = page.getByRole('dialog')
  await confirmationDialog
    .getByRole('button', { name: 'Delete', exact: true })
    .click()

  await expect(page).toHaveURL(url => url.pathname === '/projects')
  await expect(
    page.getByText('Project deleted successfully', { exact: true })
  ).toBeVisible()

  const refreshedProjectsResponse =
    waitForPublishedProjectsResponse(publicPage)
  await publicPage.reload()
  await refreshedProjectsResponse

  await expect(publicPage.getByText(title, { exact: true })).toHaveCount(0)

  await publicContext.close()
})
