use std::io::Cursor;

use zip::ZipArchive;

use crate::pdf::{PdfColor, PdfDocument};
use crate::{read_zip_text, Result};

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
                current = String::new();
                append_wrapped_word(&mut lines, &mut current, word, max_width, font_size);
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

fn append_wrapped_word(
    lines: &mut Vec<String>,
    current: &mut String,
    word: &str,
    max_width: f32,
    font_size: f32,
) {
    if crate::text_width(word, font_size) <= max_width {
        current.push_str(word);
        return;
    }

    for chunk in split_word_to_width(word, max_width, font_size) {
        if !current.is_empty() {
            lines.push(std::mem::take(current));
        }
        current.push_str(&chunk);
    }
}

fn split_word_to_width(word: &str, max_width: f32, font_size: f32) -> Vec<String> {
    let mut chunks = Vec::new();
    let mut current = String::new();

    for ch in word.chars() {
        let mut candidate = current.clone();
        candidate.push(ch);
        if !current.is_empty() && crate::text_width(&candidate, font_size) > max_width {
            chunks.push(std::mem::take(&mut current));
        }
        current.push(ch);
    }

    if !current.is_empty() {
        chunks.push(current);
    }

    chunks
}

#[cfg(test)]
mod tests {
    use super::wrap_text;

    #[test]
    fn wraps_unspaced_text_without_truncating() {
        let text = "abcdefghijklmnopqrstuvwxyz";
        let lines = wrap_text(text, 40.0, 12.0);

        assert!(lines.len() > 1);
        assert_eq!(lines.concat(), text);
        assert!(lines
            .iter()
            .all(|line| crate::text_width(line, 12.0) <= 40.0));
    }
}
