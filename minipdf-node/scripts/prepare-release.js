'use strict'

const fs = require('node:fs')
const path = require('node:path')
const { postprocessLoader } = require('./postprocess-loader.js')

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

const PUBLISHED_PACKAGE_NAMES = {
  'minipdf-win32-x64-msvc': '@mini-software/minipdf-win32-x64-msvc'
}

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

    const generatedName = platformPackage.name
    platformPackage.name = PUBLISHED_PACKAGE_NAMES[generatedName] || generatedName
    fs.writeFileSync(
      path.join(platformDirectory, 'package.json'),
      `${JSON.stringify(platformPackage, null, 2)}\n`
    )

    const readmePath = path.join(platformDirectory, 'README.md')
    if (platformPackage.name !== generatedName && fs.existsSync(readmePath)) {
      const readme = fs.readFileSync(readmePath, 'utf8')
      fs.writeFileSync(readmePath, readme.replaceAll(generatedName, platformPackage.name))
    }

    optionalDependencies[platformPackage.name] = platformPackage.version
    fs.copyFileSync(licensePath, path.join(platformDirectory, 'LICENSE'))
  }

  const nativePackageNames = Object.keys(optionalDependencies).sort()
  const publishedPackageNames = SUPPORTED_NATIVE_PACKAGES
    .map((name) => PUBLISHED_PACKAGE_NAMES[name] || name)
    .sort()
  if (!publishedPackageNames.every((name, index) => name === nativePackageNames[index])) {
    throw new Error(`Unexpected platform package set: ${nativePackageNames.join(', ')}`)
  }

  packageJson.optionalDependencies = optionalDependencies
  fs.writeFileSync(packagePath, `${JSON.stringify(packageJson, null, 2)}\n`)

  const loaderPath = path.join(packageRoot, 'index.js')
  if (fs.existsSync(loaderPath)) {
    const loader = fs.readFileSync(loaderPath, 'utf8')
    fs.writeFileSync(loaderPath, postprocessLoader(loader))
  }

  return optionalDependencies
}

if (require.main === module) {
  prepareRelease()
}

module.exports = { prepareRelease }