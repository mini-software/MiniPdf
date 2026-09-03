'use strict'

const assert = require('node:assert/strict')
const fs = require('node:fs')
const os = require('node:os')
const path = require('node:path')
const test = require('node:test')

const minipdf = require('../lib/index.js')
const packageJson = require('../package.json')
const { postprocessLoader } = require('../scripts/postprocess-loader.js')
const { prepareRelease } = require('../scripts/prepare-release.js')

const fixturePath = path.join(
  __dirname,
  '..',
  '..',
  'tests',
  'Issue_Files',
  'docx',
  'Invoice.docx'
)

test('detects Office format from a Buffer', () => {
  const input = fs.readFileSync(fixturePath)
  assert.equal(minipdf.detectOfficeFormat(input), 'docx')
})

test('converts path and Buffer inputs to PDF', () => {
  const input = fs.readFileSync(fixturePath)
  const fromBuffer = minipdf.convertBytesToPdf(input)
  const fromPath = minipdf.convertToPdfBytes(fixturePath, {
    pageSize: minipdf.PageSize.A4
  })

  assert.equal(fromBuffer.subarray(0, 8).toString(), '%PDF-1.4')
  assert.equal(fromPath.subarray(0, 8).toString(), '%PDF-1.4')
})

test('writes converted PDF to an output path', (context) => {
  const outputDirectory = fs.mkdtempSync(path.join(os.tmpdir(), 'minipdf-node-'))
  context.after(() => fs.rmSync(outputDirectory, { recursive: true, force: true }))

  const outputPath = path.join(outputDirectory, 'invoice.pdf')
  minipdf.convertToPdf(fixturePath, outputPath)

  assert.equal(fs.readFileSync(outputPath).subarray(0, 8).toString(), '%PDF-1.4')
})

test('rejects invalid custom page dimensions', () => {
  assert.throws(
    () => minipdf.convertToPdfBytes(fixturePath, {
      pageSize: { width: 0, height: 841.89 }
    }),
    (error) => {
      assert.equal(error.code, 'InvalidArg')
      assert.match(error.message, /page width and height must be positive finite values/)
      return true
    }
  )
})

test('is configured for public publication', () => {
  assert.equal(packageJson.private, undefined)
  assert.equal(packageJson.publishConfig.access, 'public')
  assert.equal(packageJson.optionalDependencies, undefined)
})

test('registers fonts through the protected native boundary', () => {
  minipdf.registerFont('TestFont', Buffer.from([0, 1, 2, 3]))

  assert.ok(minipdf.registeredFonts().some((font) => font.name === 'TestFont'))
})

test('keeps native loader diagnostics compatible with Node 18', () => {
  const loaderPath = path.join(__dirname, '..', 'index.js')
  const loader = fs.readFileSync(loaderPath, 'utf8')

  assert.doesNotMatch(loader, /which ldd|readFileSync/)
  assert.match(loader, /Failed to load MiniPdf native binding for \$\{platform\}\/\$\{arch\}/)
  assert.equal(postprocessLoader(loader), loader)
})

test('prepares aligned platform packages for release', (context) => {
  const packageRoot = fs.mkdtempSync(path.join(os.tmpdir(), 'minipdf-release-'))
  context.after(() => fs.rmSync(packageRoot, { recursive: true, force: true }))

  fs.writeFileSync(
    path.join(packageRoot, 'package.json'),
    JSON.stringify({ name: 'minipdf', version: '0.1.0' })
  )
  fs.writeFileSync(path.join(packageRoot, 'LICENSE'), 'license text')

  for (let index = 0; index < 8; index += 1) {
    const platformDirectory = path.join(packageRoot, 'npm', `platform-${index}`)
    fs.mkdirSync(platformDirectory, { recursive: true })
    fs.writeFileSync(
      path.join(platformDirectory, 'package.json'),
      JSON.stringify({ name: `minipdf-platform-${index}`, version: '0.1.0' })
    )
  }

  const dependencies = prepareRelease(packageRoot)
  const preparedPackage = JSON.parse(
    fs.readFileSync(path.join(packageRoot, 'package.json'), 'utf8')
  )

  assert.equal(Object.keys(dependencies).length, 8)
  assert.deepEqual(preparedPackage.optionalDependencies, dependencies)
  assert.equal(
    fs.readFileSync(path.join(packageRoot, 'npm', 'platform-0', 'LICENSE'), 'utf8'),
    'license text'
  )
})
