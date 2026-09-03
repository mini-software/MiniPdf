use std::panic::{catch_unwind, AssertUnwindSafe};

use minipdf::{ConversionOptions as CoreConversionOptions, PageSize as CorePageSize};
use napi::bindgen_prelude::{Buffer, Error, Result, Status};
use napi_derive::napi;

#[napi(object)]
pub struct PageSize {
    pub width: f64,
    pub height: f64,
}

#[napi(object)]
pub struct ConversionOptions {
    pub page_size: Option<PageSize>,
}

#[napi(object)]
pub struct RegisteredFont {
    pub name: String,
    pub data: Buffer,
}

#[napi(js_name = "convertToPdf")]
pub fn convert_to_pdf(
    input_path: String,
    output_path: String,
    options: Option<ConversionOptions>,
) -> Result<()> {
    let options = to_core_options(options)?;
    minipdf::convert_to_pdf_with_options(input_path, output_path, &options).map_err(to_napi_error)
}

#[napi(js_name = "convertToPdfBytes")]
pub fn convert_to_pdf_bytes(
    input_path: String,
    options: Option<ConversionOptions>,
) -> Result<Buffer> {
    let options = to_core_options(options)?;
    minipdf::convert_to_pdf_bytes_with_options(input_path, &options)
        .map(Buffer::from)
        .map_err(to_napi_error)
}

#[napi(js_name = "convertBytesToPdf")]
pub fn convert_bytes_to_pdf(input: Buffer, options: Option<ConversionOptions>) -> Result<Buffer> {
    let options = to_core_options(options)?;
    minipdf::convert_bytes_to_pdf_with_options(input.as_ref(), &options)
        .map(Buffer::from)
        .map_err(to_napi_error)
}

#[napi(js_name = "detectOfficeFormat")]
pub fn detect_office_format(input: Buffer) -> Result<String> {
    minipdf::detect_office_format(input.as_ref())
        .map(|format| match format {
            minipdf::OfficeFormat::Unknown => "unknown",
            minipdf::OfficeFormat::Xlsx => "xlsx",
            minipdf::OfficeFormat::Docx => "docx",
            minipdf::OfficeFormat::Pptx => "pptx",
        })
        .map(str::to_owned)
        .map_err(to_napi_error)
}

#[napi(js_name = "registerFont")]
pub fn register_font(name: String, font_data: Buffer) -> Result<()> {
    catch_core_panic(|| minipdf::register_font(name, font_data.to_vec()))
}

#[napi(js_name = "registeredFonts")]
pub fn registered_fonts() -> Result<Vec<RegisteredFont>> {
    catch_core_panic(minipdf::registered_fonts).map(|fonts| {
        fonts
            .into_iter()
            .map(|font| RegisteredFont {
                name: font.name,
                data: Buffer::from(font.data),
            })
            .collect()
    })
}

fn to_core_options(options: Option<ConversionOptions>) -> Result<CoreConversionOptions> {
    let page_size = options
        .and_then(|options| options.page_size)
        .map(|page_size| CorePageSize::new(page_size.width as f32, page_size.height as f32))
        .transpose()
        .map_err(to_napi_error)?;

    Ok(CoreConversionOptions { page_size })
}

fn to_napi_error(error: minipdf::MiniPdfError) -> Error {
    let status = match &error {
        minipdf::MiniPdfError::InvalidInput(_) | minipdf::MiniPdfError::UnsupportedFormat => {
            Status::InvalidArg
        }
        _ => Status::GenericFailure,
    };
    Error::new(status, error.to_string())
}

fn catch_core_panic<T>(operation: impl FnOnce() -> T) -> Result<T> {
    catch_unwind(AssertUnwindSafe(operation)).map_err(|_| {
        Error::new(
            Status::GenericFailure,
            "MiniPdf core operation panicked".to_owned(),
        )
    })
}
