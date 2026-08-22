use std::collections::HashMap;
use std::io::Cursor;

use zip::ZipArchive;

use crate::pdf::{PdfColor, PdfDocument};
use crate::{read_zip_text, text_width, Result};

const PAGE_WIDTH: f32 = 612.0;
const PAGE_HEIGHT: f32 = 792.0;
const MARGIN_X: f32 = 52.0;
const MARGIN_TOP: f32 = 70.55;
const MARGIN_BOTTOM: f32 = 54.0;
const CELL_FONT_SIZE: f32 = 11.0;
const ROW_HEIGHT: f32 = 15.0;
const COL_WIDTH: f32 = 47.4;

#[derive(Debug, Clone, Default)]
struct CellData {
    text: String,
    is_numeric: bool,
}

#[derive(Debug, Clone)]
struct SheetData {
    rows: Vec<RowData>,
}

#[derive(Debug, Clone)]
struct RowData {
    index: usize,
    cells: Vec<CellData>,
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

    for (_, path) in sheet_paths {
        let Some(sheet_xml) = read_zip_text(&mut archive, &path)? else {
            continue;
        };
        let rows = read_sheet_rows(&sheet_xml, &shared_strings)?;
        sheets.push(SheetData { rows });
    }

    if sheets.is_empty() {
        if let Some(sheet_xml) = read_zip_text(&mut archive, "xl/worksheets/sheet1.xml")? {
            sheets.push(SheetData {
                rows: read_sheet_rows(&sheet_xml, &shared_strings)?,
            });
        }
    }

    if sheets.is_empty() {
        sheets.push(SheetData {
            rows: vec![RowData {
                index: 0,
                cells: vec![CellData {
                    text: "Empty XLSX workbook".to_owned(),
                    is_numeric: false,
                }],
            }],
        });
    }

    Ok(sheets)
}

