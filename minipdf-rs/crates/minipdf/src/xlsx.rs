use std::collections::HashMap;
use std::io::Cursor;

use zip::ZipArchive;

use crate::pdf::{PdfColor, PdfDocument};
use crate::{read_zip_text, truncate_to_width, Result};

const PAGE_WIDTH: f32 = 841.89;
const PAGE_HEIGHT: f32 = 595.28;
const MARGIN: f32 = 36.0;
const TITLE_FONT_SIZE: f32 = 14.0;
const CELL_FONT_SIZE: f32 = 8.0;
const ROW_HEIGHT: f32 = 19.0;
const COL_WIDTH: f32 = 92.0;

#[derive(Debug, Clone)]
struct SheetData {
    name: String,
    rows: Vec<Vec<String>>,
}

pub(crate) fn convert_xlsx_bytes(input: &[u8]) -> Result<Vec<u8>> {
    let sheets = read_xlsx_sheets(input)?;
    let mut doc = PdfDocument::new();
    render_xlsx(&mut doc, &sheets);
    Ok(doc.to_bytes())
}

fn read_xlsx_sheets(input: &[u8]) -> Result<Vec<SheetData>> {
    let cursor = Cursor::new(input);
    let mut archive = ZipArchive::new(cursor)?;
    let shared_strings = read_shared_strings(&mut archive)?;
    let sheet_paths = read_sheet_paths(&mut archive)?;
    let mut sheets = Vec::new();

    for (name, path) in sheet_paths {
        let Some(sheet_xml) = read_zip_text(&mut archive, &path)? else {
            continue;
        };
        let rows = read_sheet_rows(&sheet_xml, &shared_strings)?;
        sheets.push(SheetData { name, rows });
    }

    if sheets.is_empty() {
        if let Some(sheet_xml) = read_zip_text(&mut archive, "xl/worksheets/sheet1.xml")? {
            sheets.push(SheetData {
                name: "Sheet1".to_owned(),
                rows: read_sheet_rows(&sheet_xml, &shared_strings)?,
            });
        }
    }

    if sheets.is_empty() {
        sheets.push(SheetData {
            name: "Workbook".to_owned(),
            rows: vec![vec!["Empty XLSX workbook".to_owned()]],
        });
    }

    Ok(sheets)
}

fn read_shared_strings<R: std::io::Read + std::io::Seek>(archive: &mut ZipArchive<R>) -> Result<Vec<String>> {
    let Some(shared_xml) = read_zip_text(archive, "xl/sharedStrings.xml")? else {
        return Ok(Vec::new());
    };
    let xml = roxmltree::Document::parse(&shared_xml)?;
    let strings = xml
        .descendants()
        .filter(|node| node.has_tag_name("si"))
        .map(|si| {
            si.descendants()
                .filter(|node| node.has_tag_name("t"))
                .filter_map(|node| node.text())
                .collect::<String>()
        })
        .collect();
    Ok(strings)
}

fn read_sheet_paths<R: std::io::Read + std::io::Seek>(archive: &mut ZipArchive<R>) -> Result<Vec<(String, String)>> {
    let rels = read_workbook_relationships(archive)?;
    let Some(workbook_xml) = read_zip_text(archive, "xl/workbook.xml")? else {
        return Ok(Vec::new());
    };
    let workbook = roxmltree::Document::parse(&workbook_xml)?;
    let mut result = Vec::new();

    for (index, sheet) in workbook
        .descendants()
        .filter(|node| node.has_tag_name("sheet"))
        .enumerate()
    {
        let name = sheet.attribute("name").unwrap_or("Sheet").to_owned();
        let rel_id = sheet
            .attributes()
            .find(|attr| attr.name() == "id")
            .map(|attr| attr.value().to_owned());
        let path = rel_id
            .and_then(|id| rels.get(&id).cloned())
            .unwrap_or_else(|| format!("xl/worksheets/sheet{}.xml", index + 1));
        result.push((name, normalize_xl_path(&path)));
    }

    Ok(result)
}

fn read_workbook_relationships<R: std::io::Read + std::io::Seek>(archive: &mut ZipArchive<R>) -> Result<HashMap<String, String>> {
    let Some(rels_xml) = read_zip_text(archive, "xl/_rels/workbook.xml.rels")? else {
        return Ok(HashMap::new());
    };
    let rels_doc = roxmltree::Document::parse(&rels_xml)?;
    let mut rels = HashMap::new();
    for rel in rels_doc.descendants().filter(|node| node.has_tag_name("Relationship")) {
        let Some(id) = rel.attribute("Id") else {
            continue;
        };
        let Some(target) = rel.attribute("Target") else {
            continue;
        };
        rels.insert(id.to_owned(), target.to_owned());
    }
    Ok(rels)
}

fn normalize_xl_path(path: &str) -> String {
    let path = path.replace('\\', "/");
    if let Some(stripped) = path.strip_prefix('/') {
        stripped.to_owned()
    } else if path.starts_with("xl/") {
        path
    } else {
        format!("xl/{path}")
    }
}

