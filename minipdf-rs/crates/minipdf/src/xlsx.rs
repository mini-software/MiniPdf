use std::collections::HashMap;
use std::io::{Cursor, Read};

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
    bold: bool,
}

#[derive(Debug, Clone)]
struct SheetData {
    rows: Vec<RowData>,
    images: Vec<SheetImage>,
}

#[derive(Debug, Clone)]
struct RowData {
    index: usize,
    height: f32,
    cells: Vec<CellData>,
}

#[derive(Debug, Default)]
struct XlsxStyles {
    cell_bold: Vec<bool>,
}

#[derive(Debug, Clone)]
struct SheetImage {
    data: Vec<u8>,
    pixel_width: u16,
    pixel_height: u16,
    col: usize,
    row: usize,
    col_offset: f32,
    row_offset: f32,
    width: f32,
    height: f32,
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
    let styles = read_styles(&mut archive)?;
    let sheet_paths = read_sheet_paths(&mut archive)?;
    let mut sheets = Vec::new();

    for (_, path) in sheet_paths {
        let Some(sheet_xml) = read_zip_text(&mut archive, &path)? else {
            continue;
        };
        let images = read_sheet_images(&mut archive, &path, &sheet_xml)?;
        let rows = read_sheet_rows(&sheet_xml, &shared_strings, &styles)?;
        sheets.push(SheetData { rows, images });
    }

    if sheets.is_empty() {
        if let Some(sheet_xml) = read_zip_text(&mut archive, "xl/worksheets/sheet1.xml")? {
            sheets.push(SheetData {
                rows: read_sheet_rows(&sheet_xml, &shared_strings, &styles)?,
                images: read_sheet_images(&mut archive, "xl/worksheets/sheet1.xml", &sheet_xml)?,
            });
        }
    }

    if sheets.is_empty() {
        sheets.push(SheetData {
            rows: vec![RowData {
                index: 0,
                height: ROW_HEIGHT,
                cells: vec![CellData {
                    text: "Empty XLSX workbook".to_owned(),
                    is_numeric: false,
                    bold: false,
                }],
            }],
            images: Vec::new(),
        });
    }

    Ok(sheets)
}

fn read_sheet_images<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
    sheet_path: &str,
    sheet_xml: &str,
) -> Result<Vec<SheetImage>> {
    let sheet = roxmltree::Document::parse(sheet_xml)?;
    let Some(drawing_id) = sheet
        .descendants()
        .find(|node| node.has_tag_name("drawing"))
        .and_then(relationship_id)
    else {
        return Ok(Vec::new());
    };
    let sheet_rels = read_part_relationships(archive, sheet_path)?;
    let Some(drawing_target) = sheet_rels.get(&drawing_id) else {
        return Ok(Vec::new());
    };
    let drawing_path = resolve_part_target(sheet_path, drawing_target);
    let Some(drawing_xml) = read_zip_text(archive, &drawing_path)? else {
        return Ok(Vec::new());
    };
    let drawing_rels = read_part_relationships(archive, &drawing_path)?;
    let drawing = roxmltree::Document::parse(&drawing_xml)?;
    let mut images = Vec::new();

    for anchor in drawing
        .descendants()
        .filter(|node| node.has_tag_name("oneCellAnchor"))
    {
        let Some(from) = anchor.children().find(|node| node.has_tag_name("from")) else {
            continue;
        };
        let Some(ext) = anchor.children().find(|node| node.has_tag_name("ext")) else {
            continue;
        };
        let Some(image_id) = anchor
            .descendants()
            .find(|node| node.has_tag_name("blip"))
            .and_then(relationship_id)
        else {
            continue;
        };
        let Some(image_target) = drawing_rels.get(&image_id) else {
            continue;
        };
        let image_path = resolve_part_target(&drawing_path, image_target);
        if !image_path.to_ascii_lowercase().ends_with(".jpeg")
            && !image_path.to_ascii_lowercase().ends_with(".jpg")
        {
            continue;
        }
        let Some(data) = read_zip_bytes(archive, &image_path)? else {
            continue;
        };
        let Some((pixel_width, pixel_height)) = jpeg_dimensions(&data) else {
            continue;
        };
        images.push(SheetImage {
            data,
            pixel_width,
            pixel_height,
            col: child_number(from, "col").unwrap_or(0),
            row: child_number(from, "row").unwrap_or(0),
            col_offset: child_number(from, "colOff").unwrap_or(0) as f32 / 12_700.0,
            row_offset: child_number(from, "rowOff").unwrap_or(0) as f32 / 12_700.0,
            width: ext
                .attribute("cx")
                .and_then(|value| value.parse::<f32>().ok())
                .unwrap_or(0.0)
                / 12_700.0,
            height: ext
                .attribute("cy")
                .and_then(|value| value.parse::<f32>().ok())
                .unwrap_or(0.0)
                / 12_700.0,
        });
    }
    Ok(images)
}