fn read_shared_strings<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
) -> Result<Vec<String>> {
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

fn read_sheet_paths<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
) -> Result<Vec<(String, String)>> {
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

fn read_workbook_relationships<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
) -> Result<HashMap<String, String>> {
    let Some(rels_xml) = read_zip_text(archive, "xl/_rels/workbook.xml.rels")? else {
        return Ok(HashMap::new());
    };
    let rels_doc = roxmltree::Document::parse(&rels_xml)?;
    let mut rels = HashMap::new();
    for rel in rels_doc
        .descendants()
        .filter(|node| node.has_tag_name("Relationship"))
    {
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

fn read_sheet_rows(sheet_xml: &str, shared_strings: &[String]) -> Result<Vec<RowData>> {
    let xml = roxmltree::Document::parse(sheet_xml)?;
    let mut rows = Vec::new();

    for row in xml.descendants().filter(|node| node.has_tag_name("row")) {
        let row_index = row
            .attribute("r")
            .and_then(|value| value.parse::<usize>().ok())
            .and_then(|value| value.checked_sub(1))
            .unwrap_or(rows.len());
        let mut cells: Vec<(usize, CellData)> = Vec::new();
        for cell in row.children().filter(|node| node.has_tag_name("c")) {
            let col = cell
                .attribute("r")
                .and_then(column_index_from_ref)
                .unwrap_or(cells.len());
            let value = read_cell_value(cell, shared_strings);
            cells.push((col, value));
        }

        let width = cells
            .iter()
            .map(|(col, _)| *col)
            .max()
            .map(|col| col + 1)
            .unwrap_or(0);
        let mut row_values = vec![CellData::default(); width];
        for (col, value) in cells {
            if let Some(slot) = row_values.get_mut(col) {
                *slot = value;
            }
        }
        if row_values.iter().any(|cell| !cell.text.is_empty()) {
            rows.push(RowData {
                index: row_index,
                cells: row_values,
            });
        }
    }

    Ok(rows)
}

fn read_cell_value(cell: roxmltree::Node<'_, '_>, shared_strings: &[String]) -> CellData {
    let cell_type = cell.attribute("t");
    if cell_type == Some("inlineStr") {
        let text = cell
            .descendants()
            .filter(|node| node.has_tag_name("t"))
            .filter_map(|node| node.text())
            .collect::<String>();
        return CellData {
            text,
            is_numeric: false,
        };
    }

    let value = cell
        .children()
        .find(|node| node.has_tag_name("v"))
        .and_then(|node| node.text())
        .unwrap_or("");

    if cell_type == Some("s") {
        let text = value
            .parse::<usize>()
            .ok()
            .and_then(|index| shared_strings.get(index).cloned())
            .unwrap_or_default();
        return CellData {
            text,
            is_numeric: false,
        };
    }

    CellData {
        text: value.to_owned(),
        is_numeric: matches!(cell_type, None | Some("n")) && value.parse::<f64>().is_ok(),
    }
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
    let max_cols = ((PAGE_WIDTH - MARGIN_X * 2.0) / COL_WIDTH).floor() as usize;
    let column_count = sheet
        .rows
        .iter()
        .map(|row| row.cells.len())
        .max()
        .unwrap_or(0)
        .max(1);

    for column_start in (0..column_count).step_by(max_cols) {
        render_sheet_columns(doc, sheet, column_start, max_cols);
    }
}

fn render_sheet_columns(
    doc: &mut PdfDocument,
    sheet: &SheetData,
    column_start: usize,
    max_cols: usize,
) {
    let mut page_index = doc.pages().len();
    doc.add_page(PAGE_WIDTH, PAGE_HEIGHT);
    let mut y = PAGE_HEIGHT - MARGIN_TOP - ROW_HEIGHT;
    let mut next_row_index = 0;

    for row in &sheet.rows {
        for _ in next_row_index..row.index {
            advance_xlsx_row(doc, &mut page_index, &mut y);
        }

        if y < MARGIN_BOTTOM + ROW_HEIGHT {
            page_index = doc.pages().len();
            doc.add_page(PAGE_WIDTH, PAGE_HEIGHT);
            y = PAGE_HEIGHT - MARGIN_TOP - ROW_HEIGHT;
        }

        let page = doc.page_mut(page_index).expect("page index is valid");

        for (page_col_index, cell) in row
            .cells
            .iter()
            .skip(column_start)
            .take(max_cols)
            .enumerate()
        {
            let cell_x = MARGIN_X + page_col_index as f32 * COL_WIDTH;
            let x = if cell.is_numeric {
                cell_x + COL_WIDTH - text_width(&cell.text, CELL_FONT_SIZE)
            } else {
                cell_x + 3.0
            };
            page.add_text(
                &cell.text,
                x,
                y + 1.0,
                CELL_FONT_SIZE,
                PdfColor::BLACK,
                false,
            );
        }
        advance_xlsx_row(doc, &mut page_index, &mut y);
        next_row_index = row.index + 1;
    }
}

fn advance_xlsx_row(doc: &mut PdfDocument, page_index: &mut usize, y: &mut f32) {
    *y -= ROW_HEIGHT;
    if *y < MARGIN_BOTTOM + ROW_HEIGHT {
        *page_index = doc.pages().len();
        doc.add_page(PAGE_WIDTH, PAGE_HEIGHT);
        *y = PAGE_HEIGHT - MARGIN_TOP - ROW_HEIGHT;
    }
}

#[cfg(test)]
mod tests {
    use super::{column_index_from_ref, read_sheet_rows};

    #[test]
    fn parses_excel_column_references() {
        assert_eq!(column_index_from_ref("A1"), Some(0));
        assert_eq!(column_index_from_ref("Z9"), Some(25));
        assert_eq!(column_index_from_ref("AA10"), Some(26));
    }

    #[test]
    fn preserves_sparse_row_indices() {
        let xml = r#"<worksheet><sheetData><row r="1"><c r="A1" t="inlineStr"><is><t>First</t></is></c></row><row r="5"><c r="A5" t="inlineStr"><is><t>Fifth</t></is></c></row></sheetData></worksheet>"#;
        let rows = read_sheet_rows(xml, &[]).expect("worksheet XML is valid");

        assert_eq!(rows.len(), 2);
        assert_eq!(rows[0].index, 0);
        assert_eq!(rows[1].index, 4);
    }
}
