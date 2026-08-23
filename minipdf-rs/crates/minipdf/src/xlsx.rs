use std::collections::HashMap;
use std::io::{Cursor, Read};

use zip::ZipArchive;

use crate::pdf::{PdfColor, PdfDocument, PdfTextStyle};
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
    style: CellStyle,
}

#[derive(Debug, Clone)]
struct SheetData {
    rows: Vec<RowData>,
    images: Vec<SheetImage>,
    column_widths: Vec<f32>,
    merges: Vec<MergeRange>,
}

#[derive(Debug, Clone)]
struct RowData {
    index: usize,
    height: f32,
    cells: Vec<CellData>,
}

#[derive(Debug, Clone, Copy)]
struct CellStyle {
    bold: bool,
    italic: bool,
    font_size: f32,
    font_color: PdfColor,
    fill_color: Option<PdfColor>,
    border_color: Option<PdfColor>,
    centered: bool,
    vertical_alignment: VerticalAlignment,
}

#[derive(Debug, Clone, Copy, Default, PartialEq)]
enum VerticalAlignment {
    Top,
    Center,
    #[default]
    Bottom,
}

impl Default for CellStyle {
    fn default() -> Self {
        Self {
            bold: false,
            italic: false,
            font_size: CELL_FONT_SIZE,
            font_color: PdfColor::BLACK,
            fill_color: None,
            border_color: None,
            centered: false,
            vertical_alignment: VerticalAlignment::Bottom,
        }
    }
}

#[derive(Debug, Clone, Copy)]
struct FontStyle {
    bold: bool,
    italic: bool,
    size: f32,
    color: PdfColor,
}

impl Default for FontStyle {
    fn default() -> Self {
        Self {
            bold: false,
            italic: false,
            size: CELL_FONT_SIZE,
            color: PdfColor::BLACK,
        }
    }
}

#[derive(Debug, Default)]
struct XlsxStyles {
    cells: Vec<CellStyle>,
}

#[derive(Debug, Clone, Copy)]
struct MergeRange {
    start_col: usize,
    end_col: usize,
    start_row: usize,
    end_row: usize,
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
        let column_widths = read_column_widths(&sheet_xml)?;
        let merges = read_merge_ranges(&sheet_xml)?;
        sheets.push(SheetData {
            rows,
            images,
            column_widths,
            merges,
        });
    }

    if sheets.is_empty() {
        if let Some(sheet_xml) = read_zip_text(&mut archive, "xl/worksheets/sheet1.xml")? {
            sheets.push(SheetData {
                rows: read_sheet_rows(&sheet_xml, &shared_strings, &styles)?,
                images: read_sheet_images(&mut archive, "xl/worksheets/sheet1.xml", &sheet_xml)?,
                column_widths: read_column_widths(&sheet_xml)?,
                merges: read_merge_ranges(&sheet_xml)?,
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
                    style: CellStyle::default(),
                }],
            }],
            images: Vec::new(),
            column_widths: Vec::new(),
            merges: Vec::new(),
        });
    }

    Ok(sheets)
}

fn read_column_widths(sheet_xml: &str) -> Result<Vec<f32>> {
    let xml = roxmltree::Document::parse(sheet_xml)?;
    let mut widths = Vec::new();
    for column in xml.descendants().filter(|node| node.has_tag_name("col")) {
        let Some(min) = column
            .attribute("min")
            .and_then(|value| value.parse::<usize>().ok())
        else {
            continue;
        };
        let max = column
            .attribute("max")
            .and_then(|value| value.parse::<usize>().ok())
            .unwrap_or(min);
        let width = if matches!(column.attribute("hidden"), Some("1" | "true")) {
            0.0
        } else {
            column
                .attribute("width")
                .and_then(|value| value.parse::<f32>().ok())
                .filter(|value| *value > 0.0)
                .map(excel_column_width_to_points)
                .unwrap_or(COL_WIDTH)
        };
        if widths.len() < max {
            widths.resize(max, COL_WIDTH);
        }
        for column_index in min..=max {
            widths[column_index - 1] = width;
        }
    }
    Ok(widths)
}

fn excel_column_width_to_points(char_units: f32) -> f32 {
    char_units * 5.55
}

fn read_merge_ranges(sheet_xml: &str) -> Result<Vec<MergeRange>> {
    let xml = roxmltree::Document::parse(sheet_xml)?;
    Ok(xml
        .descendants()
        .filter(|node| node.has_tag_name("mergeCell"))
        .filter_map(|node| node.attribute("ref"))
        .filter_map(|reference| {
            let (start, end) = reference.split_once(':')?;
            let (start_col, start_row) = cell_position(start)?;
            let (end_col, end_row) = cell_position(end)?;
            Some(MergeRange {
                start_col,
                end_col,
                start_row,
                end_row,
            })
        })
        .collect())
}