fn read_part_relationships<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
    part_path: &str,
) -> Result<HashMap<String, String>> {
    let Some(file_name) = part_path.rsplit('/').next() else {
        return Ok(HashMap::new());
    };
    let parent = part_path
        .strip_suffix(file_name)
        .unwrap_or_default()
        .trim_end_matches('/');
    let rels_path = format!("{parent}/_rels/{file_name}.rels");
    let Some(rels_xml) = read_zip_text(archive, &rels_path)? else {
        return Ok(HashMap::new());
    };
    let rels_doc = roxmltree::Document::parse(&rels_xml)?;
    Ok(rels_doc
        .descendants()
        .filter(|node| node.has_tag_name("Relationship"))
        .filter_map(|rel| Some((rel.attribute("Id")?, rel.attribute("Target")?)))
        .map(|(id, target)| (id.to_owned(), target.to_owned()))
        .collect())
}

fn relationship_id(node: roxmltree::Node<'_, '_>) -> Option<String> {
    node.attributes()
        .find(|attribute| matches!(attribute.name(), "id" | "embed"))
        .map(|attribute| attribute.value().to_owned())
}

fn resolve_part_target(source_part: &str, target: &str) -> String {
    let combined = if target.starts_with('/') {
        target.trim_start_matches('/').to_owned()
    } else {
        let parent = source_part
            .rsplit_once('/')
            .map(|(path, _)| path)
            .unwrap_or("");
        format!("{parent}/{target}")
    };
    let mut segments = Vec::new();
    let normalized = combined.replace('\\', "/");
    for segment in normalized.split('/') {
        match segment {
            "" | "." => {}
            ".." => {
                segments.pop();
            }
            _ => segments.push(segment),
        }
    }
    segments.join("/")
}

fn child_number(node: roxmltree::Node<'_, '_>, name: &str) -> Option<usize> {
    node.children()
        .find(|child| child.has_tag_name(name))
        .and_then(|child| child.text())
        .and_then(|value| value.parse().ok())
}

fn read_zip_bytes<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
    path: &str,
) -> Result<Option<Vec<u8>>> {
    let Ok(mut entry) = archive.by_name(path) else {
        return Ok(None);
    };
    let mut bytes = Vec::with_capacity(entry.size() as usize);
    entry.read_to_end(&mut bytes)?;
    Ok(Some(bytes))
}

fn jpeg_dimensions(data: &[u8]) -> Option<(u16, u16)> {
    if !data.starts_with(&[0xff, 0xd8]) {
        return None;
    }
    let mut index = 2;
    while index + 3 < data.len() {
        while index < data.len() && data[index] != 0xff {
            index += 1;
        }
        while index < data.len() && data[index] == 0xff {
            index += 1;
        }
        let marker = *data.get(index)?;
        index += 1;
        if marker == 0xd9 || marker == 0xda {
            break;
        }
        if marker == 0x01 || (0xd0..=0xd8).contains(&marker) {
            continue;
        }
        let length = u16::from_be_bytes([*data.get(index)?, *data.get(index + 1)?]) as usize;
        if length < 2 || index + length > data.len() {
            return None;
        }
        if matches!(
            marker,
            0xc0..=0xc3 | 0xc5..=0xc7 | 0xc9..=0xcb | 0xcd..=0xcf
        ) {
            let height = u16::from_be_bytes([*data.get(index + 3)?, *data.get(index + 4)?]);
            let width = u16::from_be_bytes([*data.get(index + 5)?, *data.get(index + 6)?]);
            return Some((width, height));
        }
        index += length;
    }
    None
}

