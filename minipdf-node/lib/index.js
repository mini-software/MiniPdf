'use strict'

const binding = require('../index.js')

const PageSize = Object.freeze({
  A4: Object.freeze({ width: 595.28, height: 841.89 }),
  LETTER: Object.freeze({ width: 612, height: 792 })
})

module.exports = {
  ...binding,
  PageSize
}
