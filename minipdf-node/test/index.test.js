'use strict'

const assert = require('node:assert/strict')
const fs = require('node:fs')
const os = require('node:os')
const path = require('node:path')
const test = require('node:test')

const minipdf = require('../lib/index.js')
const packageJson = require('../package.json')

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
    /page width and height must be positive finite values/
  )
})

test('keeps native package versions aligned with the main package', () => {
  const nativeVersions = Object.values(packageJson.optionalDependencies)

  assert.equal(packageJson.private, undefined)
  assert.equal(nativeVersions.length, 8)
  assert.deepEqual([...new Set(nativeVersions)], [packageJson.version])
})