fn read_styles<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
) -> Result<XlsxStyles> {
    let Some(styles_xml) = read_zip_text(archive, "xl/styles.xml")? else {
        return Ok(XlsxStyles::default());
    };
    let xml = roxmltree::Document::parse(&styles_xml)?;
    let font_bold = xml
        .descendants()
        .find(|node| node.has_tag_name("fonts"))
        .map(|fonts| {
            fonts
                .children()
                .filter(|node| node.has_tag_name("font"))
                .map(|font| font.children().any(|node| node.has_tag_name("b")))
                .collect::<Vec<_>>()
        })
        .unwrap_or_default();
    let cell_bold = xml
        .descendants()
        .find(|node| node.has_tag_name("cellXfs"))
        .map(|cell_xfs| {
            cell_xfs
                .children()
                .filter(|node| node.has_tag_name("xf"))
                .map(|xf| {
                    xf.attribute("fontId")
                        .and_then(|value| value.parse::<usize>().ok())
                        .and_then(|font_id| font_bold.get(font_id).copied())
                        .unwrap_or(false)
                })
                .collect::<Vec<_>>()
        })
        .unwrap_or_default();
    Ok(XlsxStyles { cell_bold })
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

fn read_sheet_rows(
    sheet_xml: &str,
    shared_strings: &[String],
    styles: &XlsxStyles,
) -> Result<Vec<RowData>> {
    let xml = roxmltree::Document::parse(sheet_xml)?;
    let mut rows = Vec::new();

    for row in xml.descendants().filter(|node| node.has_tag_name("row")) {
        let row_index = row
            .attribute("r")
            .and_then(|value| value.parse::<usize>().ok())
            .and_then(|value| value.checked_sub(1))
            .unwrap_or(rows.len());
        let height = row
            .attribute("ht")
            .and_then(|value| value.parse::<f32>().ok())
            .filter(|value| *value > 0.0)
            .unwrap_or(ROW_HEIGHT);
        let mut cells: Vec<(usize, CellData)> = Vec::new();
        for cell in row.children().filter(|node| node.has_tag_name("c")) {
            let col = cell
                .attribute("r")
                .and_then(column_index_from_ref)
                .unwrap_or(cells.len());
            let value = read_cell_value(cell, shared_strings, styles);
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
                height,
                cells: row_values,
            });
        }
    }

    Ok(rows)
}

fn read_cell_value(
    cell: roxmltree::Node<'_, '_>,
    shared_strings: &[String],
    styles: &XlsxStyles,
) -> CellData {
    let cell_type = cell.attribute("t");
    let bold = cell
        .attribute("s")
        .and_then(|value| value.parse::<usize>().ok())
        .and_then(|style_id| styles.cell_bold.get(style_id).copied())
        .unwrap_or(false);
    if cell_type == Some("inlineStr") {
        let text = cell
            .descendants()
            .filter(|node| node.has_tag_name("t"))
            .filter_map(|node| node.text())
            .collect::<String>();
        return CellData {
            text,
            is_numeric: false,
            bold,
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
            bold,
        };
    }

    CellData {
        text: value.to_owned(),
        is_numeric: matches!(cell_type, None | Some("n")) && value.parse::<f64>().is_ok(),
        bold,
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
        let image_ids = sheet
            .images
            .iter()
            .map(|image| {
                doc.add_jpeg_image(image.data.clone(), image.pixel_width, image.pixel_height)
            })
            .collect::<Vec<_>>();
        render_sheet(doc, sheet, &image_ids);
    }
}

fn render_sheet(doc: &mut PdfDocument, sheet: &SheetData, image_ids: &[usize]) {
    let max_cols = ((PAGE_WIDTH - MARGIN_X * 2.0) / COL_WIDTH).floor() as usize;
    let column_count = sheet
        .rows
        .iter()
        .map(|row| row.cells.len())
        .max()
        .unwrap_or(0)
        .max(1);

    for column_start in (0..column_count).step_by(max_cols) {
        render_sheet_columns(doc, sheet, image_ids, column_start, max_cols);
    }
}

