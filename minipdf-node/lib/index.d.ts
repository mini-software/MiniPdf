/// <reference types="node" />

export type OfficeFormat = 'unknown' | 'xlsx' | 'docx' | 'pptx'

export interface PageSizeValue {
  readonly width: number
  readonly height: number
}

export declare const PageSize: {
  readonly A4: Readonly<PageSizeValue>
  readonly LETTER: Readonly<PageSizeValue>
}

export interface ConversionOptions {
  pageSize?: PageSizeValue
}

export interface RegisteredFont {
  name: string
  data: Buffer
}

export declare function convertToPdf(
  inputPath: string,
  outputPath: string,
  options?: ConversionOptions
): void

export declare function convertToPdfBytes(
  inputPath: string,
  options?: ConversionOptions
): Buffer

export declare function convertBytesToPdf(
  input: Buffer,
  options?: ConversionOptions
): Buffer

export declare function detectOfficeFormat(input: Buffer): OfficeFormat

export declare function registerFont(name: string, fontData: Buffer): void

export declare function registeredFonts(): RegisteredFont[]
