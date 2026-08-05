import {
  expect,
  test,
  type BrowserContext,
  type Page
} from '@playwright/test'
import { signInAsAdmin } from './helpers/authentication.js'

const publicOrigin = 'http://127.0.0.1:3101'

const waitForPublishedProjectsResponse = (page: Page) =>
  page.waitForResponse((response) => {
    const url = new URL(response.url())

    return url.pathname === '/graphql'
      && url.searchParams.get('operationName') === 'GetPublishedProjects'
      && response.ok()
  })

test('a published project with an image and link can be created, edited, viewed, and deleted', async ({
  browser,
  page
}) => {
  const uniqueSuffix = crypto.randomUUID().slice(0, 8)
  const title = `E2E Project ${uniqueSuffix}`
  const summary = 'Created by the browser lifecycle test.'
  const imageFileName = `e2e-project-image-${uniqueSuffix}.png`
  const linkText = `E2E Repository ${uniqueSuffix}`
  const linkUrl = `https://example.com/repositories/${uniqueSuffix}`
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

  await page.getByRole('tab', { name: 'Links: 0 active', exact: true }).click()
  await page.getByRole('button', { name: 'Add link', exact: true }).click()
  await page.getByLabel('Url').fill(linkUrl)
  await page.getByLabel('Link Text').fill(linkText)

  await page.getByRole('button', { name: 'Save', exact: true }).click()

  await expect(page).toHaveURL(/\/projects\/[0-9a-f-]+\/?$/)
  await expect(page.getByText('Project created.', { exact: true })).toBeVisible()

  let publicContext: BrowserContext | undefined
  let publicPage: Page | undefined
  let thumbnailUrl: string | null = null
  let fullImageUrl: string | null = null

  try {
    await page.reload()

    const saveButton = page.getByRole('button', {
      name: 'Save',
      exact: true
    })

    await page.getByRole('tab', {
      name: 'Images: 1 active',
      exact: true
    }).click()

    const altTextInput = page.getByLabel('Alt Text')
    const imageRow = page.locator('.editor-list-item-layout').filter({
      has: altTextInput
    })
    await expect(altTextInput).toHaveValue(imageFileName)
    await imageRow.getByRole('button', {
      name: 'Remove',
      exact: true
    }).click()

    await expect(page.getByRole('tab', {
      name: 'Images: 0 active, 1 pending removal',
      exact: true
    })).toBeVisible()
    await expect(altTextInput).toBeDisabled()
    await expect(imageRow.getByText(
      'Will be removed',
      { exact: true }
    )).toBeVisible()
    await expect(saveButton).toBeEnabled()

    await imageRow.getByRole('button', {
      name: 'Undo',
      exact: true
    }).click()

    await expect(page.getByRole('tab', {
      name: 'Images: 1 active',
      exact: true
    })).toBeVisible()
    await expect(altTextInput).toBeEnabled()
    await expect(saveButton).toBeDisabled()

    await page.getByRole('tab', {
      name: 'Links: 1 active',
      exact: true
    }).click()

    const linkUrlInput = page.getByLabel('Url')
    const linkTextInput = page.getByLabel('Link Text')
    const linkRow = page.locator('.editor-list-item-layout').filter({
      has: linkUrlInput
    })
    const linkTypeInput = linkRow.locator('input[role="combobox"]')

    await expect(linkUrlInput).toHaveValue(linkUrl)
    await expect(linkTextInput).toHaveValue(linkText)
    await linkRow.getByRole('button', {
      name: 'Remove',
      exact: true
    }).click()

    await expect(page.getByRole('tab', {
      name: 'Links: 0 active, 1 pending removal',
      exact: true
    })).toBeVisible()
    await expect(linkUrlInput).toBeDisabled()
    await expect(linkTextInput).toBeDisabled()
    await expect(linkTypeInput).toBeDisabled()
    await expect(linkRow.getByText(
      'Will be removed',
      { exact: true }
    )).toBeVisible()
    await expect(saveButton).toBeEnabled()

    await linkRow.getByRole('button', {
      name: 'Undo',
      exact: true
    }).click()

    await expect(page.getByRole('tab', {
      name: 'Links: 1 active',
      exact: true
    })).toBeVisible()
    await expect(linkUrlInput).toBeEnabled()
    await expect(linkTextInput).toBeEnabled()
    await expect(linkTypeInput).toBeEnabled()
    await expect(saveButton).toBeDisabled()

    await page.getByRole('button', {
      name: 'Add link',
      exact: true
    }).click()

    await expect(page.getByRole('tab', {
      name: 'Links: 2 active, 1 pending addition',
      exact: true
    })).toBeVisible()

    const linkRows = page.locator(
      '.project-links-list-form .editor-list-item-layout'
    )
    await expect(linkRows).toHaveCount(2)
    await linkRows.nth(1).getByRole('button', {
      name: 'Remove',
      exact: true
    }).click()

    await expect(page.getByRole('tab', {
      name: 'Links: 1 active',
      exact: true
    })).toBeVisible()
    await expect(page.getByLabel('Url')).toHaveCount(1)
    await expect(saveButton).toBeDisabled()

    publicContext = await browser.newContext()
    publicPage = await publicContext.newPage()

    const initialProjectsResponse =
      waitForPublishedProjectsResponse(publicPage)

    await publicPage.goto(publicOrigin)
    await initialProjectsResponse

    await expect(publicPage.getByText(title, { exact: true })).toBeVisible()
    await expect(publicPage.getByText(summary, { exact: true })).toBeVisible()
    await expect(publicPage.getByRole('link', {
      name: linkText,
      exact: true
    })).toBeVisible()

    const thumbnail = publicPage.getByAltText(imageFileName, { exact: true })
    await expect(thumbnail).toBeVisible()
    await expect.poll(() => thumbnail.evaluate((image: HTMLImageElement) =>
      image.complete && image.naturalWidth > 0
    )).toBe(true)

    thumbnailUrl = await thumbnail.getAttribute('src')
    expect(thumbnailUrl).not.toBeNull()

    await publicPage.getByRole('button', {
      name: title,
      exact: true
    }).click()

    const fullImage = publicPage.getByRole('button', {
      name: `Enlarge ${imageFileName}`,
      exact: true
    }).getByAltText(
      `Full image of ${imageFileName}`,
      { exact: true }
    )
    await expect(fullImage).toBeVisible()
    await expect.poll(() => fullImage.evaluate((image: HTMLImageElement) =>
      image.complete && image.naturalWidth > 0
    )).toBe(true)

    fullImageUrl = await fullImage.getAttribute('src')
    expect(fullImageUrl).not.toBeNull()

    await linkRow.getByRole('button', {
      name: 'Remove',
      exact: true
    }).click()

    await expect(page.getByRole('tab', {
      name: 'Links: 0 active, 1 pending removal',
      exact: true
    })).toBeVisible()
    await expect(saveButton).toBeEnabled()

    await saveButton.click()
    await expect(page.getByText('Project saved.', { exact: true })).toBeVisible()

    await page.reload()
    await page.getByRole('tab', {
      name: 'Links: 0 active',
      exact: true
    }).click()
    await expect(page.getByLabel('Url')).toHaveCount(0)

    const projectWithoutLinkResponse =
      waitForPublishedProjectsResponse(publicPage)
    await publicPage.reload()
    await projectWithoutLinkResponse

    await expect(publicPage.getByText(title, { exact: true })).toBeVisible()
    await expect(publicPage.getByRole('link', {
      name: linkText,
      exact: true
    })).toHaveCount(0)
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

  if (!publicContext || !publicPage) {
    throw new Error('The public browser context was not created.')
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