fn column_groups(widths: &[f32], usable_width: f32) -> Vec<(usize, usize)> {
    let mut groups = Vec::new();
    let mut start = 0;
    while start < widths.len() {
        let mut end = start;
        let mut group_width = 0.0;
        while end < widths.len() {
            let width = widths[end];
            if end > start && group_width + width > usable_width {
                break;
            }
            group_width += width;
            end += 1;
        }
        groups.push((start, end.max(start + 1)));
        start = end.max(start + 1);
    }
    groups
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
    let fonts = xml
        .descendants()
        .find(|node| node.has_tag_name("fonts"))
        .map(|fonts| {
            fonts
                .children()
                .filter(|node| node.has_tag_name("font"))
                .map(|font| FontStyle {
                    bold: font
                        .children()
                        .any(|node| node.has_tag_name("b") && node.attribute("val") != Some("0")),
                    italic: font
                        .children()
                        .any(|node| node.has_tag_name("i") && node.attribute("val") != Some("0")),
                    size: font
                        .children()
                        .find(|node| node.has_tag_name("sz"))
                        .and_then(|node| node.attribute("val"))
                        .and_then(|value| value.parse::<f32>().ok())
                        .unwrap_or(CELL_FONT_SIZE),
                    color: font
                        .children()
                        .find(|node| node.has_tag_name("color"))
                        .and_then(|node| node.attribute("rgb"))
                        .and_then(parse_rgb_color)
                        .unwrap_or(PdfColor::BLACK),
                })
                .collect::<Vec<_>>()
        })
        .unwrap_or_default();
    let fills = xml
        .descendants()
        .find(|node| node.has_tag_name("fills"))
        .map(|fills| {
            fills
                .children()
                .filter(|node| node.has_tag_name("fill"))
                .map(|fill| {
                    fill.descendants()
                        .find(|node| node.has_tag_name("patternFill"))
                        .filter(|node| node.attribute("patternType") == Some("solid"))
                        .and_then(|pattern| {
                            pattern.children().find(|node| node.has_tag_name("fgColor"))
                        })
                        .and_then(|color| color.attribute("rgb"))
                        .and_then(parse_rgb_color)
                })
                .collect::<Vec<_>>()
        })
        .unwrap_or_default();
    let borders = xml
        .descendants()
        .find(|node| node.has_tag_name("borders"))
        .map(|borders| {
            borders
                .children()
                .filter(|node| node.has_tag_name("border"))
                .map(|border| {
                    border
                        .children()
                        .find(|side| side.attribute("style").is_some())
                        .map(|side| {
                            side.children()
                                .find(|node| node.has_tag_name("color"))
                                .and_then(|node| node.attribute("rgb"))
                                .and_then(parse_rgb_color)
                                .unwrap_or(PdfColor::BLACK)
                        })
                })
                .collect::<Vec<_>>()
        })
        .unwrap_or_default();
    let cells = xml
        .descendants()
        .find(|node| node.has_tag_name("cellXfs"))
        .map(|cell_xfs| {
            cell_xfs
                .children()
                .filter(|node| node.has_tag_name("xf"))
                .map(|xf| {
                    let font = xf
                        .attribute("fontId")
                        .and_then(|value| value.parse::<usize>().ok())
                        .and_then(|font_id| fonts.get(font_id).copied())
                        .unwrap_or_default();
                    let fill_color = xf
                        .attribute("fillId")
                        .and_then(|value| value.parse::<usize>().ok())
                        .and_then(|fill_id| fills.get(fill_id).copied().flatten());
                    let border_color = xf
                        .attribute("borderId")
                        .and_then(|value| value.parse::<usize>().ok())
                        .and_then(|border_id| borders.get(border_id).copied().flatten());
                    let alignment = xf.children().find(|node| node.has_tag_name("alignment"));
                    CellStyle {
                        bold: font.bold,
                        italic: font.italic,
                        font_size: font.size,
                        font_color: font.color,
                        fill_color,
                        border_color,
                        centered: alignment.and_then(|node| node.attribute("horizontal"))
                            == Some("center"),
                        vertical_alignment: parse_vertical_alignment(
                            alignment.and_then(|node| node.attribute("vertical")),
                        ),
                    }
                })
                .collect::<Vec<_>>()
        })
        .unwrap_or_default();
    Ok(XlsxStyles { cells })
}

