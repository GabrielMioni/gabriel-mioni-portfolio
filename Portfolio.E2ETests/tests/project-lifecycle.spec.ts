import { expect, test, type Page } from '@playwright/test'
import { signInAsAdmin } from './helpers/authentication.js'

const publicOrigin = 'http://127.0.0.1:3101'

const waitForPublishedProjectsResponse = (page: Page) =>
  page.waitForResponse((response) => {
    const url = new URL(response.url())

    return url.pathname === '/graphql'
      && url.searchParams.get('operationName') === 'GetPublishedProjects'
      && response.ok()
  })

test('a published project with an image can be created, viewed, and deleted', async ({
  browser,
  page
}) => {
  const uniqueSuffix = crypto.randomUUID().slice(0, 8)
  const title = `E2E Project ${uniqueSuffix}`
  const summary = 'Created by the browser lifecycle test.'
  const imageFileName = `e2e-project-image-${uniqueSuffix}.png`
  const imageFile = Buffer.from(
    'iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACLSURBVHhe7dAxAQAgEIDAL2My438I3akAwy2MzLn7zIbBpgEMNg1gsGkAg00DGGwawGDTAAabBjDYNIDBpgEMNg1gsGkAg00DGGwawGDTAAabBjDYNIDBpgEMNg1gsGkAg00DGGwawGDTAAabBjDYNIDBpgEMNg1gsGkAg00DGGwawGDTAAabBjDYfDh7Ikrsk2V5AAAAAElFTkSuQmCC',
    'base64'
  )

  await signInAsAdmin(page)
  await page.goto('/projects/create')
  await page.getByLabel('Title').fill(title)
  await page.getByLabel('Summary').fill(summary)
  await page.getByLabel('Published').check()

  await page.getByRole('tab', { name: 'Images' }).click()

  const fileChooserPromise = page.waitForEvent('filechooser')
  await page.getByText(/Drag and drop images here/).click()
  const fileChooser = await fileChooserPromise

  await fileChooser.setFiles({
    name: imageFileName,
    mimeType: 'image/png',
    buffer: imageFile
  })

  await expect(
    page.getByText(`${imageFileName} (pending)`, { exact: true })
  ).toBeVisible()

  await page.getByRole('button', { name: 'Save', exact: true }).click()

  await expect(page).toHaveURL(/\/projects\/[0-9a-f-]+\/?$/)
  await expect(page.getByText('Project created.', { exact: true })).toBeVisible()

  const publicContext = await browser.newContext()
  const publicPage = await publicContext.newPage()
  let thumbnailUrl: string | null = null
  let fullImageUrl: string | null = null

  try {
    const initialProjectsResponse =
      waitForPublishedProjectsResponse(publicPage)

    await publicPage.goto(publicOrigin)
    await initialProjectsResponse

    await expect(publicPage.getByText(title, { exact: true })).toBeVisible()
    await expect(publicPage.getByText(summary, { exact: true })).toBeVisible()

    const thumbnail = publicPage.getByAltText(imageFileName, { exact: true })
    await expect(thumbnail).toBeVisible()
    await expect.poll(() => thumbnail.evaluate((image: HTMLImageElement) =>
      image.complete && image.naturalWidth > 0
    )).toBe(true)

    thumbnailUrl = await thumbnail.getAttribute('src')
    expect(thumbnailUrl).not.toBeNull()

    await publicPage.getByText(title, { exact: true }).click()

    const fullImage = publicPage.getByAltText(
      `Thumbnail of ${imageFileName}`,
      { exact: true }
    )
    await expect(fullImage).toBeVisible()
    await expect.poll(() => fullImage.evaluate((image: HTMLImageElement) =>
      image.complete && image.naturalWidth > 0
    )).toBe(true)

    fullImageUrl = await fullImage.getAttribute('src')
    expect(fullImageUrl).not.toBeNull()
  } finally {
    await page.getByRole('button', { name: 'Delete', exact: true }).click()
    const confirmationDialog = page.getByRole('dialog')
    await confirmationDialog
      .getByRole('button', { name: 'Delete', exact: true })
      .click()

    await expect(page).toHaveURL(url => url.pathname === '/projects')
    await expect(
      page.getByText('Project deleted successfully', { exact: true })
    ).toBeVisible()
  }

  const refreshedProjectsResponse =
    waitForPublishedProjectsResponse(publicPage)
  await publicPage.reload()
  await refreshedProjectsResponse

  await expect(publicPage.getByText(title, { exact: true })).toHaveCount(0)

  for (const imageUrl of [thumbnailUrl!, fullImageUrl!]) {
    await expect.poll(async () => {
      const cacheBustedUrl = new URL(imageUrl)
      cacheBustedUrl.searchParams.set('e2e', crypto.randomUUID())

      const response = await publicPage.request.get(cacheBustedUrl.toString())
      return response.status()
    }).toBe(404)
  }

  await publicContext.close()
})
