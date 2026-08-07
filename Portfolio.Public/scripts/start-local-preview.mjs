process.env.PORT ||= '3003'

const outputEntry = new URL('../.output/server/index.mjs', import.meta.url)

globalThis._importMeta_ = {
  url: outputEntry.href,
  env: process.env
}

await import(outputEntry.href)