fn render_sheet_columns(
    doc: &mut PdfDocument,
    sheet: &SheetData,
    image_ids: &[usize],
    column_start: usize,
    max_cols: usize,
) {
    let mut page_index = doc.pages().len();
    doc.add_page(PAGE_WIDTH, PAGE_HEIGHT);
    let mut y = PAGE_HEIGHT - MARGIN_TOP;
    let mut next_row_index = 0;

    for row in &sheet.rows {
        for _ in next_row_index..row.index {
            advance_xlsx_row(doc, &mut page_index, &mut y, ROW_HEIGHT);
        }

        if y - row.height < MARGIN_BOTTOM {
            page_index = doc.pages().len();
            doc.add_page(PAGE_WIDTH, PAGE_HEIGHT);
            y = PAGE_HEIGHT - MARGIN_TOP;
        }
        y -= row.height;

        let page = doc.page_mut(page_index).expect("page index is valid");

        for (image, image_id) in sheet.images.iter().zip(image_ids).filter(|(image, _)| {
            image.row == row.index
                && image.col >= column_start
                && image.col < column_start + max_cols
        }) {
            let x = MARGIN_X + (image.col - column_start) as f32 * COL_WIDTH + image.col_offset;
            let top = y + row.height - image.row_offset;
            page.add_image(*image_id, x, top - image.height, image.width, image.height);
        }

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
                cell.bold,
            );
        }
        next_row_index = row.index + 1;
    }
}

fn advance_xlsx_row(doc: &mut PdfDocument, page_index: &mut usize, y: &mut f32, row_height: f32) {
    if *y - row_height < MARGIN_BOTTOM {
        *page_index = doc.pages().len();
        doc.add_page(PAGE_WIDTH, PAGE_HEIGHT);
        *y = PAGE_HEIGHT - MARGIN_TOP;
    }
    *y -= row_height;
}

#[cfg(test)]
mod tests {
    use super::{
        column_index_from_ref, jpeg_dimensions, read_sheet_rows, relationship_id, XlsxStyles,
    };

    #[test]
    fn parses_excel_column_references() {
        assert_eq!(column_index_from_ref("A1"), Some(0));
        assert_eq!(column_index_from_ref("Z9"), Some(25));
        assert_eq!(column_index_from_ref("AA10"), Some(26));
    }

    #[test]
    fn preserves_sparse_row_indices() {
        let xml = r#"<worksheet><sheetData><row r="1"><c r="A1" t="inlineStr"><is><t>First</t></is></c></row><row r="5"><c r="A5" t="inlineStr"><is><t>Fifth</t></is></c></row></sheetData></worksheet>"#;
        let rows =
            read_sheet_rows(xml, &[], &XlsxStyles::default()).expect("worksheet XML is valid");

        assert_eq!(rows.len(), 2);
        assert_eq!(rows[0].index, 0);
        assert_eq!(rows[1].index, 4);
    }

    #[test]
    fn preserves_custom_row_height_and_bold_cell_style() {
        let xml = r#"<worksheet><sheetData><row r="1" ht="68"><c r="D1" s="1" t="inlineStr"><is><t>Product Name</t></is></c></row></sheetData></worksheet>"#;
        let styles = XlsxStyles {
            cell_bold: vec![false, true],
        };
        let rows = read_sheet_rows(xml, &[], &styles).expect("worksheet XML is valid");

        assert_eq!(rows[0].height, 68.0);
        assert!(rows[0].cells[3].bold);
    }

    #[test]
    fn reads_jpeg_dimensions_from_start_of_frame() {
        let jpeg = [
            0xff, 0xd8, 0xff, 0xc0, 0x00, 0x11, 0x08, 0x00, 0x78, 0x00, 0xa0, 0x03, 0x01, 0x11,
            0x00, 0x02, 0x11, 0x00, 0x03, 0x11, 0x00, 0xff, 0xd9,
        ];

        assert_eq!(jpeg_dimensions(&jpeg), Some((160, 120)));
    }

    #[test]
    fn reads_drawing_relationship_ids_and_embeds() {
        let xml = roxmltree::Document::parse(
            r#"<root xmlns:r="relationships"><drawing r:id="rId1"/><blip r:embed="rId2"/></root>"#,
        )
        .expect("relationship XML is valid");
        let mut children = xml
            .root_element()
            .children()
            .filter(|node| node.is_element());

        assert_eq!(
            relationship_id(children.next().unwrap()).as_deref(),
            Some("rId1")
        );
        assert_eq!(
            relationship_id(children.next().unwrap()).as_deref(),
            Some("rId2")
        );
    }
}
