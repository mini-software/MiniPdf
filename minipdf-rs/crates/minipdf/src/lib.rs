mod docx;
mod office;
mod pdf;
mod xlsx;

use std::fs;
use std::io::{Cursor, Read, Seek};
use std::path::Path;
use std::sync::{Mutex, OnceLock};

pub use office::OfficeFormat;
pub use pdf::{PdfColor, PdfDocument};
use zip::ZipArchive;

#[derive(Debug, thiserror::Error)]
pub enum MiniPdfError {
    #[error("I/O error: {0}")]
    Io(#[from] std::io::Error),
    #[error("ZIP package error: {0}")]
    Zip(#[from] zip::result::ZipError),
    #[error("XML parse error: {0}")]
    Xml(#[from] roxmltree::Error),
    #[error("unsupported or unknown Office document format")]
    UnsupportedFormat,
    #[error("invalid input: {0}")]
    InvalidInput(String),
}

pub type Result<T> = std::result::Result<T, MiniPdfError>;

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct PageSize {
    pub width: f32,
    pub height: f32,
}

impl PageSize {
    pub const A4: Self = Self {
        width: 595.28,
        height: 841.89,
    };
    pub const LETTER: Self = Self {
        width: 612.0,
        height: 792.0,
    };

    pub fn new(width: f32, height: f32) -> Result<Self> {
        if !width.is_finite() || !height.is_finite() || width <= 0.0 || height <= 0.0 {
            return Err(MiniPdfError::InvalidInput(
                "page width and height must be positive finite values".to_owned(),
            ));
        }
        Ok(Self { width, height })
    }
}

#[derive(Debug, Clone, Copy, Default)]
pub struct ConversionOptions {
    pub page_size: Option<PageSize>,
}

#[derive(Debug, Clone)]
pub struct RegisteredFont {
    pub name: String,
    pub data: Vec<u8>,
}

static REGISTERED_FONTS: OnceLock<Mutex<Vec<RegisteredFont>>> = OnceLock::new();

pub fn register_font(name: impl Into<String>, font_data: impl Into<Vec<u8>>) {
    let fonts = REGISTERED_FONTS.get_or_init(|| Mutex::new(Vec::new()));
    let mut fonts = fonts.lock().expect("font registry lock poisoned");
    fonts.push(RegisteredFont {
        name: name.into(),
        data: font_data.into(),
    });
}

pub fn registered_fonts() -> Vec<RegisteredFont> {
    let Some(fonts) = REGISTERED_FONTS.get() else {
        return Vec::new();
    };
    fonts.lock().expect("font registry lock poisoned").clone()
}

pub(crate) fn with_registered_fonts<T>(callback: impl FnOnce(&[RegisteredFont]) -> T) -> T {
    let Some(fonts) = REGISTERED_FONTS.get() else {
        return callback(&[]);
    };
    let fonts = fonts.lock().expect("font registry lock poisoned");
    callback(&fonts)
}

pub fn convert_to_pdf(input_path: impl AsRef<Path>, output_path: impl AsRef<Path>) -> Result<()> {
    convert_to_pdf_with_options(input_path, output_path, &ConversionOptions::default())
}

pub fn convert_to_pdf_with_options(
    input_path: impl AsRef<Path>,
    output_path: impl AsRef<Path>,
    options: &ConversionOptions,
) -> Result<()> {
    let pdf = convert_to_pdf_bytes_with_options(input_path, options)?;
    fs::write(output_path, pdf)?;
    Ok(())
}

pub fn convert_to_pdf_bytes(input_path: impl AsRef<Path>) -> Result<Vec<u8>> {
    convert_to_pdf_bytes_with_options(input_path, &ConversionOptions::default())
}

pub fn convert_to_pdf_bytes_with_options(
    input_path: impl AsRef<Path>,
    options: &ConversionOptions,
) -> Result<Vec<u8>> {
    let input_path = input_path.as_ref();
    let bytes = fs::read(input_path)?;
    match input_path
        .extension()
        .and_then(|ext| ext.to_str())
        .map(|ext| ext.to_ascii_lowercase())
    {
        Some(ext) if ext == "docx" => docx::convert_docx_bytes(&bytes, options),
        Some(ext) if ext == "xlsx" => xlsx::convert_xlsx_bytes(&bytes, options),
        _ => convert_bytes_to_pdf_with_options(&bytes, options),
    }
}

pub fn convert_bytes_to_pdf(input: &[u8]) -> Result<Vec<u8>> {
    convert_bytes_to_pdf_with_options(input, &ConversionOptions::default())
}

pub fn convert_bytes_to_pdf_with_options(
    input: &[u8],
    options: &ConversionOptions,
) -> Result<Vec<u8>> {
    match detect_office_format(input)? {
        OfficeFormat::Docx => docx::convert_docx_bytes(input, options),
        OfficeFormat::Xlsx => xlsx::convert_xlsx_bytes(input, options),
        OfficeFormat::Unknown => Err(MiniPdfError::UnsupportedFormat),
    }
}

pub fn detect_office_format(input: &[u8]) -> Result<OfficeFormat> {
    let cursor = Cursor::new(input);
    office::detect_office_format(cursor)
}

fn read_zip_text<R: Read + Seek>(
    archive: &mut ZipArchive<R>,
    path: &str,
) -> Result<Option<String>> {
    let Ok(mut file) = archive.by_name(path) else {
        return Ok(None);
    };
    let mut text = String::new();
    file.read_to_string(&mut text)?;
    Ok(Some(text))
}

fn text_width(text: &str, font_size: f32) -> f32 {
    text.chars()
        .map(|ch| if ch.is_ascii() { 0.5 } else { 0.9 })
        .sum::<f32>()
        * font_size
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn writes_basic_pdf_document() {
        let mut doc = PdfDocument::new();
        let page = doc.add_page(595.28, 841.89);
        page.add_text(
            "Hello from Rust MiniPdf",
            72.0,
            760.0,
            14.0,
            PdfColor::BLACK,
            false,
        );

        let pdf = doc.to_bytes();
        assert!(pdf.starts_with(b"%PDF-1.4"));
        assert!(pdf.ends_with(b"%%EOF\n"));
    }

    #[test]
    fn declares_pdf_stream_lengths_exactly() {
        let mut doc = PdfDocument::new();
        let page = doc.add_page(595.28, 841.89);
        page.add_text("Hello", 72.0, 760.0, 14.0, PdfColor::BLACK, false);

        let pdf = doc.to_bytes();
        let mut offset = 0;
        let mut checked_streams = 0;

        while let Some(relative_length_pos) = find_bytes(&pdf[offset..], b"/Length ") {
            let length_start = offset + relative_length_pos + b"/Length ".len();
            let length_end = length_start
                + pdf[length_start..]
                    .iter()
                    .position(|byte| !byte.is_ascii_digit())
                    .expect("stream length is terminated");
            let declared_length = std::str::from_utf8(&pdf[length_start..length_end])
                .expect("stream length is ASCII")
                .parse::<usize>()
                .expect("stream length is numeric");

            let stream_marker_pos = length_end
                + find_bytes(&pdf[length_end..], b"stream\n")
                    .expect("stream marker follows length");
            let stream_start = stream_marker_pos + b"stream\n".len();
            let stream_end = stream_start
                + find_bytes(&pdf[stream_start..], b"\nendstream")
                    .expect("endstream marker follows stream data");

            assert_eq!(declared_length, stream_end - stream_start);
            checked_streams += 1;
            offset = stream_end + b"\nendstream".len();
        }

        assert!(checked_streams > 0);
    }

    fn find_bytes(haystack: &[u8], needle: &[u8]) -> Option<usize> {
        haystack
            .windows(needle.len())
            .position(|window| window == needle)
    }
}
