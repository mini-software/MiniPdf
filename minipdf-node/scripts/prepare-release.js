'use strict'

const fs = require('node:fs')
const path = require('node:path')

function prepareRelease(packageRoot = process.cwd()) {
  const packagePath = path.join(packageRoot, 'package.json')
  const licensePath = path.join(packageRoot, 'LICENSE')
  const platformRoot = path.join(packageRoot, 'npm')
  const packageJson = JSON.parse(fs.readFileSync(packagePath, 'utf8'))
  const platformDirectories = fs.readdirSync(platformRoot, { withFileTypes: true })
    .filter((entry) => entry.isDirectory())
    .map((entry) => entry.name)
    .sort()

  if (platformDirectories.length !== 8) {
    throw new Error(`Expected 8 platform packages, found ${platformDirectories.length}`)
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

  packageJson.optionalDependencies = optionalDependencies
  fs.writeFileSync(packagePath, `${JSON.stringify(packageJson, null, 2)}\n`)

  return optionalDependencies
}

if (require.main === module) {
  prepareRelease()
}

module.exports = { prepareRelease }