fn read_sheet_rows(sheet_xml: &str, shared_strings: &[String]) -> Result<Vec<Vec<String>>> {
    let xml = roxmltree::Document::parse(sheet_xml)?;
    let mut rows = Vec::new();

    for row in xml.descendants().filter(|node| node.has_tag_name("row")) {
        let mut cells: Vec<(usize, String)> = Vec::new();
        for cell in row.children().filter(|node| node.has_tag_name("c")) {
            let col = cell.attribute("r").and_then(column_index_from_ref).unwrap_or(cells.len());
            let value = read_cell_value(cell, shared_strings);
            cells.push((col, value));
        }

        let width = cells.iter().map(|(col, _)| *col).max().map(|col| col + 1).unwrap_or(0);
        let mut row_values = vec![String::new(); width];
        for (col, value) in cells {
            if let Some(slot) = row_values.get_mut(col) {
                *slot = value;
            }
        }
        if row_values.iter().any(|value| !value.is_empty()) {
            rows.push(row_values);
        }
    }

    Ok(rows)
}

fn read_cell_value(cell: roxmltree::Node<'_, '_>, shared_strings: &[String]) -> String {
    let cell_type = cell.attribute("t");
    if cell_type == Some("inlineStr") {
        return cell
            .descendants()
            .filter(|node| node.has_tag_name("t"))
            .filter_map(|node| node.text())
            .collect::<String>();
    }

    let value = cell
        .children()
        .find(|node| node.has_tag_name("v"))
        .and_then(|node| node.text())
        .unwrap_or("");

    if cell_type == Some("s") {
        return value
            .parse::<usize>()
            .ok()
            .and_then(|index| shared_strings.get(index).cloned())
            .unwrap_or_default();
    }

    value.to_owned()
}

fn column_index_from_ref(cell_ref: &str) -> Option<usize> {
    let mut col = 0usize;
    let mut seen_letter = false;
    for ch in cell_ref.chars().filter(|ch| ch.is_ascii_alphabetic()) {
        seen_letter = true;
        col = col * 26 + (ch.to_ascii_uppercase() as usize - 'A' as usize + 1);
    }
    seen_letter.then_some(col.saturating_sub(1))
}

fn render_xlsx(doc: &mut PdfDocument, sheets: &[SheetData]) {
    for sheet in sheets {
        render_sheet(doc, sheet);
    }
}

fn render_sheet(doc: &mut PdfDocument, sheet: &SheetData) {
    let mut page_index = doc.pages().len();
    doc.add_page(PAGE_WIDTH, PAGE_HEIGHT);
    let mut y = PAGE_HEIGHT - MARGIN - TITLE_FONT_SIZE;
    let max_cols = ((PAGE_WIDTH - MARGIN * 2.0) / COL_WIDTH).floor() as usize;

    let page = doc.page_mut(page_index).expect("page index is valid");
    page.add_text(&sheet.name, MARGIN, y, TITLE_FONT_SIZE, PdfColor::BLACK, true);
    y -= ROW_HEIGHT * 1.4;

    for (row_index, row) in sheet.rows.iter().enumerate() {
        if y < MARGIN + ROW_HEIGHT {
            page_index = doc.pages().len();
            doc.add_page(PAGE_WIDTH, PAGE_HEIGHT);
            y = PAGE_HEIGHT - MARGIN - ROW_HEIGHT;
        }

        let page = doc.page_mut(page_index).expect("page index is valid");

        let is_header = row_index == 0;
        for col_index in 0..row.len().min(max_cols) {
            let x = MARGIN + col_index as f32 * COL_WIDTH;
            let fill = if is_header { PdfColor::TABLE_HEADER } else { PdfColor::WHITE };
            page.add_rect(x, y - 4.0, COL_WIDTH, ROW_HEIGHT, fill);
            page.add_line(x, y - 4.0, x + COL_WIDTH, y - 4.0, PdfColor::LIGHT_GRAY, 0.5);
            page.add_line(x, y - 4.0, x, y - 4.0 + ROW_HEIGHT, PdfColor::LIGHT_GRAY, 0.5);

            let text = truncate_to_width(&row[col_index], COL_WIDTH - 6.0, CELL_FONT_SIZE);
            page.add_text(text, x + 3.0, y + 1.0, CELL_FONT_SIZE, PdfColor::BLACK, is_header);
        }
        let right = MARGIN + row.len().min(max_cols) as f32 * COL_WIDTH;
        page.add_line(MARGIN, y - 4.0 + ROW_HEIGHT, right, y - 4.0 + ROW_HEIGHT, PdfColor::LIGHT_GRAY, 0.5);
        page.add_line(right, y - 4.0, right, y - 4.0 + ROW_HEIGHT, PdfColor::LIGHT_GRAY, 0.5);
        y -= ROW_HEIGHT;
    }
}

#[cfg(test)]
mod tests {
    use super::column_index_from_ref;

    #[test]
    fn parses_excel_column_references() {
        assert_eq!(column_index_from_ref("A1"), Some(0));
        assert_eq!(column_index_from_ref("Z9"), Some(25));
        assert_eq!(column_index_from_ref("AA10"), Some(26));
    }
}