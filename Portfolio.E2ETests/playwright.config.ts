import { defineConfig, devices } from '@playwright/test'
import { fileURLToPath } from 'node:url'

const repositoryRoot = fileURLToPath(new URL('../', import.meta.url))
const adminRoot = fileURLToPath(new URL('../Portfolio.Admin/', import.meta.url))
const publicRoot = fileURLToPath(new URL('../Portfolio.Public/', import.meta.url))
const apiOrigin = 'http://127.0.0.1:5218'
const adminOrigin = 'http://127.0.0.1:3100'
const publicOrigin = 'http://127.0.0.1:3101'
const nodeCommand = `"${process.execPath}"`
const connectionString = process.env.E2E_SQL_CONNECTION_STRING

if (!connectionString) {
  throw new Error(
    'E2E_SQL_CONNECTION_STRING is missing. Run the suite through npm test.'
  )
}

export default defineConfig({
  testDir: './tests',
  fullyParallel: false,
  workers: 1,
  timeout: 90_000,
  expect: {
    timeout: 10_000
  },
  use: {
    baseURL: adminOrigin,
    screenshot: 'only-on-failure',
    trace: 'retain-on-failure'
  },
  projects: [
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome']
      }
    }
  ],
  webServer: [
    {
      name: 'API',
      command:
        'dotnet run --project Portfolio.Api/Portfolio.Api.csproj '
        + '--configuration Release --no-launch-profile',
      cwd: repositoryRoot,
      env: {
        ...process.env,
        ASPNETCORE_ENVIRONMENT: 'EndToEnd',
        ASPNETCORE_URLS: apiOrigin,
        ConnectionStrings__DefaultConnection: connectionString,
        R2__AccessKey: 'e2e-access-key',
        R2__SecretKey: 'e2e-secret-key',
        R2__Endpoint: 'http://127.0.0.1:9',
        R2__Bucket: 'e2e',
        R2__PublicBaseUrl: 'http://storage.test'
      },
      url: `${apiOrigin}/api/health`,
      timeout: 120_000,
      reuseExistingServer: false,
      stdout: 'pipe',
      stderr: 'pipe'
    },
    {
      name: 'Admin',
      command:
        `${nodeCommand} ./node_modules/nuxt/bin/nuxt.mjs dev `
        + '--host 127.0.0.1 --port 3100',
      cwd: adminRoot,
      env: {
        ...process.env,
        NUXT_API_ORIGIN: apiOrigin,
        NUXT_BUILD_DIR: '.cache/nuxt-e2e',
        NUXT_END_TO_END: 'true'
      },
      url: `${adminOrigin}/api/health`,
      timeout: 120_000,
      reuseExistingServer: false,
      stdout: 'pipe',
      stderr: 'pipe'
    },
    {
      name: 'Public',
      command:
        `${nodeCommand} ./node_modules/nuxt/bin/nuxt.mjs dev `
        + '--host 127.0.0.1 --port 3101',
      cwd: publicRoot,
      env: {
        ...process.env,
        NUXT_API_ORIGIN: apiOrigin,
        NUXT_BUILD_DIR: '.cache/nuxt-e2e',
        NUXT_END_TO_END: 'true'
      },
      url: publicOrigin,
      timeout: 120_000,
      reuseExistingServer: false,
      stdout: 'pipe',
      stderr: 'pipe'
    }
  ]
})
