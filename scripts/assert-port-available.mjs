import { createServer } from 'node:net'

const [portArgument, applicationName = 'Development server'] = process.argv.slice(2)
const port = Number(portArgument)

if (!Number.isInteger(port) || port < 1 || port > 65_535) {
  throw new Error(`Invalid development port: "${portArgument ?? ''}".`)
}

const server = createServer()

try {
  await new Promise((resolve, reject) => {
    server.once('error', reject)
    server.listen({ host: 'localhost', port }, resolve)
  })
} catch (error) {
  if (error instanceof Error && 'code' in error && error.code === 'EADDRINUSE') {
    console.error(
      `${applicationName} requires http://localhost:${port}, but that port is already in use.`
    )
    process.exitCode = 1
  } else {
    throw error
  }
} finally {
  if (server.listening) {
    await new Promise((resolve, reject) => {
      server.close(error => error ? reject(error) : resolve())
    })
  }
}