fn parse_rgb_color(value: &str) -> Option<PdfColor> {
    let rgb = value.get(value.len().checked_sub(6)?..)?;
    let number = u32::from_str_radix(rgb, 16).ok()?;
    Some(PdfColor::new(
        ((number >> 16) & 0xff) as f32 / 255.0,
        ((number >> 8) & 0xff) as f32 / 255.0,
        (number & 0xff) as f32 / 255.0,
    ))
}

fn parse_vertical_alignment(value: Option<&str>) -> VerticalAlignment {
    match value {
        Some("top") => VerticalAlignment::Top,
        Some("center") => VerticalAlignment::Center,
        _ => VerticalAlignment::Bottom,
    }
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
        let has_custom_height = row.attribute("ht").is_some();
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
        if has_custom_height || row_values.iter().any(|cell| !cell.text.is_empty()) {
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
    let style = cell
        .attribute("s")
        .and_then(|value| value.parse::<usize>().ok())
        .and_then(|style_id| styles.cells.get(style_id).copied())
        .unwrap_or_default();
    if cell_type == Some("inlineStr") {
        let text = cell
            .descendants()
            .filter(|node| node.has_tag_name("t"))
            .filter_map(|node| node.text())
            .collect::<String>();
        return CellData {
            text,
            is_numeric: false,
            style,
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
            style,
        };
    }

    let numeric_value = matches!(cell_type, None | Some("n"))
        .then(|| value.parse::<f64>().ok())
        .flatten();
    CellData {
        text: numeric_value
            .filter(|_| {
                !value.contains(['e', 'E'])
                    && value
                        .split_once('.')
                        .is_some_and(|(_, fraction)| fraction.len() >= 15)
            })
            .map(|value| value.to_string())
            .unwrap_or_else(|| value.to_owned()),
        is_numeric: numeric_value.is_some(),
        style,
    }
}

fn cell_position(cell_ref: &str) -> Option<(usize, usize)> {
    let col = column_index_from_ref(cell_ref)?;
    let row = cell_ref
        .chars()
        .skip_while(|ch| ch.is_ascii_alphabetic() || *ch == '$')
        .collect::<String>()
        .parse::<usize>()
        .ok()?
        .checked_sub(1)?;
    Some((col, row))
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
    let column_count = sheet
        .rows
        .iter()
        .map(|row| row.cells.len())
        .max()
        .unwrap_or(0)
        .max(1);
    let mut widths = sheet.column_widths.clone();
    widths.resize(column_count, COL_WIDTH);

    for (column_start, column_end) in column_groups(&widths, PAGE_WIDTH - MARGIN_X * 2.0) {
        render_sheet_columns(doc, sheet, image_ids, &widths, column_start, column_end);
    }
}

fn render_sheet_columns(
    doc: &mut PdfDocument,
    sheet: &SheetData,
    image_ids: &[usize],
    column_widths: &[f32],
    column_start: usize,
    column_end: usize,
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
            image.row == row.index && image.col >= column_start && image.col < column_end
        }) {
            let x = MARGIN_X
                + column_widths[column_start..image.col].iter().sum::<f32>()
                + image.col_offset;
            let top = y + row.height - image.row_offset;
            page.add_image(*image_id, x, top - image.height, image.width, image.height);
        }

        let mut cell_x = MARGIN_X;
        for (column_index, cell) in row
            .cells
            .iter()
            .enumerate()
            .skip(column_start)
            .take(column_end - column_start)
        {
            let merge = sheet.merges.iter().find(|merge| {
                merge.start_row == row.index
                    && merge.start_col == column_index
                    && merge.end_row >= row.index
            });
            let merge_end = merge
                .map(|merge| (merge.end_col + 1).min(column_end))
                .unwrap_or(column_index + 1);
            let cell_width = column_widths[column_index..merge_end].iter().sum::<f32>();
            if let Some(fill) = cell.style.fill_color {
                page.add_rect(cell_x, y, cell_width, row.height, fill);
            }
            if let Some(border) = cell.style.border_color {
                page.add_line(cell_x, y, cell_x + cell_width, y, border, 0.5);
                page.add_line(
                    cell_x,
                    y + row.height,
                    cell_x + cell_width,
                    y + row.height,
                    border,
                    0.5,
                );
                page.add_line(cell_x, y, cell_x, y + row.height, border, 0.5);
                page.add_line(
                    cell_x + cell_width,
                    y,
                    cell_x + cell_width,
                    y + row.height,
                    border,
                    0.5,
                );
            }
            let font_size = if cell.style.bold {
                cell.style.font_size
            } else {
                cell.style.font_size * (10.0 / 11.0)
            };
            let text_width = text_width(&cell.text, font_size);
            let x = if cell.is_numeric {
                cell_x + cell_width - text_width - 3.0
            } else if cell.style.centered {
                cell_x + (cell_width - text_width).max(0.0) / 2.0
            } else {
                cell_x + 3.0
            };
            let text_y = match cell.style.vertical_alignment {
                VerticalAlignment::Top => y + (row.height - cell.style.font_size).max(0.0),
                VerticalAlignment::Center => y + (row.height - cell.style.font_size).max(0.0) / 2.0,
                VerticalAlignment::Bottom => y + 1.0,
            };
            page.add_styled_text(
                &cell.text,
                x,
                text_y,
                font_size,
                PdfTextStyle {
                    color: cell.style.font_color,
                    bold: cell.style.bold,
                    italic: cell.style.italic,
                },
            );
            cell_x += column_widths[column_index];
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
        column_groups, column_index_from_ref, jpeg_dimensions, parse_rgb_color,
        parse_vertical_alignment, read_column_widths, read_merge_ranges, read_sheet_rows,
        relationship_id, CellStyle, VerticalAlignment, XlsxStyles,
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
    fn normalizes_general_numeric_values() {
        let xml = r#"<worksheet><sheetData><row r="1"><c r="A1" t="n"><v>8.859999999999999</v></c><c r="B1" t="n"><v>6.022e+23</v></c></row></sheetData></worksheet>"#;
        let rows =
            read_sheet_rows(xml, &[], &XlsxStyles::default()).expect("worksheet XML is valid");

        assert_eq!(rows[0].cells[0].text, "8.86");
        assert!(rows[0].cells[0].is_numeric);
        assert_eq!(rows[0].cells[1].text, "6.022e+23");
        assert!(rows[0].cells[1].is_numeric);
    }

    #[test]
    fn preserves_custom_row_height_and_bold_cell_style() {
        let xml = r#"<worksheet><sheetData><row r="1" ht="68"><c r="D1" s="1" t="inlineStr"><is><t>Product Name</t></is></c></row></sheetData></worksheet>"#;
        let styles = XlsxStyles {
            cells: vec![
                CellStyle::default(),
                CellStyle {
                    bold: true,
                    ..CellStyle::default()
                },
            ],
        };
        let rows = read_sheet_rows(xml, &[], &styles).expect("worksheet XML is valid");

        assert_eq!(rows[0].height, 68.0);
        assert!(rows[0].cells[3].style.bold);
    }

    #[test]
    fn preserves_empty_custom_height_rows_for_image_anchors() {
        let xml = r#"<worksheet><sheetData><row r="4" ht="60" customHeight="1"/></sheetData></worksheet>"#;
        let rows =
            read_sheet_rows(xml, &[], &XlsxStyles::default()).expect("worksheet XML is valid");

        assert_eq!(rows.len(), 1);
        assert_eq!(rows[0].index, 3);
        assert_eq!(rows[0].height, 60.0);
        assert!(rows[0].cells.is_empty());
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

    #[test]
    fn uses_explicit_column_widths_for_horizontal_page_groups() {
        let xml = r#"<worksheet><cols><col min="1" max="1" width="15"/><col min="2" max="2" width="20"/><col min="3" max="4" width="15"/><col min="5" max="5" width="22"/><col min="6" max="14" width="15"/></cols></worksheet>"#;
        let widths = read_column_widths(xml).expect("worksheet XML is valid");
        let groups = column_groups(&widths, 508.0);

        assert_eq!(widths[0], 83.25);
        assert_eq!(widths.len(), 14);
        assert_eq!(groups, vec![(0, 5), (5, 11), (11, 14)]);
    }

    #[test]
    fn reads_merged_ranges_and_argb_colors() {
        let xml = r#"<worksheet><mergeCells><mergeCell ref="A1:N1"/></mergeCells></worksheet>"#;
        let merges = read_merge_ranges(xml).expect("worksheet XML is valid");
        let color = parse_rgb_color("004472C4").expect("ARGB color is valid");

        assert_eq!(merges[0].start_col, 0);
        assert_eq!(merges[0].end_col, 13);
        assert_eq!(merges[0].start_row, 0);
        assert_eq!(color.r, 68.0 / 255.0);
        assert_eq!(color.g, 114.0 / 255.0);
        assert_eq!(color.b, 196.0 / 255.0);
    }

    #[test]
    fn distinguishes_top_center_and_bottom_alignment() {
        assert_eq!(
            parse_vertical_alignment(Some("top")),
            VerticalAlignment::Top
        );
        assert_eq!(
            parse_vertical_alignment(Some("center")),
            VerticalAlignment::Center
        );
        assert_eq!(
            parse_vertical_alignment(Some("bottom")),
            VerticalAlignment::Bottom
        );
        assert_eq!(parse_vertical_alignment(None), VerticalAlignment::Bottom);
    }
}
