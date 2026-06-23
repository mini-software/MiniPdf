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

pub fn convert_to_pdf(input_path: impl AsRef<Path>, output_path: impl AsRef<Path>) -> Result<()> {
    let pdf = convert_to_pdf_bytes(input_path)?;
    fs::write(output_path, pdf)?;
    Ok(())
}

pub fn convert_to_pdf_bytes(input_path: impl AsRef<Path>) -> Result<Vec<u8>> {
    let input_path = input_path.as_ref();
    let bytes = fs::read(input_path)?;
    match input_path.extension().and_then(|ext| ext.to_str()).map(|ext| ext.to_ascii_lowercase()) {
        Some(ext) if ext == "docx" => docx::convert_docx_bytes(&bytes),
        Some(ext) if ext == "xlsx" => xlsx::convert_xlsx_bytes(&bytes),
        _ => convert_bytes_to_pdf(&bytes),
    }
}

pub fn convert_bytes_to_pdf(input: &[u8]) -> Result<Vec<u8>> {
    match detect_office_format(input)? {
        OfficeFormat::Docx => docx::convert_docx_bytes(input),
        OfficeFormat::Xlsx => xlsx::convert_xlsx_bytes(input),
        OfficeFormat::Unknown => Err(MiniPdfError::UnsupportedFormat),
    }
}

pub fn detect_office_format(input: &[u8]) -> Result<OfficeFormat> {
    let cursor = Cursor::new(input);
    office::detect_office_format(cursor)
}

fn read_zip_text<R: Read + Seek>(archive: &mut ZipArchive<R>, path: &str) -> Result<Option<String>> {
    let Ok(mut file) = archive.by_name(path) else {
        return Ok(None);
    };
    let mut text = String::new();
    file.read_to_string(&mut text)?;
    Ok(Some(text))
}

fn text_width(text: &str, font_size: f32) -> f32 {
    text.chars().map(|ch| if ch.is_ascii() { 0.5 } else { 0.9 }).sum::<f32>() * font_size
}

fn truncate_to_width(text: &str, max_width: f32, font_size: f32) -> String {
    if text_width(text, font_size) <= max_width {
        return text.to_owned();
    }

    let ellipsis = "...";
    let mut result = String::new();
    for ch in text.chars() {
        let candidate = format!("{result}{ch}{ellipsis}");
        if text_width(&candidate, font_size) > max_width {
            break;
        }
        result.push(ch);
    }
    result.push_str(ellipsis);
    result
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn writes_basic_pdf_document() {
        let mut doc = PdfDocument::new();
        let page = doc.add_page(595.28, 841.89);
        page.add_text("Hello from Rust MiniPdf", 72.0, 760.0, 14.0, PdfColor::BLACK, false);

        let pdf = doc.to_bytes();
        assert!(pdf.starts_with(b"%PDF-1.4"));
        assert!(pdf.ends_with(b"%%EOF\n"));
    }

    #[test]
    fn truncates_text_to_fit_width() {
        let text = truncate_to_width("abcdefghijklmnopqrstuvwxyz", 40.0, 12.0);
        assert!(text.ends_with("..."));
        assert!(text_width(&text, 12.0) <= 40.0);
    }
}