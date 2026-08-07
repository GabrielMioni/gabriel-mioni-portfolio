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

const requireEnvironmentVariable = (name: string) => {
  const value = process.env[name]

  if (!value) {
    throw new Error(
      `${name} is missing. Configure .env.test.local or GitHub Actions.`
    )
  }

  return value
}

if (!connectionString) {
  throw new Error(
    'E2E_SQL_CONNECTION_STRING is missing. Run the suite through npm test.'
  )
}

const r2AccessKey = requireEnvironmentVariable('R2_TEST_ACCESS_KEY')
const r2SecretKey = requireEnvironmentVariable('R2_TEST_SECRET_KEY')
const r2Endpoint = requireEnvironmentVariable('R2_TEST_ENDPOINT')
const r2Bucket = requireEnvironmentVariable('R2_TEST_BUCKET')
const r2PublicBaseUrl = requireEnvironmentVariable(
  'R2_TEST_PUBLIC_BASE_URL'
)

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
        ClientApplications__AdminOrigin: adminOrigin,
        ClientApplications__PublicOrigin: publicOrigin,
        ConnectionStrings__DefaultConnection: connectionString,
        R2__AccessKey: r2AccessKey,
        R2__SecretKey: r2SecretKey,
        R2__Endpoint: r2Endpoint,
        R2__Bucket: r2Bucket,
        R2__PublicBaseUrl: r2PublicBaseUrl
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
        NUXT_END_TO_END: 'true',
        NUXT_PUBLIC_STORAGE_BASE: r2PublicBaseUrl
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
        NUXT_END_TO_END: 'true',
        NUXT_PUBLIC_STORAGE_BASE: r2PublicBaseUrl
      },
      url: publicOrigin,
      timeout: 120_000,
      reuseExistingServer: false,
      stdout: 'pipe',
      stderr: 'pipe'
    }
  ]
})
