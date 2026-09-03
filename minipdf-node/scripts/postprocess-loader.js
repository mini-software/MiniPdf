'use strict'

const fs = require('node:fs')
const path = require('node:path')

const oldWindowsX64Require = "require('minipdf-win32-x64-msvc')"
const newWindowsX64Require = "require('@mini-software/minipdf-win32-x64-msvc')"
const oldMuslFunction = /function isMusl\(\) \{[\s\S]*?\n\}\n\nswitch \(platform\)/
const newMuslFunction = `function isMusl() {
  const { glibcVersionRuntime } = process.report.getReport().header
  return !glibcVersionRuntime
}

switch (platform)`

const oldLoadFailure = `if (!nativeBinding) {
  if (loadError) {
    throw loadError
  }
  throw new Error(\`Failed to load native binding\`)
}`
const newLoadFailure = `if (!nativeBinding) {
  const detail = loadError ? \`: \${loadError.message}\` : ''
  throw new Error(
    \`Failed to load MiniPdf native binding for \${platform}/\${arch}\${detail}. \` +
      'Reinstall minipdf with optional dependencies enabled or build it from source.',
    { cause: loadError || undefined }
  )
}`

function postprocessLoader(source) {
  let output = source.replaceAll('\r\n', '\n').replace(
    "const { existsSync, readFileSync } = require('fs')",
    "const { existsSync } = require('fs')"
  )
  output = output.replaceAll(oldWindowsX64Require, newWindowsX64Require)

  if (!output.includes(newMuslFunction)) {
    output = output.replace(oldMuslFunction, newMuslFunction)
  }
  if (!output.includes(newLoadFailure)) {
    output = output.replace(oldLoadFailure, newLoadFailure)
  }

  if (
    !output.includes(newWindowsX64Require) ||
    !output.includes(newMuslFunction) ||
    !output.includes(newLoadFailure)
  ) {
    throw new Error('Generated N-API loader did not match the expected structure')
  }

  return output
}

if (require.main === module) {
  const loaderPath = path.join(__dirname, '..', 'index.js')
  const source = fs.readFileSync(loaderPath, 'utf8')
  fs.writeFileSync(loaderPath, postprocessLoader(source))
}

module.exports = { postprocessLoader }