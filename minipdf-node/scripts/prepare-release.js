'use strict'

const fs = require('node:fs')
const path = require('node:path')

const SUPPORTED_NATIVE_PACKAGES = [
  'minipdf-darwin-arm64',
  'minipdf-darwin-x64',
  'minipdf-linux-arm64-gnu',
  'minipdf-linux-arm64-musl',
  'minipdf-linux-x64-gnu',
  'minipdf-linux-x64-musl',
  'minipdf-win32-arm64-msvc',
  'minipdf-win32-x64-msvc'
]

function prepareRelease(packageRoot = process.cwd()) {
  const packagePath = path.join(packageRoot, 'package.json')
  const licensePath = path.join(packageRoot, 'LICENSE')
  const platformRoot = path.join(packageRoot, 'npm')
  const packageJson = JSON.parse(fs.readFileSync(packagePath, 'utf8'))
  const platformDirectories = fs.readdirSync(platformRoot, { withFileTypes: true })
    .filter((entry) => entry.isDirectory())
    .map((entry) => entry.name)
    .sort()

  if (platformDirectories.length !== SUPPORTED_NATIVE_PACKAGES.length) {
    throw new Error(
      `Expected ${SUPPORTED_NATIVE_PACKAGES.length} platform packages, found ${platformDirectories.length}`
    )
  }

  const optionalDependencies = {}
  for (const directory of platformDirectories) {
    const platformDirectory = path.join(platformRoot, directory)
    const platformPackage = JSON.parse(
      fs.readFileSync(path.join(platformDirectory, 'package.json'), 'utf8')
    )

    if (platformPackage.version !== packageJson.version) {
      throw new Error(
        `${platformPackage.name} version ${platformPackage.version} does not match ${packageJson.version}`
      )
    }

    optionalDependencies[platformPackage.name] = platformPackage.version
    fs.copyFileSync(licensePath, path.join(platformDirectory, 'LICENSE'))
  }

  const nativePackageNames = Object.keys(optionalDependencies).sort()
  if (!SUPPORTED_NATIVE_PACKAGES.every((name, index) => name === nativePackageNames[index])) {
    throw new Error(`Unexpected platform package set: ${nativePackageNames.join(', ')}`)
  }

  packageJson.optionalDependencies = optionalDependencies
  fs.writeFileSync(packagePath, `${JSON.stringify(packageJson, null, 2)}\n`)

  return optionalDependencies
}

if (require.main === module) {
  prepareRelease()
}

module.exports = { prepareRelease }