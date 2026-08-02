import { spawn } from 'node:child_process'
import { existsSync } from 'node:fs'
import { loadEnvFile } from 'node:process'
import { fileURLToPath } from 'node:url'
import path from 'node:path'

const containerName = 'portfolio-e2e-sql'
const sqlImage = 'mcr.microsoft.com/mssql/server:2022-CU26-ubuntu-22.04'
const sqlPassword = 'Portfolio_E2E!2026'
const sqlPort = 14333
const currentDirectory = path.dirname(fileURLToPath(import.meta.url))
const projectRoot = path.resolve(currentDirectory, '..')
const repositoryRoot = path.resolve(projectRoot, '..')
const localEnvironmentPath = path.join(
  repositoryRoot,
  '.env.test.local'
)

if (existsSync(localEnvironmentPath)) {
  loadEnvFile(localEnvironmentPath)
}

const playwrightCli = path.join(
  projectRoot,
  'node_modules',
  '@playwright',
  'test',
  'cli.js'
)

const run = (
  command,
  args,
  {
    env = process.env,
    rejectOnFailure = true,
    stdio = 'inherit'
  } = {}
) => new Promise((resolve, reject) => {
  const child = spawn(command, args, {
    cwd: projectRoot,
    env,
    stdio
  })

  child.on('error', reject)
  child.on('exit', (exitCode) => {
    if (exitCode === 0 || !rejectOnFailure) {
      resolve(exitCode ?? 1)
      return
    }

    reject(new Error(`${command} exited with code ${exitCode ?? 1}.`))
  })
})

const wait = milliseconds =>
  new Promise(resolve => setTimeout(resolve, milliseconds))

const waitForSqlServer = async () => {
  for (let attempt = 1; attempt <= 60; attempt += 1) {
    const exitCode = await run(
      'docker',
      [
        'exec',
        containerName,
        '/opt/mssql-tools18/bin/sqlcmd',
        '-S',
        'localhost',
        '-U',
        'sa',
        '-P',
        sqlPassword,
        '-C',
        '-Q',
        'SELECT 1'
      ],
      {
        rejectOnFailure: false,
        stdio: 'ignore'
      }
    )

    if (exitCode === 0) return
    await wait(1_000)
  }

  throw new Error('SQL Server did not become ready within 60 seconds.')
}

let containerStarted = false

try {
  await run('docker', [
    'run',
    '--detach',
    '--rm',
    '--name',
    containerName,
    '--env',
    'ACCEPT_EULA=Y',
    '--env',
    `MSSQL_SA_PASSWORD=${sqlPassword}`,
    '--publish',
    `${sqlPort}:1433`,
    sqlImage
  ])
  containerStarted = true
  await waitForSqlServer()

  const connectionString = [
    `Server=127.0.0.1,${sqlPort}`,
    'Database=PortfolioE2E',
    'User Id=sa',
    `Password=${sqlPassword}`,
    'TrustServerCertificate=True'
  ].join(';')

  try {
    await run(
      process.execPath,
      [playwrightCli, 'test', ...process.argv.slice(2)],
      {
        env: {
          ...process.env,
          E2E_SQL_CONNECTION_STRING: connectionString
        }
      }
    )
  } catch (error) {
    console.error(error instanceof Error ? error.message : error)
    process.exitCode = 1
  }
} finally {
  if (containerStarted) {
    await run(
      'docker',
      ['rm', '--force', containerName],
      { rejectOnFailure: false }
    )
  }
}
