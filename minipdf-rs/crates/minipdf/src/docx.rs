use std::io::Cursor;

use zip::ZipArchive;

use crate::pdf::{PdfColor, PdfDocument};
use crate::{read_zip_text, truncate_to_width, Result};

const PAGE_WIDTH: f32 = 595.28;
const PAGE_HEIGHT: f32 = 841.89;
const MARGIN: f32 = 54.0;
const BODY_FONT_SIZE: f32 = 11.0;
const LINE_HEIGHT: f32 = 16.0;

pub(crate) fn convert_docx_bytes(input: &[u8]) -> Result<Vec<u8>> {
    let paragraphs = read_docx_paragraphs(input)?;
    let mut doc = PdfDocument::new();
    render_docx(&mut doc, &paragraphs);
    Ok(doc.to_bytes())
}

fn read_docx_paragraphs(input: &[u8]) -> Result<Vec<String>> {
    let cursor = Cursor::new(input);
    let mut archive = ZipArchive::new(cursor)?;
    let Some(document_xml) = read_zip_text(&mut archive, "word/document.xml")? else {
        return Ok(vec!["Empty DOCX document".to_owned()]);
    };

    let xml = roxmltree::Document::parse(&document_xml)?;
    let mut paragraphs = Vec::new();

    for paragraph in xml.descendants().filter(|node| node.has_tag_name("p")) {
        let mut text = String::new();
        for node in paragraph.descendants() {
            if node.has_tag_name("t") {
                if let Some(value) = node.text() {
                    text.push_str(value);
                }
            } else if node.has_tag_name("tab") {
                text.push('\t');
            } else if node.has_tag_name("br") {
                text.push('\n');
            }
        }

        if !text.trim().is_empty() {
            paragraphs.push(text);
        }
    }

    if paragraphs.is_empty() {
        paragraphs.push("Empty DOCX document".to_owned());
    }

    Ok(paragraphs)
}

fn render_docx(doc: &mut PdfDocument, paragraphs: &[String]) {
    let mut page_index = doc.pages().len();
    doc.add_page(PAGE_WIDTH, PAGE_HEIGHT);
    let mut y = PAGE_HEIGHT - MARGIN - BODY_FONT_SIZE;
    let max_width = PAGE_WIDTH - MARGIN * 2.0;

    for paragraph in paragraphs {
        let lines = wrap_text(paragraph, max_width, BODY_FONT_SIZE);
        for line in lines {
            if y < MARGIN {
                page_index = doc.pages().len();
                doc.add_page(PAGE_WIDTH, PAGE_HEIGHT);
                y = PAGE_HEIGHT - MARGIN - BODY_FONT_SIZE;
            }
            let page = doc.page_mut(page_index).expect("page index is valid");
            page.add_text(line, MARGIN, y, BODY_FONT_SIZE, PdfColor::BLACK, false);
            y -= LINE_HEIGHT;
        }
        y -= LINE_HEIGHT * 0.35;
    }
}

fn wrap_text(text: &str, max_width: f32, font_size: f32) -> Vec<String> {
    let text = text.replace('\t', "    ");
    let mut lines = Vec::new();

    for forced_line in text.lines() {
        let mut current = String::new();
        for word in forced_line.split_whitespace() {
            let candidate = if current.is_empty() {
                word.to_owned()
            } else {
                format!("{current} {word}")
            };

            if crate::text_width(&candidate, font_size) <= max_width {
                current = candidate;
            } else {
                if !current.is_empty() {
                    lines.push(current);
                }
                current = truncate_to_width(word, max_width, font_size);
            }
        }

        if current.is_empty() {
            lines.push(String::new());
        } else {
            lines.push(current);
        }
    }

    lines
}