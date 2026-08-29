use std::collections::HashMap;
use std::io::{Cursor, Read};

use unicode_bidi::{bidi_class, BidiClass};
use zip::ZipArchive;

use crate::pdf::{styled_text_width_with_font, PdfColor, PdfDocument, PdfTextStyle};
use crate::{read_zip_text, ConversionOptions, PageSize, Result};

const MARGIN_X: f32 = 54.0;
const MARGIN_TOP: f32 = 70.55;
const MARGIN_BOTTOM: f32 = 54.0;
const CELL_FONT_SIZE: f32 = 11.0;
const CELL_BORDER_WIDTH: f32 = 0.96;
const PRINT_TITLE_HORIZONTAL_SCALE: f32 = 0.9957;
const PRINT_TITLE_HORIZONTAL_OFFSET: f32 = 0.4;
const FIT_TO_PAGE_HORIZONTAL_SCALE: f32 = 1.0048;
const FIT_TO_PAGE_HORIZONTAL_OFFSET: f32 = 0.5;
const PRINT_TITLE_VERTICAL_SCALE: f32 = 0.98;
const FIT_TO_PAGE_VERTICAL_SCALE: f32 = 1.0104;
const FIT_TO_WIDTH_ONLY_VERTICAL_SCALE: f32 = 0.972;
const PRINT_TITLE_WIDTH_ONLY_VERTICAL_SCALE: f32 = 1.0233;
const FIT_TO_PAGE_TOP_OFFSET: f32 = 0.96;
const SVG_FALLBACK_HORIZONTAL_SCALE: f32 = 0.972;
const GROUP_DRAWING_TOP_OFFSET: f32 = 0.96;
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
    row_breaks: Vec<usize>,
    default_row_height: f32,
    print_title_rows: Option<(usize, usize)>,
    page_setup: SheetPageSetup,
}

#[derive(Debug, Clone, Copy)]
struct SheetPageSetup {
    page_size: PageSize,
    page_size_from_printer: bool,
    margin_left: f32,
    margin_right: f32,
    margin_top: f32,
    margin_bottom: f32,
    print_scale: f32,
    fit_to_width: bool,
    fit_to_height: bool,
    horizontal_centered: bool,
}

impl Default for SheetPageSetup {
    fn default() -> Self {
        Self {
            page_size: PageSize::A4,
            page_size_from_printer: false,
            margin_left: MARGIN_X,
            margin_right: MARGIN_X,
            margin_top: MARGIN_TOP,
            margin_bottom: MARGIN_BOTTOM,
            print_scale: 1.0,
            fit_to_width: false,
            fit_to_height: false,
            horizontal_centered: false,
        }
    }
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
    strike: bool,
    font_size: f32,
    font_color: PdfColor,
    fill_color: Option<PdfColor>,
    fill_override: bool,
    borders: CellBorders,
    number_format: NumberFormat,
    horizontal_alignment: HorizontalAlignment,
    vertical_alignment: VerticalAlignment,
    indent: f32,
    wrap_text: bool,
    preferred_font: Option<&'static str>,
}

#[derive(Debug, Clone, Copy, Default)]
struct CellBorders {
    left: Option<PdfColor>,
    right: Option<PdfColor>,
    top: Option<PdfColor>,
    bottom: Option<PdfColor>,
    left_width: f32,
    right_width: f32,
    top_width: f32,
    bottom_width: f32,
    left_dotted: bool,
    right_dotted: bool,
    top_dotted: bool,
    bottom_dotted: bool,
}

impl CellBorders {
    fn any(self) -> bool {
        self.left.is_some() || self.right.is_some() || self.top.is_some() || self.bottom.is_some()
    }
}

#[derive(Debug, Clone, Copy, Default)]
struct TableBorders {
    edges: CellBorders,
    vertical: Option<(PdfColor, f32)>,
    horizontal: Option<(PdfColor, f32)>,
}

#[derive(Debug, Clone, Copy, Default)]
enum NumberFormat {
    #[default]
    General,
    DateMonthDayYear,
    PercentageZeroDecimals,
    PercentageTwoDecimals,
    ThousandsTwoDecimals,
    DollarTwoDecimals,
    DollarAccounting,
}

#[derive(Debug, Clone, Copy, Default, PartialEq)]
enum HorizontalAlignment {
    #[default]
    General,
    Left,
    Center,
    Right,
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
            strike: false,
            font_size: CELL_FONT_SIZE,
            font_color: PdfColor::BLACK,
            fill_color: None,
            fill_override: false,
            borders: CellBorders::default(),
            number_format: NumberFormat::General,
            horizontal_alignment: HorizontalAlignment::General,
            vertical_alignment: VerticalAlignment::Bottom,
            indent: 0.0,
            wrap_text: false,
            preferred_font: None,
        }
    }
}

#[derive(Debug, Clone, Copy)]
struct FontStyle {
    bold: bool,
    italic: bool,
    size: f32,
    color: PdfColor,
    preferred_font: Option<&'static str>,
}

#[derive(Debug, Clone, Copy, Default)]
struct DifferentialFontStyle {
    bold: Option<bool>,
    italic: Option<bool>,
    strike: Option<bool>,
    color: Option<PdfColor>,
}

impl DifferentialFontStyle {
    fn any(self) -> bool {
        self.bold.is_some()
            || self.italic.is_some()
            || self.strike.is_some()
            || self.color.is_some()
    }
}

impl Default for FontStyle {
    fn default() -> Self {
        Self {
            bold: false,
            italic: false,
            size: CELL_FONT_SIZE,
            color: PdfColor::BLACK,
            preferred_font: None,
        }
    }
}

#[derive(Debug, Default)]
struct XlsxStyles {
    cells: Vec<CellStyle>,
    differential_fonts: Vec<DifferentialFontStyle>,
    differential_fills: Vec<Option<PdfColor>>,
    max_digit_width: f32,
    table_base_fills: HashMap<String, PdfColor>,
    table_first_row_stripes: HashMap<String, (PdfColor, usize)>,
    table_header_fills: HashMap<String, PdfColor>,
    table_total_fills: HashMap<String, PdfColor>,
    table_whole_fonts: HashMap<String, DifferentialFontStyle>,
    table_header_fonts: HashMap<String, DifferentialFontStyle>,
    table_total_fonts: HashMap<String, DifferentialFontStyle>,
    table_whole_borders: HashMap<String, TableBorders>,
    table_header_borders: HashMap<String, TableBorders>,
    table_total_borders: HashMap<String, TableBorders>,
}

fn parse_number_format(
    format_id: Option<&str>,
    custom_formats: &HashMap<usize, String>,
) -> NumberFormat {
    match format_id {
        Some("14") => NumberFormat::DateMonthDayYear,
        Some("9") => NumberFormat::PercentageZeroDecimals,
        Some("10") => NumberFormat::PercentageTwoDecimals,
        Some("4") => NumberFormat::ThousandsTwoDecimals,
        Some("44") => NumberFormat::DollarAccounting,
        Some(id) => id
            .parse::<usize>()
            .ok()
            .and_then(|id| custom_formats.get(&id))
            .map(|code| {
                let normalized = code.to_ascii_lowercase();
                if normalized.contains('$') && normalized.contains("0.00") {
                    if normalized.contains('*') {
                        NumberFormat::DollarAccounting
                    } else {
                        NumberFormat::DollarTwoDecimals
                    }
                } else {
                    NumberFormat::General
                }
            })
            .unwrap_or(NumberFormat::General),
        None => NumberFormat::General,
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
struct MergeRange {
    start_col: usize,
    end_col: usize,
    start_row: usize,
    end_row: usize,
}

#[derive(Debug, Clone)]
struct SheetImage {
    data: SheetImageData,
    pixel_width: u16,
    pixel_height: u16,
    col: usize,
    row: usize,
    col_offset: f32,
    row_offset: f32,
    width: f32,
    height: f32,
    foreground: bool,
}

#[derive(Debug, Clone)]
enum SheetImageData {
    Jpeg(Vec<u8>),
    Rgba(Vec<u8>),
}

pub(crate) fn convert_xlsx_bytes(input: &[u8], options: &ConversionOptions) -> Result<Vec<u8>> {
    let sheets = read_xlsx_sheets(input)?;
    let mut doc = PdfDocument::new();
    render_xlsx(&mut doc, &sheets, options.page_size);
    Ok(doc.to_bytes())
}

fn read_xlsx_sheets(input: &[u8]) -> Result<Vec<SheetData>> {
    let cursor = Cursor::new(input);
    let mut archive = ZipArchive::new(cursor)?;
    let shared_strings = read_shared_strings(&mut archive)?;
    let styles = read_styles(&mut archive)?;
    let sheet_paths = read_sheet_paths(&mut archive)?;
    let print_title_rows = read_print_title_rows(&mut archive)?;
    let print_areas = read_print_areas(&mut archive)?;
    let mut sheets = Vec::new();

    for (name, path) in sheet_paths {
        let Some(sheet_xml) = read_zip_text(&mut archive, &path)? else {
            continue;
        };
        let mut rows = read_sheet_rows(&sheet_xml, &shared_strings, &styles)?;
        apply_sheet_table_styles(&mut archive, &path, &sheet_xml, &mut rows, &styles)?;
        apply_sheet_conditional_formats(&sheet_xml, &mut rows, &styles)?;
        let mut column_widths = read_column_widths_with_mdw(&sheet_xml, styles.max_digit_width)?;
        let default_row_height = read_default_row_height(&sheet_xml)?;
        let mut images = read_sheet_images(
            &mut archive,
            &path,
            &sheet_xml,
            &column_widths,
            &rows,
            default_row_height,
        )?;
        let mut merges = read_merge_ranges(&sheet_xml)?;
        let mut row_breaks = read_row_breaks(&sheet_xml)?;
        let mut sheet_print_title_rows = print_title_rows.get(&name).copied();
        if let Some(print_area) = print_areas.get(&name).copied() {
            apply_print_area(
                &mut rows,
                &mut images,
                &mut column_widths,
                &mut merges,
                &mut row_breaks,
                &mut sheet_print_title_rows,
                print_area,
            );
        }
        trim_trailing_empty_rows(&mut rows, &images);
        let mut page_setup = read_page_setup(&sheet_xml)?;
        if should_use_printer_page_size(page_setup) {
            if let Some(page_size) = read_printer_page_size(&mut archive, &path, &sheet_xml)? {
                page_setup.page_size = page_size;
                page_setup.page_size_from_printer = true;
            }
        }
        sheets.push(SheetData {
            rows,
            images,
            column_widths,
            merges,
            row_breaks,
            default_row_height,
            print_title_rows: sheet_print_title_rows,
            page_setup,
        });
    }

    if sheets.is_empty() {
        if let Some(sheet_xml) = read_zip_text(&mut archive, "xl/worksheets/sheet1.xml")? {
            let mut rows = read_sheet_rows(&sheet_xml, &shared_strings, &styles)?;
            apply_sheet_table_styles(
                &mut archive,
                "xl/worksheets/sheet1.xml",
                &sheet_xml,
                &mut rows,
                &styles,
            )?;
            apply_sheet_conditional_formats(&sheet_xml, &mut rows, &styles)?;
            let column_widths = read_column_widths_with_mdw(&sheet_xml, styles.max_digit_width)?;
            let default_row_height = read_default_row_height(&sheet_xml)?;
            let images = read_sheet_images(
                &mut archive,
                "xl/worksheets/sheet1.xml",
                &sheet_xml,
                &column_widths,
                &rows,
                default_row_height,
            )?;
            trim_trailing_empty_rows(&mut rows, &images);
            let mut page_setup = read_page_setup(&sheet_xml)?;
            if should_use_printer_page_size(page_setup) {
                if let Some(page_size) =
                    read_printer_page_size(&mut archive, "xl/worksheets/sheet1.xml", &sheet_xml)?
                {
                    page_setup.page_size = page_size;
                    page_setup.page_size_from_printer = true;
                }
            }
            sheets.push(SheetData {
                rows,
                images,
                column_widths,
                merges: read_merge_ranges(&sheet_xml)?,
                row_breaks: read_row_breaks(&sheet_xml)?,
                default_row_height,
                print_title_rows: None,
                page_setup,
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
            row_breaks: Vec::new(),
            default_row_height: ROW_HEIGHT,
            print_title_rows: None,
            page_setup: SheetPageSetup::default(),
        });
    }

    Ok(sheets)
}

fn read_default_row_height(sheet_xml: &str) -> Result<f32> {
    let document = roxmltree::Document::parse(sheet_xml)?;
    Ok(document
        .descendants()
        .find(|node| node.has_tag_name("sheetFormatPr"))
        .and_then(|node| node.attribute("defaultRowHeight"))
        .and_then(|value| value.parse::<f32>().ok())
        .filter(|value| *value > 0.0)
        .unwrap_or(ROW_HEIGHT))
}

fn read_print_title_rows<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
) -> Result<HashMap<String, (usize, usize)>> {
    let Some(workbook_xml) = read_zip_text(archive, "xl/workbook.xml")? else {
        return Ok(HashMap::new());
    };
    let document = roxmltree::Document::parse(&workbook_xml)?;
    let mut titles = HashMap::new();
    for name in document.descendants().filter(|node| {
        node.has_tag_name("definedName") && node.attribute("name") == Some("_xlnm.Print_Titles")
    }) {
        let Some((sheet_name, range)) = name.text().and_then(|value| value.split_once('!')) else {
            continue;
        };
        let Some((start, end)) = range.split(',').find_map(|part| {
            let (start, end) = part.split_once(':')?;
            let start = start.trim().trim_start_matches('$').parse::<usize>().ok()?;
            let end = end.trim().trim_start_matches('$').parse::<usize>().ok()?;
            Some((start.checked_sub(1)?, end.checked_sub(1)?))
        }) else {
            continue;
        };
        titles.insert(
            sheet_name.trim_matches('\'').replace("''", "'"),
            (start, end),
        );
    }
    Ok(titles)
}

fn read_print_areas<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
) -> Result<HashMap<String, MergeRange>> {
    let Some(workbook_xml) = read_zip_text(archive, "xl/workbook.xml")? else {
        return Ok(HashMap::new());
    };
    let document = roxmltree::Document::parse(&workbook_xml)?;
    Ok(document
        .descendants()
        .filter(|node| {
            node.has_tag_name("definedName") && node.attribute("name") == Some("_xlnm.Print_Area")
        })
        .filter_map(|node| node.text().and_then(parse_print_area))
        .collect())
}

fn parse_print_area(value: &str) -> Option<(String, MergeRange)> {
    let (sheet_name, ranges) = value.rsplit_once('!')?;
    let (start, end) = ranges.split(',').next()?.split_once(':')?;
    let (start_col, start_row) = cell_position(start)?;
    let (end_col, end_row) = cell_position(end)?;
    Some((
        sheet_name.trim_matches('\'').replace("''", "'"),
        MergeRange {
            start_col,
            end_col,
            start_row,
            end_row,
        },
    ))
}

#[allow(clippy::too_many_arguments)]
fn apply_print_area(
    rows: &mut Vec<RowData>,
    images: &mut Vec<SheetImage>,
    column_widths: &mut Vec<f32>,
    merges: &mut Vec<MergeRange>,
    row_breaks: &mut Vec<usize>,
    print_title_rows: &mut Option<(usize, usize)>,
    print_area: MergeRange,
) {
    rows.retain_mut(|row| {
        if row.index < print_area.start_row || row.index > print_area.end_row {
            return false;
        }
        row.index -= print_area.start_row;
        row.cells = row
            .cells
            .get(print_area.start_col..=print_area.end_col.min(row.cells.len().saturating_sub(1)))
            .unwrap_or_default()
            .to_vec();
        true
    });
    *column_widths = column_widths
        .get(
            print_area.start_col
                ..=print_area
                    .end_col
                    .min(column_widths.len().saturating_sub(1)),
        )
        .unwrap_or_default()
        .to_vec();
    images.retain_mut(|image| {
        if image.col < print_area.start_col
            || image.col > print_area.end_col
            || image.row < print_area.start_row
            || image.row > print_area.end_row
        {
            return false;
        }
        image.col -= print_area.start_col;
        image.row -= print_area.start_row;
        true
    });
    merges.retain_mut(|merge| {
        if merge.end_col < print_area.start_col
            || merge.start_col > print_area.end_col
            || merge.end_row < print_area.start_row
            || merge.start_row > print_area.end_row
        {
            return false;
        }
        merge.start_col = merge.start_col.max(print_area.start_col) - print_area.start_col;
        merge.end_col = merge.end_col.min(print_area.end_col) - print_area.start_col;
        merge.start_row = merge.start_row.max(print_area.start_row) - print_area.start_row;
        merge.end_row = merge.end_row.min(print_area.end_row) - print_area.start_row;
        true
    });
    row_breaks.retain_mut(|row| {
        if *row <= print_area.start_row || *row > print_area.end_row {
            return false;
        }
        *row -= print_area.start_row;
        true
    });
    *print_title_rows = print_title_rows.and_then(|(start, end)| {
        let start = start.max(print_area.start_row);
        let end = end.min(print_area.end_row);
        (start <= end).then_some((start - print_area.start_row, end - print_area.start_row))
    });
}

fn read_page_setup(sheet_xml: &str) -> Result<SheetPageSetup> {
    let document = roxmltree::Document::parse(sheet_xml)?;
    let page_setup = document
        .descendants()
        .find(|node| node.is_element() && node.tag_name().name() == "pageSetup");
    let mut page_size = match page_setup.and_then(|node| node.attribute("paperSize")) {
        Some("1") => PageSize::LETTER,
        Some("9") | None => PageSize::A4,
        Some(_) => PageSize::A4,
    };
    if page_setup.and_then(|node| node.attribute("orientation")) == Some("landscape") {
        std::mem::swap(&mut page_size.width, &mut page_size.height);
    }
    let page_margins = document
        .descendants()
        .find(|node| node.is_element() && node.tag_name().name() == "pageMargins");
    let margin = |name: &str, fallback: f32| {
        page_margins
            .and_then(|node| node.attribute(name))
            .and_then(|value| value.parse::<f32>().ok())
            .filter(|value| *value >= 0.0)
            .map(|inches| inches * 72.0)
            .unwrap_or(fallback)
    };
    let print_scale = page_setup
        .and_then(|node| node.attribute("scale"))
        .and_then(|value| value.parse::<f32>().ok())
        .filter(|value| *value > 0.0)
        .map(|percent| percent / 100.0)
        .unwrap_or(1.0);
    let fit_to_page = document
        .descendants()
        .find(|node| node.is_element() && node.tag_name().name() == "pageSetUpPr")
        .and_then(|node| node.attribute("fitToPage"))
        == Some("1");
    let fit_to_width = page_setup
        .and_then(|node| node.attribute("fitToWidth"))
        .and_then(|value| value.parse::<usize>().ok())
        .unwrap_or(1);
    let fit_to_height = page_setup
        .and_then(|node| node.attribute("fitToHeight"))
        .and_then(|value| value.parse::<usize>().ok())
        .unwrap_or(1);
    let horizontal_centered = document
        .descendants()
        .find(|node| node.is_element() && node.tag_name().name() == "printOptions")
        .and_then(|node| node.attribute("horizontalCentered"))
        .is_some_and(|value| matches!(value, "1" | "true"));

    let margin_top = margin("top", MARGIN_TOP);
    Ok(SheetPageSetup {
        page_size,
        page_size_from_printer: false,
        margin_left: margin("left", MARGIN_X),
        margin_right: margin("right", MARGIN_X),
        margin_top: if fit_to_page {
            margin_top.max(13.0)
        } else {
            margin_top
        },
        margin_bottom: margin("bottom", MARGIN_BOTTOM),
        print_scale,
        fit_to_width: fit_to_page && fit_to_width > 0,
        fit_to_height: fit_to_page && fit_to_height > 0,
        horizontal_centered,
    })
}

fn read_printer_page_size<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
    sheet_path: &str,
    sheet_xml: &str,
) -> Result<Option<PageSize>> {
    let sheet = roxmltree::Document::parse(sheet_xml)?;
    let Some(page_setup) = sheet
        .descendants()
        .find(|node| node.has_tag_name("pageSetup"))
    else {
        return Ok(None);
    };
    if page_setup.attribute("paperSize").is_some() {
        return Ok(None);
    }
    let Some(settings_id) = relationship_id(page_setup) else {
        return Ok(None);
    };
    let relationships = read_part_relationships(archive, sheet_path)?;
    let Some(target) = relationships.get(&settings_id) else {
        return Ok(None);
    };
    let settings_path = resolve_part_target(sheet_path, target);
    let Some(data) = read_zip_bytes(archive, &settings_path)? else {
        return Ok(None);
    };
    Ok(parse_windows_devmode_page_size(&data))
}

fn should_use_printer_page_size(page_setup: SheetPageSetup) -> bool {
    !page_setup.fit_to_width || page_setup.fit_to_height
}

fn parse_windows_devmode_page_size(data: &[u8]) -> Option<PageSize> {
    let read_u16 = |offset: usize| {
        Some(u16::from_le_bytes([
            *data.get(offset)?,
            *data.get(offset + 1)?,
        ]))
    };
    let size = read_u16(68)? as usize;
    if size < 84 || data.len() < size {
        return None;
    }
    let orientation = read_u16(76)?;
    let paper_size = read_u16(78)?;
    let paper_length = read_u16(80)?;
    let paper_width = read_u16(82)?;
    let mut page_size = match paper_size {
        1 => PageSize::LETTER,
        9 => PageSize::A4,
        _ if paper_width > 0 && paper_length > 0 => PageSize {
            width: paper_width as f32 * 72.0 / 254.0,
            height: paper_length as f32 * 72.0 / 254.0,
        },
        _ => return None,
    };
    if orientation == 2 {
        std::mem::swap(&mut page_size.width, &mut page_size.height);
    }
    Some(page_size)
}

#[cfg(test)]
fn read_column_widths(sheet_xml: &str) -> Result<Vec<f32>> {
    read_column_widths_with_mdw(sheet_xml, 7.0)
}

fn read_column_widths_with_mdw(sheet_xml: &str, max_digit_width: f32) -> Result<Vec<f32>> {
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
                .map(|width| excel_column_width_to_points_with_mdw(width, max_digit_width))
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

fn excel_column_width_to_points_with_mdw(char_units: f32, max_digit_width: f32) -> f32 {
    let padding = (128.0 / max_digit_width).floor();
    (((256.0 * char_units + padding) / 256.0) * max_digit_width).floor() * 0.75
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

fn read_row_breaks(sheet_xml: &str) -> Result<Vec<usize>> {
    let xml = roxmltree::Document::parse(sheet_xml)?;
    Ok(xml
        .descendants()
        .filter(|node| node.has_tag_name("rowBreaks"))
        .flat_map(|row_breaks| row_breaks.children())
        .filter(|node| node.has_tag_name("brk"))
        .filter_map(|node| node.attribute("id"))
        .filter_map(|value| value.parse::<usize>().ok())
        .filter(|row_index| *row_index > 0)
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

fn effective_content_scale(
    unscaled_width: f32,
    usable_width: f32,
    print_scale: f32,
    fit_to_width: bool,
) -> f32 {
    if !fit_to_width || unscaled_width <= 0.0 {
        return print_scale;
    }
    (usable_width / unscaled_width).min(1.0)
}

fn read_sheet_images<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
    sheet_path: &str,
    sheet_xml: &str,
    column_widths: &[f32],
    rows: &[RowData],
    default_row_height: f32,
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
    let theme_colors = read_theme_colors(archive)?;
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
            data: SheetImageData::Jpeg(data),
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
            foreground: false,
        });
    }
    for anchor in drawing
        .descendants()
        .filter(|node| node.has_tag_name("twoCellAnchor"))
    {
        if let Some(image) = read_group_image(archive, &drawing_path, &drawing_rels, anchor)? {
            images.push(image);
        } else if let Some(image) =
            read_two_cell_picture(archive, &drawing_path, &drawing_rels, anchor, &theme_colors)?
        {
            images.push(image);
        } else if let Some(image) = read_two_cell_shape(
            anchor,
            &theme_colors,
            column_widths,
            rows,
            default_row_height,
        ) {
            images.push(image);
        }
    }
    Ok(images)
}

fn read_two_cell_shape(
    anchor: roxmltree::Node<'_, '_>,
    theme_colors: &[PdfColor],
    column_widths: &[f32],
    rows: &[RowData],
    default_row_height: f32,
) -> Option<SheetImage> {
    let from = anchor.children().find(|node| node.has_tag_name("from"))?;
    let shape = anchor.children().find(|node| node.has_tag_name("sp"))?;
    let shape_properties = shape.children().find(|node| node.has_tag_name("spPr"))?;
    let transform = shape_properties
        .children()
        .find(|node| node.has_tag_name("xfrm"))?;
    let extent = transform.children().find(|node| node.has_tag_name("ext"))?;
    let transform_width = emu_attribute(extent, "cx")? / 12_700.0;
    let transform_height = emu_attribute(extent, "cy")? / 12_700.0;
    let (width, height) = anchor
        .children()
        .find(|node| node.has_tag_name("to"))
        .map(|to| {
            let from_x = drawing_column_position(from, column_widths);
            let from_y = drawing_row_position(from, rows, default_row_height);
            let to_x = drawing_column_position(to, column_widths);
            let to_y = drawing_row_position(to, rows, default_row_height);
            (to_x - from_x, to_y - from_y)
        })
        .filter(|(width, height)| *width > 0.0 && *height > 0.0)
        .unwrap_or((transform_width, transform_height));
    if width <= 0.0 || height <= 0.0 {
        return None;
    }
    let fill = shape_properties
        .children()
        .find(|node| node.has_tag_name("solidFill"))
        .and_then(|solid_fill| solid_fill.children().find(|node| node.is_element()))
        .and_then(|color| drawing_color(color, theme_colors))
        .or_else(|| {
            shape
                .children()
                .find(|node| node.has_tag_name("style"))
                .and_then(|style| style.children().find(|node| node.has_tag_name("fillRef")))
                .and_then(|fill_ref| fill_ref.children().find(|node| node.is_element()))
                .and_then(|color| drawing_color(color, theme_colors))
        })?;
    let pixel_width = (width * 2.0).round().clamp(1.0, u16::MAX as f32) as u16;
    let pixel_height = (height * 2.0).round().clamp(1.0, u16::MAX as f32) as u16;
    let fill_pixel = image::Rgba([
        (fill.r * 255.0).round() as u8,
        (fill.g * 255.0).round() as u8,
        (fill.b * 255.0).round() as u8,
        255,
    ]);
    let mut canvas = if shape_properties
        .children()
        .any(|node| node.has_tag_name("prstGeom") && node.attribute("prst") == Some("rect"))
    {
        image::RgbaImage::from_pixel(pixel_width.into(), pixel_height.into(), fill_pixel)
    } else {
        let geometry = shape_properties
            .children()
            .find(|node| node.has_tag_name("custGeom"))?;
        let path = geometry
            .descendants()
            .find(|node| node.has_tag_name("path") && node.attribute("fill") != Some("none"))?;
        let path_width = path.attribute("w")?.parse::<f32>().ok()?;
        let path_height = path.attribute("h")?.parse::<f32>().ok()?;
        if path_width <= 0.0 || path_height <= 0.0 {
            return None;
        }
        let mut points = Vec::new();
        for command in path.children().filter(|node| node.is_element()) {
            if !matches!(command.tag_name().name(), "moveTo" | "lnTo" | "cubicBezTo") {
                continue;
            }
            let Some(point) = command.descendants().rfind(|node| node.has_tag_name("pt")) else {
                continue;
            };
            let x =
                point.attribute("x")?.parse::<f32>().ok()? / path_width * f32::from(pixel_width);
            let y =
                point.attribute("y")?.parse::<f32>().ok()? / path_height * f32::from(pixel_height);
            points.push((x, y));
        }
        if points.len() < 3 {
            return None;
        }
        let mut canvas = image::RgbaImage::new(pixel_width.into(), pixel_height.into());
        draw_filled_polygon(&mut canvas, &points, fill_pixel);
        canvas
    };
    if transform.attribute("rot") == Some("10800000") {
        canvas = image::imageops::rotate180(&canvas);
    }
    Some(SheetImage {
        data: SheetImageData::Rgba(canvas.into_raw()),
        pixel_width,
        pixel_height,
        col: child_number(from, "col").unwrap_or(0),
        row: child_number(from, "row").unwrap_or(0),
        col_offset: child_number(from, "colOff").unwrap_or(0) as f32 / 12_700.0,
        row_offset: child_number(from, "rowOff").unwrap_or(0) as f32 / 12_700.0,
        width,
        height,
        foreground: true,
    })
}

fn drawing_column_position(marker: roxmltree::Node<'_, '_>, column_widths: &[f32]) -> f32 {
    let column = child_number(marker, "col").unwrap_or(0);
    (0..column)
        .map(|index| column_widths.get(index).copied().unwrap_or(COL_WIDTH))
        .sum::<f32>()
        + child_number(marker, "colOff").unwrap_or(0) as f32 / 12_700.0
}

fn drawing_row_position(
    marker: roxmltree::Node<'_, '_>,
    rows: &[RowData],
    default_row_height: f32,
) -> f32 {
    let row = child_number(marker, "row").unwrap_or(0);
    (0..row)
        .map(|index| {
            rows.iter()
                .find(|candidate| candidate.index == index)
                .map(|candidate| candidate.height)
                .unwrap_or(default_row_height)
        })
        .sum::<f32>()
        + child_number(marker, "rowOff").unwrap_or(0) as f32 / 12_700.0
}

fn drawing_color(node: roxmltree::Node<'_, '_>, theme_colors: &[PdfColor]) -> Option<PdfColor> {
    match node.tag_name().name() {
        "schemeClr" => theme_scheme_color(node.attribute("val")?, theme_colors),
        "srgbClr" => parse_rgb_color(node.attribute("val")?),
        "sysClr" => node
            .attribute("lastClr")
            .and_then(parse_rgb_color)
            .or_else(|| node.attribute("val").and_then(parse_rgb_color)),
        _ => None,
    }
}

fn theme_scheme_color(name: &str, theme_colors: &[PdfColor]) -> Option<PdfColor> {
    let index = match name {
        "lt1" | "bg1" => 0,
        "dk1" | "tx1" => 1,
        "lt2" | "bg2" => 2,
        "dk2" | "tx2" => 3,
        "accent1" => 4,
        "accent2" => 5,
        "accent3" => 6,
        "accent4" => 7,
        "accent5" => 8,
        "accent6" => 9,
        "hlink" => 10,
        "folHlink" => 11,
        _ => return None,
    };
    theme_colors.get(index).copied()
}

fn read_two_cell_picture<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
    drawing_path: &str,
    drawing_rels: &HashMap<String, String>,
    anchor: roxmltree::Node<'_, '_>,
    theme_colors: &[PdfColor],
) -> Result<Option<SheetImage>> {
    let Some(from) = anchor.children().find(|node| node.has_tag_name("from")) else {
        return Ok(None);
    };
    let Some(picture) = anchor.children().find(|node| node.has_tag_name("pic")) else {
        return Ok(None);
    };
    let Some(image_id) = picture
        .descendants()
        .find(|node| node.has_tag_name("blip"))
        .and_then(relationship_id)
    else {
        return Ok(None);
    };
    let Some(image_target) = drawing_rels.get(&image_id) else {
        return Ok(None);
    };
    let image_path = resolve_part_target(drawing_path, image_target);
    let Some(data) = read_zip_bytes(archive, &image_path)? else {
        return Ok(None);
    };
    let Ok(source) = image::load_from_memory(&data) else {
        return Ok(None);
    };
    let Some(shape_properties) = picture.children().find(|node| node.has_tag_name("spPr")) else {
        return Ok(None);
    };
    let Some(transform) = shape_properties
        .children()
        .find(|child| child.has_tag_name("xfrm"))
    else {
        return Ok(None);
    };
    let Some(extent) = transform.children().find(|node| node.has_tag_name("ext")) else {
        return Ok(None);
    };
    let width = emu_attribute(extent, "cx").unwrap_or(0.0) / 12_700.0;
    let height = emu_attribute(extent, "cy").unwrap_or(0.0) / 12_700.0;
    if width <= 0.0 || height <= 0.0 {
        return Ok(None);
    }
    let mut rgba = source.into_rgba8();
    if let Some(background) = shape_properties
        .children()
        .find(|node| node.has_tag_name("solidFill"))
        .and_then(|solid_fill| solid_fill.children().find(|node| node.is_element()))
        .and_then(|color| drawing_color(color, theme_colors))
    {
        composite_rgba_background(&mut rgba, background);
    }
    let pixel_width = rgba.width().min(u16::MAX.into()) as u16;
    let pixel_height = rgba.height().min(u16::MAX.into()) as u16;
    Ok(Some(SheetImage {
        data: SheetImageData::Rgba(rgba.into_raw()),
        pixel_width,
        pixel_height,
        col: child_number(from, "col").unwrap_or(0),
        row: child_number(from, "row").unwrap_or(0),
        col_offset: child_number(from, "colOff").unwrap_or(0) as f32 / 12_700.0,
        row_offset: child_number(from, "rowOff").unwrap_or(0) as f32 / 12_700.0,
        width,
        height,
        foreground: true,
    }))
}

fn composite_rgba_background(image: &mut image::RgbaImage, background: PdfColor) {
    let background = [
        (background.r * 255.0).round() as u16,
        (background.g * 255.0).round() as u16,
        (background.b * 255.0).round() as u16,
    ];
    for pixel in image.pixels_mut() {
        let alpha = u16::from(pixel[3]);
        if alpha == 255 {
            continue;
        }
        for channel in 0..3 {
            pixel[channel] =
                ((u16::from(pixel[channel]) * alpha + background[channel] * (255 - alpha) + 127)
                    / 255) as u8;
        }
        pixel[3] = 255;
    }
}

fn read_group_image<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
    drawing_path: &str,
    drawing_rels: &HashMap<String, String>,
    anchor: roxmltree::Node<'_, '_>,
) -> Result<Option<SheetImage>> {
    let Some(from) = anchor.children().find(|node| node.has_tag_name("from")) else {
        return Ok(None);
    };
    let Some(group) = anchor.children().find(|node| node.has_tag_name("grpSp")) else {
        return Ok(None);
    };
    let Some(group_transform) = group
        .children()
        .find(|node| node.has_tag_name("grpSpPr"))
        .and_then(|node| node.children().find(|child| child.has_tag_name("xfrm")))
    else {
        return Ok(None);
    };
    let Some(group_extent) = group_transform
        .children()
        .find(|node| node.has_tag_name("ext"))
    else {
        return Ok(None);
    };
    let width = emu_attribute(group_extent, "cx").unwrap_or(0.0) / 12_700.0;
    let height = emu_attribute(group_extent, "cy").unwrap_or(0.0) / 12_700.0;
    if width <= 0.0 || height <= 0.0 {
        return Ok(None);
    }
    let child_offset = group_transform
        .children()
        .find(|node| node.has_tag_name("chOff"));
    let child_extent = group_transform
        .children()
        .find(|node| node.has_tag_name("chExt"));
    let child_x = child_offset
        .and_then(|node| emu_attribute(node, "x"))
        .unwrap_or(0.0);
    let child_y = child_offset
        .and_then(|node| emu_attribute(node, "y"))
        .unwrap_or(0.0);
    let child_width = child_extent
        .and_then(|node| emu_attribute(node, "cx"))
        .filter(|value| *value > 0.0)
        .unwrap_or(1.0);
    let child_height = child_extent
        .and_then(|node| emu_attribute(node, "cy"))
        .filter(|value| *value > 0.0)
        .unwrap_or(1.0);
    let pixel_width = (width * 2.0).round().clamp(1.0, u16::MAX as f32) as u16;
    let pixel_height = (height * 2.0).round().clamp(1.0, u16::MAX as f32) as u16;
    let mut canvas = image::RgbaImage::new(pixel_width.into(), pixel_height.into());

    for picture in group.children().filter(|node| node.has_tag_name("pic")) {
        let Some(image_id) = picture
            .descendants()
            .find(|node| node.has_tag_name("blip"))
            .and_then(relationship_id)
        else {
            continue;
        };
        let Some(image_target) = drawing_rels.get(&image_id) else {
            continue;
        };
        let image_path = resolve_part_target(drawing_path, image_target);
        let Some(data) = read_zip_bytes(archive, &image_path)? else {
            continue;
        };
        let Ok(source) = image::load_from_memory(&data) else {
            continue;
        };
        let Some(transform) = picture
            .children()
            .find(|node| node.has_tag_name("spPr"))
            .and_then(|node| node.children().find(|child| child.has_tag_name("xfrm")))
        else {
            continue;
        };
        let Some((left, top, image_width, image_height)) = group_child_rect(
            transform,
            child_x,
            child_y,
            child_width,
            child_height,
            pixel_width,
            pixel_height,
        ) else {
            continue;
        };
        let image_width = (image_width as f32 * SVG_FALLBACK_HORIZONTAL_SCALE)
            .round()
            .max(1.0) as u32;
        let resized = source.resize_exact(
            image_width,
            image_height,
            image::imageops::FilterType::Lanczos3,
        );
        image::imageops::overlay(&mut canvas, &resized, left.into(), top.into());
    }

    for shape in group.children().filter(|node| node.has_tag_name("sp")) {
        let Some(transform) = shape
            .children()
            .find(|node| node.has_tag_name("spPr"))
            .and_then(|node| node.children().find(|child| child.has_tag_name("xfrm")))
        else {
            continue;
        };
        let Some((left, top, shape_width, shape_height)) = group_child_rect(
            transform,
            child_x,
            child_y,
            child_width,
            child_height,
            pixel_width,
            pixel_height,
        ) else {
            continue;
        };
        let fill = shape
            .descendants()
            .find(|node| node.has_tag_name("srgbClr"))
            .and_then(|node| node.attribute("val"))
            .and_then(parse_rgb_color)
            .unwrap_or(PdfColor::BLACK);
        let alpha = shape
            .descendants()
            .find(|node| node.has_tag_name("alpha"))
            .and_then(|node| node.attribute("val"))
            .and_then(|value| value.parse::<f32>().ok())
            .map(|value| (value * 255.0 / 100_000.0).round() as u8)
            .unwrap_or(255);
        draw_lower_half_ellipse(
            &mut canvas,
            left,
            top,
            shape_width,
            shape_height,
            image::Rgba([
                (fill.r * 255.0).round() as u8,
                (fill.g * 255.0).round() as u8,
                (fill.b * 255.0).round() as u8,
                alpha,
            ]),
        );
    }

    Ok(Some(SheetImage {
        data: SheetImageData::Rgba(canvas.into_raw()),
        pixel_width,
        pixel_height,
        col: child_number(from, "col").unwrap_or(0),
        row: child_number(from, "row").unwrap_or(0),
        col_offset: child_number(from, "colOff").unwrap_or(0) as f32 / 12_700.0,
        row_offset: child_number(from, "rowOff").unwrap_or(0) as f32 / 12_700.0,
        width,
        height,
        foreground: true,
    }))
}

fn emu_attribute(node: roxmltree::Node<'_, '_>, name: &str) -> Option<f32> {
    node.attribute(name)?.parse().ok()
}

fn group_child_rect(
    transform: roxmltree::Node<'_, '_>,
    group_x: f32,
    group_y: f32,
    group_width: f32,
    group_height: f32,
    pixel_width: u16,
    pixel_height: u16,
) -> Option<(u32, u32, u32, u32)> {
    let offset = transform.children().find(|node| node.has_tag_name("off"))?;
    let extent = transform.children().find(|node| node.has_tag_name("ext"))?;
    let left = ((emu_attribute(offset, "x")? - group_x) / group_width * f32::from(pixel_width))
        .round()
        .max(0.0) as u32;
    let top = ((emu_attribute(offset, "y")? - group_y) / group_height * f32::from(pixel_height))
        .round()
        .max(0.0) as u32;
    let width = (emu_attribute(extent, "cx")? / group_width * f32::from(pixel_width))
        .round()
        .max(1.0) as u32;
    let height = (emu_attribute(extent, "cy")? / group_height * f32::from(pixel_height))
        .round()
        .max(1.0) as u32;
    Some((left, top, width, height))
}

fn draw_lower_half_ellipse(
    canvas: &mut image::RgbaImage,
    left: u32,
    top: u32,
    width: u32,
    height: u32,
    color: image::Rgba<u8>,
) {
    let radius_x = width as f32 / 2.0;
    let radius_y = height as f32;
    for y in 0..height {
        for x in 0..width {
            let normalized_x = (x as f32 + 0.5 - radius_x) / radius_x;
            let normalized_y = (y as f32 + 0.5) / radius_y;
            if normalized_x * normalized_x + normalized_y * normalized_y <= 1.0
                && left + x < canvas.width()
                && top + y < canvas.height()
            {
                canvas.put_pixel(left + x, top + y, color);
            }
        }
    }
}

fn draw_filled_polygon(
    canvas: &mut image::RgbaImage,
    points: &[(f32, f32)],
    color: image::Rgba<u8>,
) {
    for y in 0..canvas.height() {
        let scan_y = y as f32 + 0.5;
        let mut intersections = Vec::new();
        for index in 0..points.len() {
            let (x1, y1) = points[index];
            let (x2, y2) = points[(index + 1) % points.len()];
            if (y1 <= scan_y && scan_y < y2) || (y2 <= scan_y && scan_y < y1) {
                intersections.push(x1 + (scan_y - y1) * (x2 - x1) / (y2 - y1));
            }
        }
        intersections.sort_by(f32::total_cmp);
        for pair in intersections.chunks_exact(2) {
            let start = pair[0].ceil().max(0.0) as u32;
            let end = pair[1].floor().min(canvas.width() as f32 - 1.0) as u32;
            for x in start..=end {
                canvas.put_pixel(x, y, color);
            }
        }
    }
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
    let theme_colors = read_theme_colors(archive)?;
    let xml = roxmltree::Document::parse(&styles_xml)?;
    let custom_formats = xml
        .descendants()
        .filter(|node| node.has_tag_name("numFmt"))
        .filter_map(|node| {
            Some((
                node.attribute("numFmtId")?.parse::<usize>().ok()?,
                node.attribute("formatCode")?.to_owned(),
            ))
        })
        .collect::<HashMap<_, _>>();
    let fonts = xml
        .descendants()
        .find(|node| node.has_tag_name("fonts"))
        .map(|fonts| {
            fonts
                .children()
                .filter(|node| node.has_tag_name("font"))
                .map(|font| {
                    let font_name = font
                        .children()
                        .find(|node| node.has_tag_name("name"))
                        .and_then(|node| node.attribute("val"));
                    let font_size = font
                        .children()
                        .find(|node| node.has_tag_name("sz"))
                        .and_then(|node| node.attribute("val"))
                        .and_then(|value| value.parse::<f32>().ok())
                        .unwrap_or(CELL_FONT_SIZE);
                    FontStyle {
                        bold: font.children().any(|node| {
                            node.has_tag_name("b") && node.attribute("val") != Some("0")
                        }) || font_name_is_emphasis(font_name),
                        italic: font.children().any(|node| {
                            node.has_tag_name("i") && node.attribute("val") != Some("0")
                        }),
                        size: excel_pdf_font_size(font_name, font_size),
                        color: font
                            .children()
                            .find(|node| node.has_tag_name("color"))
                            .and_then(|node| parse_xlsx_color(node, &theme_colors))
                            .unwrap_or(PdfColor::BLACK),
                        preferred_font: xlsx_preferred_font(font_name),
                    }
                })
                .collect::<Vec<_>>()
        })
        .unwrap_or_default();
    let max_digit_width = xml
        .descendants()
        .find(|node| node.has_tag_name("fonts"))
        .and_then(|fonts| fonts.children().find(|node| node.has_tag_name("font")))
        .map(|font| {
            let name = font
                .children()
                .find(|node| node.has_tag_name("name"))
                .and_then(|node| node.attribute("val"));
            let size = font
                .children()
                .find(|node| node.has_tag_name("sz"))
                .and_then(|node| node.attribute("val"))
                .and_then(|value| value.parse::<f32>().ok())
                .unwrap_or(CELL_FONT_SIZE);
            excel_max_digit_width(name, size)
        })
        .unwrap_or(7.0);
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
                        .and_then(|color| parse_xlsx_color(color, &theme_colors))
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
                .map(|border| parse_cell_borders(border, &theme_colors))
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
                    let fill_override = xf
                        .attribute("fillId")
                        .and_then(|value| value.parse::<usize>().ok())
                        .is_some_and(|fill_id| fill_id != 0)
                        || xf
                            .attribute("applyFill")
                            .is_some_and(|value| matches!(value, "1" | "true"));
                    let cell_borders = xf
                        .attribute("borderId")
                        .and_then(|value| value.parse::<usize>().ok())
                        .and_then(|border_id| borders.get(border_id).copied())
                        .unwrap_or_default();
                    let alignment = xf.children().find(|node| node.has_tag_name("alignment"));
                    CellStyle {
                        bold: font.bold,
                        italic: font.italic,
                        strike: false,
                        font_size: font.size,
                        font_color: font.color,
                        fill_color,
                        fill_override,
                        borders: cell_borders,
                        number_format: parse_number_format(
                            xf.attribute("numFmtId"),
                            &custom_formats,
                        ),
                        horizontal_alignment: parse_horizontal_alignment(
                            alignment.and_then(|node| node.attribute("horizontal")),
                        ),
                        vertical_alignment: parse_vertical_alignment(
                            alignment.and_then(|node| node.attribute("vertical")),
                        ),
                        indent: alignment
                            .and_then(|node| node.attribute("indent"))
                            .and_then(|value| value.parse::<f32>().ok())
                            .filter(|value| *value > 0.0)
                            .unwrap_or(0.0),
                        wrap_text: alignment
                            .and_then(|node| node.attribute("wrapText"))
                            .is_some_and(|value| matches!(value, "1" | "true")),
                        preferred_font: font.preferred_font,
                    }
                })
                .collect::<Vec<_>>()
        })
        .unwrap_or_default();
    let differential_fills = xml
        .descendants()
        .find(|node| node.has_tag_name("dxfs"))
        .map(|dxfs| {
            dxfs.children()
                .filter(|node| node.has_tag_name("dxf"))
                .map(|dxf| {
                    dxf.descendants()
                        .find(|node| node.has_tag_name("patternFill"))
                        .and_then(|pattern| {
                            pattern
                                .children()
                                .find(|node| node.has_tag_name("fgColor"))
                                .or_else(|| {
                                    pattern.children().find(|node| node.has_tag_name("bgColor"))
                                })
                        })
                        .and_then(|color| parse_xlsx_color(color, &theme_colors))
                })
                .collect::<Vec<_>>()
        })
        .unwrap_or_default();
    let differential_fonts = xml
        .descendants()
        .find(|node| node.has_tag_name("dxfs"))
        .map(|dxfs| {
            dxfs.children()
                .filter(|node| node.has_tag_name("dxf"))
                .map(|dxf| {
                    let Some(font) = dxf.children().find(|node| node.has_tag_name("font")) else {
                        return DifferentialFontStyle::default();
                    };
                    DifferentialFontStyle {
                        bold: font
                            .children()
                            .find(|node| node.has_tag_name("b"))
                            .map(|node| node.attribute("val") != Some("0")),
                        italic: font
                            .children()
                            .find(|node| node.has_tag_name("i"))
                            .map(|node| node.attribute("val") != Some("0")),
                        strike: font
                            .children()
                            .find(|node| node.has_tag_name("strike"))
                            .map(|node| node.attribute("val") != Some("0")),
                        color: font
                            .children()
                            .find(|node| node.has_tag_name("color"))
                            .and_then(|color| parse_xlsx_color(color, &theme_colors)),
                    }
                })
                .collect::<Vec<_>>()
        })
        .unwrap_or_default();
    let differential_borders = xml
        .descendants()
        .find(|node| node.has_tag_name("dxfs"))
        .map(|dxfs| {
            dxfs.children()
                .filter(|node| node.has_tag_name("dxf"))
                .map(|dxf| {
                    dxf.children()
                        .find(|node| node.has_tag_name("border"))
                        .map(|border| parse_table_borders(border, &theme_colors))
                })
                .collect::<Vec<_>>()
        })
        .unwrap_or_default();
    let table_base_fills = xml
        .descendants()
        .filter(|node| node.has_tag_name("tableStyle"))
        .filter_map(|style| {
            let name = style.attribute("name")?;
            let whole_table = style.children().find(|node| {
                node.has_tag_name("tableStyleElement")
                    && node.attribute("type") == Some("wholeTable")
            })?;
            let dxf_id = whole_table.attribute("dxfId")?.parse::<usize>().ok()?;
            let fill = differential_fills.get(dxf_id).copied().flatten()?;
            Some((name.to_owned(), fill))
        })
        .collect();
    let table_first_row_stripes = xml
        .descendants()
        .filter(|node| node.has_tag_name("tableStyle"))
        .filter_map(|style| {
            let name = style.attribute("name")?;
            let stripe = style.children().find(|node| {
                node.has_tag_name("tableStyleElement")
                    && node.attribute("type") == Some("firstRowStripe")
            })?;
            let dxf_id = stripe.attribute("dxfId")?.parse::<usize>().ok()?;
            let fill = differential_fills.get(dxf_id).copied().flatten()?;
            let size = stripe
                .attribute("size")
                .and_then(|value| value.parse::<usize>().ok())
                .filter(|value| *value > 0)
                .unwrap_or(1);
            Some((name.to_owned(), (fill, size)))
        })
        .collect();
    let collect_table_fills = |element_type: &str| {
        xml.descendants()
            .filter(|node| node.has_tag_name("tableStyle"))
            .filter_map(|style| {
                let name = style.attribute("name")?;
                let element = style.children().find(|node| {
                    node.has_tag_name("tableStyleElement")
                        && node.attribute("type") == Some(element_type)
                })?;
                let dxf_id = element.attribute("dxfId")?.parse::<usize>().ok()?;
                let fill = differential_fills.get(dxf_id).copied().flatten()?;
                Some((name.to_owned(), fill))
            })
            .collect::<HashMap<_, _>>()
    };
    let table_header_fills = collect_table_fills("headerRow");
    let table_total_fills = collect_table_fills("totalRow");
    let collect_table_fonts = |element_type: &str| {
        xml.descendants()
            .filter(|node| node.has_tag_name("tableStyle"))
            .filter_map(|style| {
                let name = style.attribute("name")?;
                let element = style.children().find(|node| {
                    node.has_tag_name("tableStyleElement")
                        && node.attribute("type") == Some(element_type)
                })?;
                let dxf_id = element.attribute("dxfId")?.parse::<usize>().ok()?;
                let font = differential_fonts.get(dxf_id).copied()?;
                font.any().then(|| (name.to_owned(), font))
            })
            .collect::<HashMap<_, _>>()
    };
    let table_whole_fonts = collect_table_fonts("wholeTable");
    let table_header_fonts = collect_table_fonts("headerRow");
    let table_total_fonts = collect_table_fonts("totalRow");
    let collect_table_borders = |element_type: &str| {
        xml.descendants()
            .filter(|node| node.has_tag_name("tableStyle"))
            .filter_map(|style| {
                let name = style.attribute("name")?;
                let element = style.children().find(|node| {
                    node.has_tag_name("tableStyleElement")
                        && node.attribute("type") == Some(element_type)
                })?;
                let dxf_id = element.attribute("dxfId")?.parse::<usize>().ok()?;
                let borders = differential_borders.get(dxf_id).copied().flatten()?;
                Some((name.to_owned(), borders))
            })
            .collect::<HashMap<_, _>>()
    };
    let table_whole_borders = collect_table_borders("wholeTable");
    let table_header_borders = collect_table_borders("headerRow");
    let table_total_borders = collect_table_borders("totalRow");
    Ok(XlsxStyles {
        cells,
        differential_fonts,
        differential_fills,
        max_digit_width,
        table_base_fills,
        table_first_row_stripes,
        table_header_fills,
        table_total_fills,
        table_whole_fonts,
        table_header_fonts,
        table_total_fonts,
        table_whole_borders,
        table_header_borders,
        table_total_borders,
    })
}

fn apply_sheet_conditional_formats(
    sheet_xml: &str,
    rows: &mut [RowData],
    styles: &XlsxStyles,
) -> Result<()> {
    let sheet = roxmltree::Document::parse(sheet_xml)?;
    for formatting in sheet
        .descendants()
        .filter(|node| node.has_tag_name("conditionalFormatting"))
    {
        let ranges = formatting.attribute("sqref").unwrap_or_default();
        for rule in formatting.children().filter(|node| {
            node.has_tag_name("cfRule")
                && node.attribute("type") == Some("cellIs")
                && node.attribute("operator") == Some("equal")
        }) {
            let Some(color) = rule
                .attribute("dxfId")
                .and_then(|value| value.parse::<usize>().ok())
                .and_then(|index| {
                    styles
                        .differential_fonts
                        .get(index)
                        .and_then(|style| style.color)
                })
            else {
                continue;
            };
            let Some(expected) = rule
                .children()
                .find(|node| node.has_tag_name("formula"))
                .and_then(|node| node.text())
                .map(|value| value.trim_matches('"'))
            else {
                continue;
            };
            for range in ranges.split_whitespace() {
                let (start, end) = range.split_once(':').unwrap_or((range, range));
                let Some((start_col, start_row)) = cell_position(start) else {
                    continue;
                };
                let Some((end_col, end_row)) = cell_position(end) else {
                    continue;
                };
                for row in rows
                    .iter_mut()
                    .filter(|row| row.index >= start_row && row.index <= end_row)
                {
                    for cell in row
                        .cells
                        .iter_mut()
                        .enumerate()
                        .filter(|(column, _)| *column >= start_col && *column <= end_col)
                        .map(|(_, cell)| cell)
                    {
                        if cell.text == expected {
                            cell.style.font_color = color;
                        }
                    }
                }
            }
        }
        for rule in formatting.children().filter(|node| {
            node.has_tag_name("cfRule") && node.attribute("type") == Some("expression")
        }) {
            let Some(formula) = rule
                .children()
                .find(|node| node.has_tag_name("formula"))
                .and_then(|node| node.text())
            else {
                continue;
            };
            if let Some((reference_col, reference_row, expected)) =
                parse_conditional_text_search(formula)
            {
                let differential_index = rule
                    .attribute("dxfId")
                    .and_then(|value| value.parse::<usize>().ok());
                let fill = differential_index
                    .and_then(|index| styles.differential_fills.get(index).copied().flatten());
                let matches = rows
                    .iter()
                    .find(|row| row.index == reference_row)
                    .and_then(|row| row.cells.get(reference_col))
                    .is_some_and(|cell| {
                        cell.text
                            .to_ascii_lowercase()
                            .contains(&expected.to_ascii_lowercase())
                    });
                if matches {
                    for range in ranges.split_whitespace() {
                        let (start, end) = range.split_once(':').unwrap_or((range, range));
                        let Some((start_col, start_row)) = cell_position(start) else {
                            continue;
                        };
                        let Some((end_col, end_row)) = cell_position(end) else {
                            continue;
                        };
                        for row in rows
                            .iter_mut()
                            .filter(|row| row.index >= start_row && row.index <= end_row)
                        {
                            if row.cells.len() <= end_col {
                                row.cells.resize(end_col + 1, CellData::default());
                            }
                            if let Some(fill) = fill {
                                for cell in &mut row.cells[start_col..=end_col] {
                                    cell.style.fill_color = Some(fill);
                                }
                            }
                        }
                    }
                }
                continue;
            }
            if let Some((reference_col, reference_row, row_absolute, expected)) =
                parse_conditional_text_equality(formula)
            {
                let differential_index = rule
                    .attribute("dxfId")
                    .and_then(|value| value.parse::<usize>().ok());
                let font = differential_index
                    .and_then(|index| styles.differential_fonts.get(index).copied())
                    .unwrap_or_default();
                let fill = differential_index
                    .and_then(|index| styles.differential_fills.get(index).copied().flatten());
                for range in ranges.split_whitespace() {
                    let (start, end) = range.split_once(':').unwrap_or((range, range));
                    let Some((start_col, start_row)) = cell_position(start) else {
                        continue;
                    };
                    let Some((end_col, end_row)) = cell_position(end) else {
                        continue;
                    };
                    for row_position in 0..rows.len() {
                        let target_row = rows[row_position].index;
                        if target_row < start_row || target_row > end_row {
                            continue;
                        }
                        let source_row = if row_absolute {
                            reference_row
                        } else {
                            reference_row + target_row - start_row
                        };
                        let matches = rows
                            .iter()
                            .find(|candidate| candidate.index == source_row)
                            .and_then(|source| source.cells.get(reference_col))
                            .is_some_and(|cell| cell.text.eq_ignore_ascii_case(&expected));
                        if !matches {
                            continue;
                        }
                        let row = &mut rows[row_position];
                        if row.cells.len() <= end_col {
                            row.cells.resize(end_col + 1, CellData::default());
                        }
                        for cell in &mut row.cells[start_col..=end_col] {
                            if let Some(bold) = font.bold {
                                cell.style.bold = bold;
                            }
                            if let Some(italic) = font.italic {
                                cell.style.italic = italic;
                            }
                            if let Some(strike) = font.strike {
                                cell.style.strike = strike;
                            }
                            if let Some(color) = font.color {
                                cell.style.font_color = color;
                            }
                            if let Some(fill) = fill {
                                cell.style.fill_color = Some(fill);
                            }
                        }
                    }
                }
                continue;
            }
            if formula.replace(' ', "") != "StartDate+0=TODAY()" {
                continue;
            }
            let Some(fill) = rule
                .attribute("dxfId")
                .and_then(|value| value.parse::<usize>().ok())
                .and_then(|index| styles.differential_fills.get(index).copied().flatten())
            else {
                continue;
            };
            for range in ranges.split_whitespace() {
                let (start, end) = range.split_once(':').unwrap_or((range, range));
                let Some((start_col, start_row)) = cell_position(start) else {
                    continue;
                };
                let Some((end_col, end_row)) = cell_position(end) else {
                    continue;
                };
                for row in rows
                    .iter_mut()
                    .filter(|row| row.index >= start_row && row.index <= end_row)
                {
                    if row.cells.len() <= end_col {
                        row.cells.resize(end_col + 1, CellData::default());
                    }
                    for cell in &mut row.cells[start_col..=end_col] {
                        cell.style.fill_color = Some(fill);
                    }
                }
            }
        }
    }
    Ok(())
}

fn parse_conditional_text_equality(formula: &str) -> Option<(usize, usize, bool, String)> {
    let (reference, expected) = formula.split_once('=')?;
    let reference = reference.trim();
    let expected = expected
        .trim()
        .strip_prefix('"')?
        .strip_suffix('"')?
        .to_owned();
    let row_absolute = reference
        .trim_start_matches('$')
        .chars()
        .find(|ch| !ch.is_ascii_alphabetic())
        == Some('$');
    let (column, row) = cell_position(reference)?;
    Some((column, row, row_absolute, expected))
}

fn parse_conditional_text_search(formula: &str) -> Option<(usize, usize, String)> {
    const PREFIX: &str = "ISNUMBER(SEARCH(\"";
    const SUFFIX: &str = "))=TRUE";
    let compact = formula.replace(' ', "");
    let upper = compact.to_ascii_uppercase();
    if !upper.starts_with(PREFIX) || !upper.ends_with(SUFFIX) {
        return None;
    }
    let body = &compact[PREFIX.len()..compact.len() - SUFFIX.len()];
    let (expected, reference) = body.split_once("\",")?;
    let (column, row) = cell_position(reference)?;
    Some((column, row, expected.to_owned()))
}

fn apply_sheet_table_styles<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
    sheet_path: &str,
    sheet_xml: &str,
    rows: &mut [RowData],
    styles: &XlsxStyles,
) -> Result<()> {
    let sheet = roxmltree::Document::parse(sheet_xml)?;
    let relationships = read_part_relationships(archive, sheet_path)?;
    for table_id in sheet
        .descendants()
        .filter(|node| node.has_tag_name("tablePart"))
        .filter_map(relationship_id)
    {
        let Some(target) = relationships.get(&table_id) else {
            continue;
        };
        let table_path = resolve_part_target(sheet_path, target);
        let Some(table_xml) = read_zip_text(archive, &table_path)? else {
            continue;
        };
        let table = roxmltree::Document::parse(&table_xml)?;
        let root = table.root_element();
        let Some((start, end)) = root
            .attribute("ref")
            .and_then(|range| range.split_once(':'))
        else {
            continue;
        };
        let Some((start_col, start_row)) = cell_position(start) else {
            continue;
        };
        let Some((end_col, end_row)) = cell_position(end) else {
            continue;
        };
        let Some(style_info) = root
            .children()
            .find(|node| node.has_tag_name("tableStyleInfo"))
        else {
            continue;
        };
        let header_rows = root
            .attribute("headerRowCount")
            .and_then(|value| value.parse::<usize>().ok())
            .unwrap_or(1);
        let data_start = start_row + header_rows;
        let total_rows = root
            .attribute("totalsRowCount")
            .and_then(|value| value.parse::<usize>().ok())
            .unwrap_or(0);
        let direct_font = |attribute: &str| {
            root.attribute(attribute)
                .and_then(|value| value.parse::<usize>().ok())
                .and_then(|index| styles.differential_fonts.get(index).copied())
                .filter(|font| font.any())
        };
        let direct_header_font = direct_font("headerRowDxfId");
        let direct_data_font = direct_font("dataDxfId");
        let direct_total_font = direct_font("totalsRowDxfId");
        let style_name = style_info.attribute("name").unwrap_or_default();
        let total_start = (end_row + 1).saturating_sub(total_rows).max(start_row);
        let data_end = total_start.saturating_sub(1);
        if data_start <= data_end {
            if let Some(font) = styles.table_whole_fonts.get(style_name).copied() {
                apply_table_font_layer(rows, start_col, end_col, data_start, data_end, font);
            }
            if let Some(font) = direct_data_font {
                apply_table_font_layer(rows, start_col, end_col, data_start, data_end, font);
            }
        }
        if header_rows > 0 {
            let header_end = (start_row + header_rows - 1).min(end_row);
            if let Some(fill) = styles.table_header_fills.get(style_name).copied() {
                apply_table_fill_layer(rows, start_col, end_col, start_row, header_end, fill);
            }
            if let Some(font) = direct_header_font {
                apply_table_font_layer(rows, start_col, end_col, start_row, header_end, font);
            }
            if let Some(font) = styles.table_header_fonts.get(style_name).copied() {
                apply_table_font_layer(rows, start_col, end_col, start_row, header_end, font);
            }
        }
        if total_rows > 0 {
            if let Some(fill) = styles.table_total_fills.get(style_name).copied() {
                apply_table_fill_layer(rows, start_col, end_col, total_start, end_row, fill);
            }
            if let Some(font) = styles.table_total_fonts.get(style_name).copied() {
                apply_table_font_layer(rows, start_col, end_col, total_start, end_row, font);
            }
            if let Some(font) = direct_total_font {
                apply_table_font_layer(rows, start_col, end_col, total_start, end_row, font);
            }
        }
        if let Some(borders) = styles.table_whole_borders.get(style_name).copied() {
            apply_table_border_layer(rows, start_col, end_col, start_row, end_row, borders);
        }
        if header_rows > 0 {
            if let Some(borders) = styles.table_header_borders.get(style_name).copied() {
                apply_table_border_layer(
                    rows,
                    start_col,
                    end_col,
                    start_row,
                    (start_row + header_rows - 1).min(end_row),
                    borders,
                );
            }
        }
        if total_rows > 0 {
            if let Some(borders) = styles.table_total_borders.get(style_name).copied() {
                apply_table_border_layer(
                    rows,
                    start_col,
                    end_col,
                    (end_row + 1).saturating_sub(total_rows).max(start_row),
                    end_row,
                    borders,
                );
            }
        }
        let base_fill = styles.table_base_fills.get(style_name).copied();
        let stripe = style_info
            .attribute("showRowStripes")
            .is_some_and(|value| matches!(value, "1" | "true"))
            .then(|| styles.table_first_row_stripes.get(style_name).copied())
            .flatten();
        for row in rows
            .iter_mut()
            .filter(|row| row.index >= data_start && row.index <= end_row)
        {
            if row.cells.len() <= end_col {
                row.cells.resize(end_col + 1, CellData::default());
            }
            let fill = stripe
                .filter(|(_, size)| ((row.index - data_start) / size) % 2 == 0)
                .map(|(fill, _)| fill)
                .or(base_fill);
            let Some(fill) = fill else {
                continue;
            };
            for cell in &mut row.cells[start_col..=end_col] {
                cell.style.fill_color = Some(fill);
            }
        }
    }
    Ok(())
}

fn apply_table_fill_layer(
    rows: &mut [RowData],
    start_col: usize,
    end_col: usize,
    start_row: usize,
    end_row: usize,
    fill: PdfColor,
) {
    for row in rows
        .iter_mut()
        .filter(|row| row.index >= start_row && row.index <= end_row)
    {
        if row.cells.len() <= end_col {
            row.cells.resize(end_col + 1, CellData::default());
        }
        for cell in &mut row.cells[start_col..=end_col] {
            cell.style.fill_color = Some(fill);
        }
    }
}

fn apply_table_font_layer(
    rows: &mut [RowData],
    start_col: usize,
    end_col: usize,
    start_row: usize,
    end_row: usize,
    font: DifferentialFontStyle,
) {
    for row in rows
        .iter_mut()
        .filter(|row| row.index >= start_row && row.index <= end_row)
    {
        if row.cells.len() <= end_col {
            row.cells.resize(end_col + 1, CellData::default());
        }
        for cell in &mut row.cells[start_col..=end_col] {
            if let Some(bold) = font.bold {
                cell.style.bold = bold;
            }
            if let Some(italic) = font.italic {
                cell.style.italic = italic;
            }
            if let Some(strike) = font.strike {
                cell.style.strike = strike;
            }
            if let Some(color) = font.color {
                cell.style.font_color = color;
            }
        }
    }
}

fn apply_table_border_layer(
    rows: &mut [RowData],
    start_col: usize,
    end_col: usize,
    start_row: usize,
    end_row: usize,
    borders: TableBorders,
) {
    for row in rows
        .iter_mut()
        .filter(|row| row.index >= start_row && row.index <= end_row)
    {
        if row.cells.len() <= end_col {
            row.cells.resize(end_col + 1, CellData::default());
        }
        for column in start_col..=end_col {
            let cell_borders = &mut row.cells[column].style.borders;
            if column == start_col {
                cell_borders.left = borders.edges.left.or(cell_borders.left);
                if borders.edges.left.is_some() {
                    cell_borders.left_width = borders.edges.left_width;
                }
            } else {
                if let Some((color, width)) = borders.vertical {
                    cell_borders.left = Some(color);
                    cell_borders.left_width = width;
                }
            }
            if column == end_col {
                cell_borders.right = borders.edges.right.or(cell_borders.right);
                if borders.edges.right.is_some() {
                    cell_borders.right_width = borders.edges.right_width;
                }
            }
            if row.index == start_row {
                cell_borders.top = borders.edges.top.or(cell_borders.top);
                if borders.edges.top.is_some() {
                    cell_borders.top_width = borders.edges.top_width;
                }
            } else {
                if let Some((color, width)) = borders.horizontal {
                    cell_borders.top = Some(color);
                    cell_borders.top_width = width;
                }
            }
            if row.index == end_row {
                cell_borders.bottom = borders.edges.bottom.or(cell_borders.bottom);
                if borders.edges.bottom.is_some() {
                    cell_borders.bottom_width = borders.edges.bottom_width;
                }
            }
        }
    }
}

fn excel_pdf_font_size(font_name: Option<&str>, size: f32) -> f32 {
    match font_name.unwrap_or_default().to_ascii_lowercase().as_str() {
        "corbel" => size * 0.962,
        "franklin gothic medium" if size >= 30.0 => size * 0.984,
        "franklin gothic medium" => size * 0.93,
        "grandview" => size * 0.956,
        "grandview display" if size >= 30.0 => size * 0.947,
        "grandview display" => size * 0.956,
        "palatino linotype" => size * 0.967,
        "verdana" => size * 0.96,
        _ => size,
    }
}

fn xlsx_preferred_font(font_name: Option<&str>) -> Option<&'static str> {
    match font_name.unwrap_or_default().to_ascii_lowercase().as_str() {
        "corbel" => Some("corbel"),
        "franklin gothic medium" => Some("framd"),
        "garamond" => Some("gara"),
        "grandview" => Some("grandview"),
        "grandview display" => Some("grandviewdisplay"),
        "palatino linotype" => Some("bookos"),
        "tw cen mt" => Some("tcm_____"),
        "verdana" => Some("verdana"),
        _ => None,
    }
}

fn excel_max_digit_width(font_name: Option<&str>, size: f32) -> f32 {
    let width_at_ten_points = match font_name.unwrap_or_default().to_ascii_lowercase().as_str() {
        "arial" => 7.06,
        "palatino linotype" => 6.42,
        "verdana" => 7.55,
        _ => 7.0,
    };
    width_at_ten_points * size / 10.0
}

fn read_theme_colors<R: std::io::Read + std::io::Seek>(
    archive: &mut ZipArchive<R>,
) -> Result<Vec<PdfColor>> {
    let Some(theme_xml) = read_zip_text(archive, "xl/theme/theme1.xml")? else {
        return Ok(Vec::new());
    };
    let document = roxmltree::Document::parse(&theme_xml)?;
    let scheme_colors = document
        .descendants()
        .find(|node| node.has_tag_name("clrScheme"))
        .map(|scheme| {
            scheme
                .children()
                .filter(|node| node.is_element())
                .filter_map(|entry| {
                    let color = entry.children().find(|node| node.is_element())?;
                    color
                        .attribute("val")
                        .and_then(parse_rgb_color)
                        .or_else(|| color.attribute("lastClr").and_then(parse_rgb_color))
                })
                .collect::<Vec<_>>()
        })
        .unwrap_or_default();
    Ok(spreadsheet_theme_colors(&scheme_colors))
}

fn spreadsheet_theme_colors(scheme_colors: &[PdfColor]) -> Vec<PdfColor> {
    const INDEX_ORDER: [usize; 12] = [1, 0, 3, 2, 4, 5, 6, 7, 8, 9, 10, 11];
    INDEX_ORDER
        .into_iter()
        .filter_map(|index| scheme_colors.get(index).copied())
        .collect()
}

fn parse_xlsx_color(node: roxmltree::Node<'_, '_>, theme_colors: &[PdfColor]) -> Option<PdfColor> {
    let color = node
        .attribute("rgb")
        .and_then(parse_rgb_color)
        .or_else(|| {
            node.attribute("theme")
                .and_then(|value| value.parse::<usize>().ok())
                .and_then(|index| theme_colors.get(index).copied())
        })
        .or_else(|| {
            node.attribute("indexed")
                .and_then(|value| value.parse::<usize>().ok())
                .and_then(indexed_color)
        })?;
    let tint = node
        .attribute("tint")
        .and_then(|value| value.parse::<f32>().ok())
        .unwrap_or(0.0)
        .clamp(-1.0, 1.0);
    Some(apply_color_tint(color, tint))
}

fn indexed_color(index: usize) -> Option<PdfColor> {
    match index % 8 {
        0 => Some(PdfColor::BLACK),
        1 => Some(PdfColor::new(1.0, 1.0, 1.0)),
        2 => Some(PdfColor::new(1.0, 0.0, 0.0)),
        3 => Some(PdfColor::new(0.0, 1.0, 0.0)),
        4 => Some(PdfColor::new(0.0, 0.0, 1.0)),
        5 => Some(PdfColor::new(1.0, 1.0, 0.0)),
        6 => Some(PdfColor::new(1.0, 0.0, 1.0)),
        7 => Some(PdfColor::new(0.0, 1.0, 1.0)),
        _ => None,
    }
}

fn apply_color_tint(color: PdfColor, tint: f32) -> PdfColor {
    if tint.abs() < f32::EPSILON {
        return color;
    }
    let is_neutral = (color.r - color.g).abs() < 0.0001 && (color.g - color.b).abs() < 0.0001;
    if is_neutral {
        let channel = if tint < 0.0 {
            color.r * (1.0 + tint)
        } else {
            color.r + (1.0 - color.r) * tint
        };
        if tint < 0.0 {
            if channel >= 0.9 {
                return PdfColor::new(channel - 0.005, channel - 0.001, channel - 0.005);
            }
            return PdfColor::new(channel, (channel + 0.001).min(1.0), channel);
        }
        return PdfColor::new(channel, channel, channel);
    }

    let max = color.r.max(color.g).max(color.b);
    let min = color.r.min(color.g).min(color.b);
    let mut hue = 0.0;
    let lightness = (max + min) / 2.0;
    let delta = max - min;
    let saturation = if delta == 0.0 {
        0.0
    } else if lightness <= 0.5 {
        delta / (max + min)
    } else {
        delta / (2.0 - max - min)
    };
    if delta != 0.0 {
        hue = if max == color.r {
            (color.g - color.b) / delta
        } else if max == color.g {
            2.0 + (color.b - color.r) / delta
        } else {
            4.0 + (color.r - color.g) / delta
        } / 6.0;
        if hue < 0.0 {
            hue += 1.0;
        }
    }
    let tinted_lightness = if tint < 0.0 {
        lightness * (1.0 + tint)
    } else {
        lightness * (1.0 - tint) + tint
    };
    let hue_to_rgb = |mut component: f32| {
        if component < 0.0 {
            component += 1.0;
        } else if component > 1.0 {
            component -= 1.0;
        }
        let upper = if tinted_lightness < 0.5 {
            tinted_lightness * (1.0 + saturation)
        } else {
            tinted_lightness + saturation - tinted_lightness * saturation
        };
        let lower = 2.0 * tinted_lightness - upper;
        if component * 6.0 < 1.0 {
            lower + (upper - lower) * component * 6.0
        } else if component * 2.0 < 1.0 {
            upper
        } else if component * 3.0 < 2.0 {
            lower + (upper - lower) * (2.0 / 3.0 - component) * 6.0
        } else {
            lower
        }
    };
    PdfColor::new(
        hue_to_rgb(hue + 1.0 / 3.0),
        hue_to_rgb(hue),
        hue_to_rgb(hue - 1.0 / 3.0),
    )
}

fn font_name_is_emphasis(font_name: Option<&str>) -> bool {
    font_name.is_some_and(|name| name.contains("黑体") || name.contains("小标宋"))
}

fn parse_cell_borders(border: roxmltree::Node<'_, '_>, theme_colors: &[PdfColor]) -> CellBorders {
    let side = |side_name: &str| {
        let side = border
            .children()
            .find(|side| side.has_tag_name(side_name) && side.attribute("style").is_some());
        let color = side.map(|side| {
            side.children()
                .find(|node| node.has_tag_name("color"))
                .and_then(|node| parse_xlsx_color(node, theme_colors))
                .unwrap_or(PdfColor::BLACK)
        });
        let width = side
            .and_then(|side| side.attribute("style"))
            .map(border_style_width)
            .unwrap_or(0.0);
        let dotted = side.is_some_and(|side| side.attribute("style") == Some("dotted"));
        (color, width, dotted)
    };
    let (left, left_width, left_dotted) = side("left");
    let (right, right_width, right_dotted) = side("right");
    let (top, top_width, top_dotted) = side("top");
    let (bottom, bottom_width, bottom_dotted) = side("bottom");
    CellBorders {
        left,
        right,
        top,
        bottom,
        left_width,
        right_width,
        top_width,
        bottom_width,
        left_dotted,
        right_dotted,
        top_dotted,
        bottom_dotted,
    }
}

fn border_style_width(style: &str) -> f32 {
    match style {
        "hair" => 0.1,
        "thin" | "dashed" | "dotted" => 0.5,
        "medium" | "mediumDashed" | "mediumDashDot" | "mediumDashDotDot" => 1.0,
        "thick" | "double" => 1.5,
        _ => CELL_BORDER_WIDTH,
    }
}

fn parse_table_borders(border: roxmltree::Node<'_, '_>, theme_colors: &[PdfColor]) -> TableBorders {
    let side = |side_name: &str| {
        let side = border
            .children()
            .find(|side| side.has_tag_name(side_name) && side.attribute("style").is_some())?;
        let color = side
            .children()
            .find(|node| node.has_tag_name("color"))
            .and_then(|node| parse_xlsx_color(node, theme_colors))
            .unwrap_or(PdfColor::BLACK);
        Some((color, border_style_width(side.attribute("style")?)))
    };
    TableBorders {
        edges: parse_cell_borders(border, theme_colors),
        vertical: side("vertical"),
        horizontal: side("horizontal"),
    }
}

fn merged_cell_borders(rows: &[RowData], merge_range: &MergeRange) -> CellBorders {
    let cell_at = |row_index: usize, column_index: usize| {
        rows.iter()
            .find(|row| row.index == row_index)
            .and_then(|row| row.cells.get(column_index))
    };
    CellBorders {
        left: (merge_range.start_row..=merge_range.end_row).find_map(|row_index| {
            cell_at(row_index, merge_range.start_col)?
                .style
                .borders
                .left
        }),
        right: (merge_range.start_row..=merge_range.end_row)
            .find_map(|row_index| cell_at(row_index, merge_range.end_col)?.style.borders.right),
        top: (merge_range.start_col..=merge_range.end_col).find_map(|column_index| {
            cell_at(merge_range.start_row, column_index)?
                .style
                .borders
                .top
        }),
        bottom: (merge_range.start_col..=merge_range.end_col).find_map(|column_index| {
            cell_at(merge_range.end_row, column_index)?
                .style
                .borders
                .bottom
        }),
        left_width: cell_at(merge_range.start_row, merge_range.start_col)
            .map(|cell| cell.style.borders.left_width)
            .unwrap_or(0.0),
        right_width: cell_at(merge_range.start_row, merge_range.end_col)
            .map(|cell| cell.style.borders.right_width)
            .unwrap_or(0.0),
        top_width: cell_at(merge_range.start_row, merge_range.start_col)
            .map(|cell| cell.style.borders.top_width)
            .unwrap_or(0.0),
        bottom_width: cell_at(merge_range.end_row, merge_range.start_col)
            .map(|cell| cell.style.borders.bottom_width)
            .unwrap_or(0.0),
        left_dotted: cell_at(merge_range.start_row, merge_range.start_col)
            .is_some_and(|cell| cell.style.borders.left_dotted),
        right_dotted: cell_at(merge_range.start_row, merge_range.end_col)
            .is_some_and(|cell| cell.style.borders.right_dotted),
        top_dotted: cell_at(merge_range.start_row, merge_range.start_col)
            .is_some_and(|cell| cell.style.borders.top_dotted),
        bottom_dotted: cell_at(merge_range.end_row, merge_range.start_col)
            .is_some_and(|cell| cell.style.borders.bottom_dotted),
    }
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

fn parse_horizontal_alignment(value: Option<&str>) -> HorizontalAlignment {
    match value {
        Some("left") => HorizontalAlignment::Left,
        Some("center") => HorizontalAlignment::Center,
        Some("right") => HorizontalAlignment::Right,
        _ => HorizontalAlignment::General,
    }
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
        if matches!(sheet.attribute("state"), Some("hidden" | "veryHidden")) {
            continue;
        }
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
    let default_row_height = read_default_row_height(sheet_xml)?;
    let column_styles = xml
        .descendants()
        .filter(|node| node.has_tag_name("col"))
        .filter_map(|column| {
            let start = column
                .attribute("min")?
                .parse::<usize>()
                .ok()?
                .checked_sub(1)?;
            let end = column
                .attribute("max")
                .and_then(|value| value.parse::<usize>().ok())
                .and_then(|value| value.checked_sub(1))
                .unwrap_or(start);
            let style = column
                .attribute("style")
                .and_then(|value| value.parse::<usize>().ok())
                .and_then(|style_id| styles.cells.get(style_id).copied())?;
            Some((start, end, style))
        })
        .collect::<Vec<_>>();
    let column_style = |column: usize| {
        column_styles
            .iter()
            .find(|(start, end, _)| column >= *start && column <= *end)
            .map(|(_, _, style)| *style)
            .unwrap_or_default()
    };
    let implicit_cell = |column: usize| {
        let mut style = column_style(column);
        style.fill_color = None;
        style.fill_override = false;
        CellData {
            style,
            ..CellData::default()
        }
    };
    let mut rows = Vec::new();

    for row in xml.descendants().filter(|node| node.has_tag_name("row")) {
        let row_index = row
            .attribute("r")
            .and_then(|value| value.parse::<usize>().ok())
            .and_then(|value| value.checked_sub(1))
            .unwrap_or(rows.len());
        let is_hidden = matches!(row.attribute("hidden"), Some("1" | "true"));
        let height = if is_hidden {
            0.0
        } else {
            row.attribute("ht")
                .and_then(|value| value.parse::<f32>().ok())
                .filter(|value| *value > 0.0)
                .unwrap_or(default_row_height)
        };
        let has_custom_height = row.attribute("ht").is_some();
        let mut cells: Vec<(usize, CellData)> = Vec::new();
        for cell in row.children().filter(|node| node.has_tag_name("c")) {
            let col = cell
                .attribute("r")
                .and_then(column_index_from_ref)
                .unwrap_or(cells.len());
            let mut value = read_cell_value(cell, shared_strings, styles, column_style(col));
            let formula = cell
                .children()
                .find(|node| node.has_tag_name("f"))
                .and_then(|node| node.text());
            let should_evaluate = value.text.is_empty()
                || formula.is_some_and(|formula| formula.trim().eq_ignore_ascii_case("TODAY()"));
            if let Some(number) = should_evaluate
                .then(|| formula.and_then(|formula| evaluate_simple_formula(formula, &rows)))
                .flatten()
            {
                value.text = format_numeric_value(number, "", value.style.number_format);
                value.is_numeric = true;
            }
            cells.push((col, value));
        }

        let width = cells
            .iter()
            .map(|(col, _)| *col)
            .max()
            .map(|col| col + 1)
            .unwrap_or(0);
        let mut row_values = (0..width).map(implicit_cell).collect::<Vec<_>>();
        for (col, value) in cells {
            if let Some(slot) = row_values.get_mut(col) {
                *slot = value;
            }
        }
        if is_hidden || has_custom_height || row_values.iter().any(cell_is_visible) {
            rows.push(RowData {
                index: row_index,
                height,
                cells: row_values,
            });
        }
    }

    let column_count = rows.iter().map(|row| row.cells.len()).max().unwrap_or(0);
    for row in &mut rows {
        while row.cells.len() < column_count {
            row.cells.push(implicit_cell(row.cells.len()));
        }
    }
    if column_styles
        .iter()
        .any(|(_, _, style)| style.fill_color.is_some() || style.borders.any())
    {
        let last_row = rows.last().map(|row| row.index).unwrap_or(0);
        let mut existing_rows = rows.into_iter().peekable();
        let mut completed_rows = Vec::with_capacity(last_row + 1);
        for row_index in 0..=last_row {
            if existing_rows
                .peek()
                .is_some_and(|row| row.index == row_index)
            {
                completed_rows.push(existing_rows.next().expect("peeked row exists"));
            } else {
                completed_rows.push(RowData {
                    index: row_index,
                    height: default_row_height,
                    cells: (0..column_count).map(implicit_cell).collect(),
                });
            }
        }
        rows = completed_rows;
    }

    Ok(rows)
}

fn trim_trailing_empty_rows(rows: &mut Vec<RowData>, images: &[SheetImage]) {
    let last_image_row = images.iter().map(|image| image.row).max();
    while rows.last().is_some_and(|row| {
        let has_visible_cell = row.cells.iter().any(cell_is_visible);
        !has_visible_cell && last_image_row.is_none_or(|image_row| row.index > image_row)
    }) {
        rows.pop();
    }
}

fn cell_is_visible(cell: &CellData) -> bool {
    !cell.text.is_empty() || cell.style.fill_color.is_some() || cell.style.borders.any()
}

fn read_cell_value(
    cell: roxmltree::Node<'_, '_>,
    shared_strings: &[String],
    styles: &XlsxStyles,
    default_style: CellStyle,
) -> CellData {
    let cell_type = cell.attribute("t");
    let style = cell
        .attribute("s")
        .and_then(|value| value.parse::<usize>().ok())
        .and_then(|style_id| styles.cells.get(style_id).copied())
        .unwrap_or(default_style);
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
        text: numeric_value.map_or_else(
            || value.to_owned(),
            |number| format_numeric_value(number, value, style.number_format),
        ),
        is_numeric: numeric_value.is_some(),
        style,
    }
}

fn format_numeric_value(value: f64, source: &str, number_format: NumberFormat) -> String {
    match number_format {
        NumberFormat::DateMonthDayYear => format_excel_date(value),
        NumberFormat::PercentageZeroDecimals => format!("{:.0}%", value * 100.0),
        NumberFormat::PercentageTwoDecimals => format!("{:.2}%", value * 100.0),
        NumberFormat::ThousandsTwoDecimals => format_thousands_two_decimals(value),
        NumberFormat::DollarTwoDecimals => {
            if value.is_sign_negative() {
                format!("-${}", format_thousands_two_decimals(value.abs()))
            } else {
                format!("${}", format_thousands_two_decimals(value))
            }
        }
        NumberFormat::DollarAccounting => {
            if value == 0.0 {
                "$ -".to_owned()
            } else if value.is_sign_negative() {
                format!("$ ({})", format_thousands_two_decimals(value.abs()))
            } else {
                format!("$ {}", format_thousands_two_decimals(value))
            }
        }
        NumberFormat::General
            if !source.contains(['e', 'E'])
                && source
                    .split_once('.')
                    .is_some_and(|(_, fraction)| fraction.len() >= 15) =>
        {
            value.to_string()
        }
        NumberFormat::General if source.is_empty() => value.to_string(),
        NumberFormat::General => source.to_owned(),
    }
}

fn format_excel_date(value: f64) -> String {
    let mut days = value.floor() as i64 - 25_569;
    days += 719_468;
    let era = if days >= 0 { days } else { days - 146_096 } / 146_097;
    let day_of_era = days - era * 146_097;
    let year_of_era =
        (day_of_era - day_of_era / 1_460 + day_of_era / 36_524 - day_of_era / 146_096) / 365;
    let mut year = year_of_era + era * 400;
    let day_of_year = day_of_era - (365 * year_of_era + year_of_era / 4 - year_of_era / 100);
    let month_part = (5 * day_of_year + 2) / 153;
    let day = day_of_year - (153 * month_part + 2) / 5 + 1;
    let month = month_part + if month_part < 10 { 3 } else { -9 };
    year += i64::from(month <= 2);
    format!("{month}/{day}/{year}")
}

fn evaluate_simple_formula(formula: &str, rows: &[RowData]) -> Option<f64> {
    let formula = formula.trim();
    if formula.eq_ignore_ascii_case("TODAY()") {
        return Some(excel_today_serial());
    }
    if formula.len() >= 5 && formula[..4].eq_ignore_ascii_case("SUM(") && formula.ends_with(')') {
        let (start, end) = formula[4..formula.len() - 1].split_once(':')?;
        let (start_col, start_row) = cell_position(start)?;
        let (end_col, end_row) = cell_position(end)?;
        let mut total = 0.0;
        for row in start_row.min(end_row)..=start_row.max(end_row) {
            for col in start_col.min(end_col)..=start_col.max(end_col) {
                total += numeric_cell_value(rows, col, row)?;
            }
        }
        return Some(total);
    }

    formula
        .split('+')
        .map(|cell_ref| {
            let (col, row) = cell_position(cell_ref.trim())?;
            numeric_cell_value(rows, col, row)
        })
        .try_fold(0.0, |total, value| value.map(|value| total + value))
}

fn excel_today_serial() -> f64 {
    let today = chrono::Local::now().date_naive();
    let excel_epoch =
        chrono::NaiveDate::from_ymd_opt(1899, 12, 30).expect("Excel epoch is a valid date");
    today.signed_duration_since(excel_epoch).num_days() as f64
}

fn numeric_cell_value(rows: &[RowData], col: usize, row: usize) -> Option<f64> {
    rows.iter()
        .find(|candidate| candidate.index == row)?
        .cells
        .get(col)?
        .text
        .replace(',', "")
        .parse()
        .ok()
}

fn format_thousands_two_decimals(value: f64) -> String {
    let rounded = (value.abs() * 100.0 + 1e-9).round() / 100.0;
    let formatted = format!("{rounded:.2}");
    let (integer, fraction) = formatted.split_once('.').unwrap_or((&formatted, "00"));
    let mut grouped = String::with_capacity(formatted.len() + integer.len() / 3);
    if value.is_sign_negative() {
        grouped.push('-');
    }
    for (index, character) in integer.chars().enumerate() {
        if index > 0 && (integer.len() - index) % 3 == 0 {
            grouped.push(',');
        }
        grouped.push(character);
    }
    grouped.push('.');
    grouped.push_str(fraction);
    grouped
}

#[cfg(test)]
fn wrap_cell_text(
    text: &str,
    max_width: f32,
    font_size: f32,
    bold: bool,
    italic: bool,
) -> Vec<String> {
    wrap_cell_text_with_font(text, max_width, font_size, bold, italic, None)
}

fn wrap_cell_text_with_font(
    text: &str,
    max_width: f32,
    font_size: f32,
    bold: bool,
    italic: bool,
    preferred_font: Option<&str>,
) -> Vec<String> {
    let mut lines = Vec::new();
    for paragraph in text.replace("\r\n", "\n").replace('\r', "\n").split('\n') {
        if paragraph.is_empty() {
            lines.push(String::new());
            continue;
        }
        let mut line = String::new();
        for character in paragraph.chars() {
            let mut candidate = line.clone();
            candidate.push(character);
            if !line.is_empty()
                && styled_text_width_with_font(&candidate, font_size, bold, italic, preferred_font)
                    > max_width
            {
                if let Some(split_index) = line.rfind(char::is_whitespace) {
                    let remainder = line[split_index..].trim_start().to_owned();
                    line.truncate(split_index);
                    lines.push(line.trim_end().to_owned());
                    line = remainder;
                    line.push(character);
                } else {
                    lines.push(line);
                    line = character.to_string();
                }
            } else {
                line = candidate;
            }
        }
        lines.push(line);
    }
    lines
}

fn cell_position(cell_ref: &str) -> Option<(usize, usize)> {
    let cell_ref = cell_ref.strip_prefix('$').unwrap_or(cell_ref);
    let column_length = cell_ref
        .chars()
        .take_while(|ch| ch.is_ascii_alphabetic())
        .count();
    if !(1..=3).contains(&column_length) {
        return None;
    }
    let (column_ref, row_ref) = cell_ref.split_at(column_length);
    let row_ref = row_ref.strip_prefix('$').unwrap_or(row_ref);
    if row_ref.is_empty() || !row_ref.chars().all(|ch| ch.is_ascii_digit()) {
        return None;
    }
    let col = column_index_from_ref(column_ref)?;
    let row = row_ref.parse::<usize>().ok()?.checked_sub(1)?;
    Some((col, row))
}

fn column_index_from_ref(cell_ref: &str) -> Option<usize> {
    let mut col = 0usize;
    let mut seen_letter = false;
    for ch in cell_ref.chars().filter(|ch| ch.is_ascii_alphabetic()) {
        seen_letter = true;
        col = col
            .checked_mul(26)?
            .checked_add(ch.to_ascii_uppercase() as usize - 'A' as usize + 1)?;
    }
    seen_letter.then_some(col.saturating_sub(1))
}

fn render_xlsx(doc: &mut PdfDocument, sheets: &[SheetData], page_size_override: Option<PageSize>) {
    for sheet in sheets {
        let image_ids = sheet
            .images
            .iter()
            .map(|image| match &image.data {
                SheetImageData::Jpeg(data) => {
                    doc.add_jpeg_image(data.clone(), image.pixel_width, image.pixel_height)
                }
                SheetImageData::Rgba(data) => {
                    doc.add_rgba_image(data.clone(), image.pixel_width, image.pixel_height)
                }
            })
            .collect::<Vec<_>>();
        render_sheet(
            doc,
            sheet,
            &image_ids,
            page_size_override.unwrap_or(sheet.page_setup.page_size),
        );
    }
}

fn render_sheet(
    doc: &mut PdfDocument,
    sheet: &SheetData,
    image_ids: &[usize],
    mut page_size: PageSize,
) {
    let column_count = rendered_column_count(sheet);
    let mut widths = sheet.column_widths.clone();
    widths.resize(column_count, COL_WIDTH);
    widths.truncate(column_count);
    let unscaled_width = widths.iter().sum::<f32>();
    if sheet.page_setup.page_size_from_printer
        && sheet.page_setup.fit_to_width
        && sheet.page_setup.fit_to_height
    {
        let last_row = sheet.rows.last().map(|row| row.index).unwrap_or(0);
        let unscaled_height = (0..=last_row)
            .map(|row_index| {
                sheet
                    .rows
                    .iter()
                    .find(|row| row.index == row_index)
                    .map(|row| row.height)
                    .unwrap_or(sheet.default_row_height)
            })
            .sum::<f32>();
        let required_width = unscaled_width * sheet.page_setup.print_scale
            + sheet.page_setup.margin_left
            + sheet.page_setup.margin_right;
        let required_height =
            unscaled_height * sheet.page_setup.print_scale * FIT_TO_PAGE_VERTICAL_SCALE
                + sheet.page_setup.margin_top
                + FIT_TO_PAGE_TOP_OFFSET
                + sheet.page_setup.margin_bottom;
        let media_scale = (required_width / page_size.width)
            .max(required_height / page_size.height)
            .max(1.0);
        page_size.width *= media_scale;
        page_size.height *= media_scale;
    }
    let usable_width =
        page_size.width - sheet.page_setup.margin_left - sheet.page_setup.margin_right;
    let mut content_scale = effective_content_scale(
        unscaled_width,
        usable_width,
        sheet.page_setup.print_scale,
        sheet.page_setup.fit_to_width,
    );
    if sheet.page_setup.fit_to_height {
        let last_row = sheet.rows.last().map(|row| row.index).unwrap_or(0);
        let unscaled_height = (0..=last_row)
            .map(|row_index| {
                sheet
                    .rows
                    .iter()
                    .find(|row| row.index == row_index)
                    .map(|row| row.height)
                    .unwrap_or(sheet.default_row_height)
            })
            .sum::<f32>();
        let vertical_scale = xlsx_vertical_scale(sheet, FIT_TO_PAGE_VERTICAL_SCALE);
        let top_offset = if sheet.print_title_rows.is_some() {
            0.0
        } else {
            FIT_TO_PAGE_TOP_OFFSET
        };
        let usable_height = page_size.height
            - sheet.page_setup.margin_top
            - top_offset
            - sheet.page_setup.margin_bottom;
        if unscaled_height > 0.0 {
            content_scale =
                content_scale.min((usable_height / (unscaled_height * vertical_scale)).min(1.0));
        }
    }
    let horizontal_scale = if sheet.print_title_rows.is_some() {
        PRINT_TITLE_HORIZONTAL_SCALE
    } else if sheet.page_setup.fit_to_width {
        FIT_TO_PAGE_HORIZONTAL_SCALE
    } else {
        1.0
    };
    let horizontal_geometry_scale = content_scale * horizontal_scale;
    for width in &mut widths {
        *width *= horizontal_geometry_scale;
    }
    let content_width = widths.iter().sum::<f32>();
    let content_left = if sheet.page_setup.horizontal_centered && content_width < usable_width {
        sheet.page_setup.margin_left + (usable_width - content_width) / 2.0
            - if sheet.print_title_rows.is_some() {
                PRINT_TITLE_HORIZONTAL_OFFSET
            } else if sheet.page_setup.fit_to_width {
                FIT_TO_PAGE_HORIZONTAL_OFFSET
            } else {
                0.0
            }
    } else {
        sheet.page_setup.margin_left
    };
    let vertical_scale = xlsx_vertical_scale(
        sheet,
        if sheet.page_setup.fit_to_width {
            FIT_TO_PAGE_VERTICAL_SCALE
        } else {
            1.0
        },
    );
    let margin_top = sheet.page_setup.margin_top
        + if sheet.page_setup.fit_to_width {
            FIT_TO_PAGE_TOP_OFFSET
        } else {
            0.0
        };
    let margin_bottom = sheet.page_setup.margin_bottom;
    let column_ranges = if sheet.page_setup.fit_to_width {
        vec![(0, widths.len())]
    } else {
        column_groups(&widths, usable_width)
            .into_iter()
            .filter(|(start, end)| column_group_has_content(sheet, *start, *end))
            .collect()
    };
    let layout = SheetRenderLayout {
        page_size,
        content_scale,
        horizontal_geometry_scale,
        content_left,
        vertical_scale,
        margin_top,
        margin_bottom,
    };

    for (column_start, column_end) in column_ranges {
        render_sheet_columns(
            doc,
            sheet,
            image_ids,
            &widths,
            column_start,
            column_end,
            &layout,
        );
    }
}

fn rendered_column_count(sheet: &SheetData) -> usize {
    let visible_cell_count = sheet
        .rows
        .iter()
        .flat_map(|row| row.cells.iter().enumerate())
        .filter(|(_, cell)| cell_is_visible(cell))
        .map(|(column, _)| column + 1)
        .max()
        .unwrap_or(0);
    let merged_column_count = sheet
        .merges
        .iter()
        .filter(|merge| merge.start_col < visible_cell_count)
        .map(|merge| merge.end_col + 1)
        .max()
        .unwrap_or(0);
    let image_column_count = sheet
        .images
        .iter()
        .map(|image| image.col + 1)
        .max()
        .unwrap_or(0);
    visible_cell_count
        .max(merged_column_count)
        .max(image_column_count)
        .max(1)
}

fn column_group_has_content(sheet: &SheetData, column_start: usize, column_end: usize) -> bool {
    sheet.rows.iter().any(|row| {
        row.cells
            .get(column_start..column_end.min(row.cells.len()))
            .is_some_and(|cells| cells.iter().any(|cell| !cell.text.is_empty()))
    }) || sheet
        .images
        .iter()
        .any(|image| image.col >= column_start && image.col < column_end)
}

fn xlsx_vertical_scale(sheet: &SheetData, fallback: f32) -> f32 {
    if sheet.page_setup.fit_to_width && !sheet.page_setup.fit_to_height {
        if sheet.print_title_rows.is_some() {
            PRINT_TITLE_WIDTH_ONLY_VERTICAL_SCALE
        } else {
            FIT_TO_WIDTH_ONLY_VERTICAL_SCALE
        }
    } else if sheet.print_title_rows.is_some() {
        if sheet.page_setup.page_size_from_printer {
            1.0
        } else {
            PRINT_TITLE_VERTICAL_SCALE
        }
    } else if sheet.page_setup.page_size_from_printer && sheet.page_setup.fit_to_width {
        1.0
    } else {
        fallback
    }
}

struct SheetRenderLayout {
    page_size: PageSize,
    content_scale: f32,
    horizontal_geometry_scale: f32,
    content_left: f32,
    vertical_scale: f32,
    margin_top: f32,
    margin_bottom: f32,
}

fn render_sheet_columns(
    doc: &mut PdfDocument,
    sheet: &SheetData,
    image_ids: &[usize],
    column_widths: &[f32],
    column_start: usize,
    column_end: usize,
    layout: &SheetRenderLayout,
) {
    let SheetRenderLayout {
        page_size,
        content_scale,
        horizontal_geometry_scale,
        content_left,
        vertical_scale,
        margin_top,
        margin_bottom,
    } = *layout;
    let row_scale = content_scale * vertical_scale;
    let automatic_breaks = automatic_row_breaks(sheet, page_size, row_scale);
    let first_page_index = doc.pages().len();
    let mut page_index = first_page_index;
    doc.add_page(page_size.width, page_size.height);
    let mut y = page_size.height - margin_top;
    let mut next_row_index = 0;

    for row in &sheet.rows {
        for _ in next_row_index..row.index {
            y -= sheet.default_row_height * row_scale;
        }

        if row.height <= 0.0 {
            next_row_index = row.index + 1;
            continue;
        }

        if (sheet.row_breaks.contains(&row.index) || automatic_breaks.contains(&row.index))
            && y < page_size.height - margin_top
        {
            page_index = doc.pages().len();
            doc.add_page(page_size.width, page_size.height);
            y = page_size.height - margin_top;
            repeat_print_title_rows(
                doc,
                sheet,
                image_ids,
                column_widths,
                column_start,
                column_end,
                content_scale,
                horizontal_geometry_scale,
                content_left,
                row_scale,
                page_index,
                &mut y,
                row.index,
            );
        }

        let row_height = row.height * row_scale;

        if sheet.print_title_rows.is_none() && y - row_height < margin_bottom {
            page_index = doc.pages().len();
            doc.add_page(page_size.width, page_size.height);
            y = page_size.height - margin_top;
            repeat_print_title_rows(
                doc,
                sheet,
                image_ids,
                column_widths,
                column_start,
                column_end,
                content_scale,
                horizontal_geometry_scale,
                content_left,
                row_scale,
                page_index,
                &mut y,
                row.index,
            );
        }
        y -= row_height;

        render_xlsx_row(
            doc,
            sheet,
            image_ids,
            column_widths,
            column_start,
            column_end,
            content_scale,
            horizontal_geometry_scale,
            content_left,
            row_scale,
            page_index,
            y,
            row,
        );
        next_row_index = row.index + 1;
    }

    let page = doc.page_mut(first_page_index).expect("page index is valid");
    for (image, image_id) in sheet.images.iter().zip(image_ids).filter(|(image, _)| {
        image.foreground && image.col >= column_start && image.col < column_end
    }) {
        let x = content_left
            + column_widths[column_start..image.col].iter().sum::<f32>()
            + image.col_offset * horizontal_geometry_scale;
        let rows_above = (0..image.row)
            .map(|row_index| {
                sheet
                    .rows
                    .iter()
                    .find(|row| row.index == row_index)
                    .map(|row| row.height)
                    .unwrap_or(sheet.default_row_height)
            })
            .sum::<f32>();
        let top =
            page_size.height - margin_top - rows_above * row_scale - image.row_offset * row_scale
                + GROUP_DRAWING_TOP_OFFSET;
        page.add_image(
            *image_id,
            x,
            top - image.height * row_scale,
            image.width * horizontal_geometry_scale,
            image.height * row_scale,
        );
    }
}

fn automatic_row_breaks(sheet: &SheetData, page_size: PageSize, row_scale: f32) -> Vec<usize> {
    const PRINT_BOTTOM_RESERVE: f32 = 1.0;
    let Some((title_start, title_end)) = sheet.print_title_rows else {
        return Vec::new();
    };
    let title_height = (title_start..=title_end)
        .map(|row_index| {
            sheet
                .rows
                .iter()
                .find(|row| row.index == row_index)
                .map(|row| row.height)
                .unwrap_or(sheet.default_row_height)
                * row_scale
        })
        .sum::<f32>();
    let mut breaks = Vec::new();
    let mut y = page_size.height - sheet.page_setup.margin_top;
    let last_row = sheet.rows.last().map(|row| row.index).unwrap_or(0);
    for row_index in 0..=last_row {
        if sheet.row_breaks.contains(&row_index)
            && y < page_size.height - sheet.page_setup.margin_top
        {
            y = page_size.height - sheet.page_setup.margin_top - title_height;
        }
        let row_height = sheet
            .rows
            .iter()
            .find(|row| row.index == row_index)
            .map(|row| row.height)
            .unwrap_or(sheet.default_row_height)
            * row_scale;
        if y - row_height < sheet.page_setup.margin_bottom + PRINT_BOTTOM_RESERVE {
            breaks.push(row_index);
            y = page_size.height - sheet.page_setup.margin_top - title_height;
        }
        y -= row_height;
    }
    breaks
}

#[allow(clippy::too_many_arguments)]
fn repeat_print_title_rows(
    doc: &mut PdfDocument,
    sheet: &SheetData,
    image_ids: &[usize],
    column_widths: &[f32],
    column_start: usize,
    column_end: usize,
    content_scale: f32,
    horizontal_geometry_scale: f32,
    content_left: f32,
    row_scale: f32,
    page_index: usize,
    y: &mut f32,
    current_row_index: usize,
) {
    let Some((title_start, title_end)) = sheet.print_title_rows else {
        return;
    };
    if current_row_index <= title_end {
        return;
    }
    for title_row_index in title_start..=title_end {
        let row = sheet
            .rows
            .iter()
            .find(|candidate| candidate.index == title_row_index);
        let row_height = row
            .map(|row| row.height)
            .unwrap_or(sheet.default_row_height)
            * row_scale;
        *y -= row_height;
        if let Some(row) = row {
            render_xlsx_row(
                doc,
                sheet,
                image_ids,
                column_widths,
                column_start,
                column_end,
                content_scale,
                horizontal_geometry_scale,
                content_left,
                row_scale,
                page_index,
                *y,
                row,
            );
        }
    }
}

#[allow(clippy::too_many_arguments)]
fn render_xlsx_row(
    doc: &mut PdfDocument,
    sheet: &SheetData,
    image_ids: &[usize],
    column_widths: &[f32],
    column_start: usize,
    column_end: usize,
    content_scale: f32,
    horizontal_geometry_scale: f32,
    content_left: f32,
    row_scale: f32,
    page_index: usize,
    y: f32,
    row: &RowData,
) {
    let row_height = row.height * row_scale;

    let page = doc.page_mut(page_index).expect("page index is valid");

    struct PendingCellText {
        cell_x: f32,
        cell_y: f32,
        cell_width: f32,
        cell_height: f32,
        clip_width: f32,
        should_clip: bool,
        lines: Vec<String>,
        font_size: f32,
        indent_width: f32,
        style: CellStyle,
        align_right: bool,
        accounting_value: Option<String>,
    }

    for (image, image_id) in sheet.images.iter().zip(image_ids).filter(|(image, _)| {
        !image.foreground
            && image.row == row.index
            && image.col >= column_start
            && image.col < column_end
    }) {
        let x = content_left
            + column_widths[column_start..image.col].iter().sum::<f32>()
            + image.col_offset * horizontal_geometry_scale;
        let image_height = image.height * row_scale;
        let top = y + row_height - image.row_offset * row_scale;
        page.add_image(
            *image_id,
            x,
            top - image_height,
            image.width * horizontal_geometry_scale,
            image_height,
        );
    }

    let mut cell_x = content_left;
    let mut pending_text = Vec::new();
    let empty_cell = CellData::default();
    for column_index in column_start..column_end {
        let merge = sheet.merges.iter().find(|merge| {
            merge.start_row <= row.index
                && merge.end_row >= row.index
                && merge.start_col <= column_index
                && merge.end_col >= column_index
        });
        if merge.is_some_and(|merge| row.index > merge.start_row) {
            cell_x += column_widths[column_index];
            continue;
        }
        if merge.is_some_and(|merge| column_index > merge.start_col && column_index != column_start)
        {
            cell_x += column_widths[column_index];
            continue;
        }
        let source_column = merge.map(|merge| merge.start_col).unwrap_or(column_index);
        let cell = row.cells.get(source_column).unwrap_or(&empty_cell);
        let merge_end = merge
            .map(|merge| (merge.end_col + 1).min(column_end))
            .unwrap_or(column_index + 1);
        let cell_width = column_widths[column_index..merge_end].iter().sum::<f32>();
        let cell_height = merge
            .map(|merge| {
                (merge.start_row..=merge.end_row)
                    .map(|row_index| {
                        sheet
                            .rows
                            .iter()
                            .find(|candidate| candidate.index == row_index)
                            .map(|candidate| candidate.height)
                            .unwrap_or(sheet.default_row_height)
                            * row_scale
                    })
                    .sum::<f32>()
            })
            .unwrap_or(row_height);
        let cell_y = y - (cell_height - row_height);
        let cell_borders = merge
            .map(|merge_range| merged_cell_borders(&sheet.rows, merge_range))
            .unwrap_or(cell.style.borders);
        if let Some(fill) = cell.style.fill_color {
            let previous_cell = column_index
                .checked_sub(1)
                .filter(|_| column_index > column_start)
                .and_then(|index| row.cells.get(index));
            let next_cell = row.cells.get(merge_end);
            let extend_left = cell_borders.left.is_none()
                && previous_cell.is_some_and(|previous| {
                    previous.style.fill_color == Some(fill)
                        && previous.style.borders.right.is_none()
                });
            let extend_fill = merge_end == column_end
                || next_cell.is_some_and(|next| next.style.fill_color == Some(fill));
            page.add_rect(
                cell_x - if extend_left { CELL_BORDER_WIDTH } else { 0.0 },
                cell_y,
                cell_width
                    + if extend_left { CELL_BORDER_WIDTH } else { 0.0 }
                    + if extend_fill { CELL_BORDER_WIDTH } else { 0.0 },
                cell_height,
                fill,
            );
        }
        if let Some(border) = cell_borders.bottom {
            let border_width = cell_borders.bottom_width.max(0.1);
            let position_offset = if border_width <= 0.5 { 0.6 } else { 0.0 };
            if cell_borders.bottom_dotted {
                page.add_dashed_line(
                    cell_x,
                    cell_y,
                    cell_x + cell_width,
                    cell_y,
                    border,
                    border_width,
                );
            } else {
                page.add_rect(
                    cell_x,
                    cell_y - border_width / 2.0 + position_offset,
                    cell_width,
                    border_width,
                    border,
                );
            }
        }
        if let Some(border) = cell_borders.top {
            let border_width = cell_borders.top_width.max(0.1);
            let position_offset = if border_width <= 0.5 { 0.6 } else { 0.0 };
            if cell_borders.top_dotted {
                page.add_dashed_line(
                    cell_x,
                    cell_y + cell_height,
                    cell_x + cell_width,
                    cell_y + cell_height,
                    border,
                    border_width,
                );
            } else {
                page.add_rect(
                    cell_x,
                    cell_y + cell_height - border_width / 2.0 + position_offset,
                    cell_width,
                    border_width,
                    border,
                );
            }
        }
        if let Some(border) = cell_borders.left {
            let border_width = cell_borders.left_width.max(0.1);
            let position_offset = if border_width <= 0.5 { 0.4 } else { 0.0 };
            if cell_borders.left_dotted {
                page.add_dashed_line(
                    cell_x,
                    cell_y,
                    cell_x,
                    cell_y + cell_height,
                    border,
                    border_width,
                );
            } else {
                page.add_rect(
                    cell_x - border_width / 2.0 - position_offset,
                    cell_y,
                    border_width,
                    cell_height,
                    border,
                );
            }
        }
        if let Some(border) = cell_borders.right {
            let border_width = cell_borders.right_width.max(0.1);
            let position_offset = if border_width <= 0.5 { 0.4 } else { 0.0 };
            if cell_borders.right_dotted {
                page.add_dashed_line(
                    cell_x + cell_width,
                    cell_y,
                    cell_x + cell_width,
                    cell_y + cell_height,
                    border,
                    border_width,
                );
            } else {
                page.add_rect(
                    cell_x + cell_width - border_width / 2.0 - position_offset,
                    cell_y,
                    border_width,
                    cell_height,
                    border,
                );
            }
        }
        let font_size = cell.style.font_size * content_scale;
        let indent_width = styled_text_width_with_font(
            &"0000".repeat(cell.style.indent as usize),
            CELL_FONT_SIZE * content_scale,
            false,
            false,
            None,
        );
        let text = if merge.is_some_and(|merge| merge.start_col < column_start) {
            String::new()
        } else {
            cell.text.replace(['\r', '\n'], " ")
        };
        let mut clip_width = cell_width;
        let mut overflow_is_blocked = false;
        if merge.is_none() {
            for (next_column, next_width) in column_widths
                .iter()
                .enumerate()
                .take(column_end)
                .skip(column_index + 1)
            {
                if row
                    .cells
                    .get(next_column)
                    .is_some_and(|next_cell| !next_cell.text.is_empty())
                {
                    overflow_is_blocked = true;
                    break;
                }
                clip_width += next_width;
            }
        }
        let lines = if cell.style.wrap_text {
            wrap_cell_text_with_font(
                &cell.text,
                (cell_width - 6.0 - indent_width).max(1.0),
                font_size,
                cell.style.bold,
                cell.style.italic,
                cell.style.preferred_font,
            )
        } else {
            vec![text]
        };
        let max_text_width = lines
            .iter()
            .map(|line| {
                styled_text_width_with_font(
                    line,
                    font_size,
                    cell.style.bold,
                    cell.style.italic,
                    cell.style.preferred_font,
                )
            })
            .fold(0.0, f32::max);
        let should_clip =
            cell.style.wrap_text || (max_text_width > clip_width && overflow_is_blocked);
        pending_text.push(PendingCellText {
            cell_x,
            cell_y,
            cell_width,
            cell_height,
            clip_width: if cell.style.wrap_text {
                cell_width
            } else {
                clip_width
            },
            should_clip,
            lines,
            font_size,
            indent_width,
            style: cell.style,
            align_right: cell.is_numeric || has_rtl_base_direction(&cell.text),
            accounting_value: matches!(cell.style.number_format, NumberFormat::DollarAccounting)
                .then(|| cell.text.strip_prefix("$ ").map(str::to_owned))
                .flatten(),
        });
        cell_x += column_widths[column_index];
    }

    for text in pending_text {
        if text.should_clip {
            page.push_clip(text.cell_x, text.cell_y, text.clip_width, text.cell_height);
        }
        let line_height = text.font_size * 1.2;
        let block_height = line_height * text.lines.len() as f32;
        let baseline_offset = text.font_size * 0.2;
        let lowest_baseline = match text.style.vertical_alignment {
            VerticalAlignment::Top => {
                text.cell_y + (text.cell_height - block_height).max(0.0) + baseline_offset
            }
            VerticalAlignment::Center => {
                text.cell_y + (text.cell_height - block_height).max(0.0) / 2.0 + baseline_offset
            }
            VerticalAlignment::Bottom => text.cell_y + baseline_offset.max(1.0),
        } + match text.style.preferred_font {
            Some("framd") => 0.9,
            Some("grandview" | "grandviewdisplay") => 0.6,
            _ => 0.0,
        };
        let line_count = text.lines.len();
        if let Some(value) = text.accounting_value {
            let text_y = lowest_baseline;
            page.add_styled_text(
                "$",
                text.cell_x + 3.0,
                text_y,
                text.font_size,
                PdfTextStyle {
                    color: text.style.font_color,
                    bold: text.style.bold,
                    italic: text.style.italic,
                    preferred_font: text.style.preferred_font,
                },
            );
            let value_width = styled_text_width_with_font(
                &value,
                text.font_size,
                text.style.bold,
                text.style.italic,
                text.style.preferred_font,
            );
            page.add_styled_text(
                value,
                text.cell_x + text.cell_width - value_width - 3.0,
                text_y,
                text.font_size,
                PdfTextStyle {
                    color: text.style.font_color,
                    bold: text.style.bold,
                    italic: text.style.italic,
                    preferred_font: text.style.preferred_font,
                },
            );
            if text.should_clip {
                page.pop_clip();
            }
            continue;
        }
        for (line_index, line) in text.lines.into_iter().enumerate() {
            let text_width = styled_text_width_with_font(
                &line,
                text.font_size,
                text.style.bold,
                text.style.italic,
                text.style.preferred_font,
            );
            let x = match text.style.horizontal_alignment {
                HorizontalAlignment::Center => {
                    text.cell_x + (text.cell_width - text_width).max(0.0) / 2.0
                }
                HorizontalAlignment::Right => {
                    text.cell_x + text.cell_width - text_width - 3.0 - text.indent_width
                }
                HorizontalAlignment::General if text.align_right => {
                    text.cell_x + text.cell_width - text_width - 3.0
                }
                HorizontalAlignment::General | HorizontalAlignment::Left => {
                    text.cell_x
                        + if text.indent_width > 0.0 {
                            text.indent_width
                        } else {
                            3.0
                        }
                }
            };
            let text_y = lowest_baseline + (line_count - line_index - 1) as f32 * line_height;
            page.add_styled_text(
                &line,
                x,
                text_y,
                text.font_size,
                PdfTextStyle {
                    color: text.style.font_color,
                    bold: text.style.bold,
                    italic: text.style.italic,
                    preferred_font: text.style.preferred_font,
                },
            );
            if text.style.strike && !line.is_empty() {
                let strike_y = text_y + text.font_size * 0.32;
                page.add_line(
                    x,
                    strike_y,
                    x + text_width,
                    strike_y,
                    text.style.font_color,
                    (text.font_size * 0.06).max(0.35),
                );
            }
        }
        if text.should_clip {
            page.pop_clip();
        }
    }
}

fn has_rtl_base_direction(text: &str) -> bool {
    text.chars()
        .find_map(|character| match bidi_class(character) {
            BidiClass::R | BidiClass::AL => Some(true),
            BidiClass::L => Some(false),
            _ => None,
        })
        .unwrap_or(false)
}

#[cfg(test)]
mod tests {
    use std::collections::HashMap;

    use super::{
        apply_color_tint, apply_print_area, apply_sheet_conditional_formats,
        apply_table_fill_layer, apply_table_font_layer, cell_position, column_group_has_content,
        column_groups, column_index_from_ref, composite_rgba_background, effective_content_scale,
        font_name_is_emphasis, format_excel_date, format_thousands_two_decimals,
        has_rtl_base_direction, indexed_color, jpeg_dimensions, merged_cell_borders,
        parse_cell_borders, parse_conditional_text_search, parse_horizontal_alignment,
        parse_number_format, parse_print_area, parse_rgb_color, parse_vertical_alignment,
        parse_windows_devmode_page_size, read_column_widths, read_merge_ranges, read_page_setup,
        read_row_breaks, read_sheet_rows, read_two_cell_shape, relationship_id,
        rendered_column_count, spreadsheet_theme_colors, trim_trailing_empty_rows, wrap_cell_text,
        xlsx_vertical_scale, CellData, CellStyle, DifferentialFontStyle, HorizontalAlignment,
        MergeRange, RowData, SheetData, SheetImage, SheetImageData, SheetPageSetup,
        VerticalAlignment, XlsxStyles,
    };
    use crate::{PageSize, PdfColor};

    #[test]
    fn parses_excel_column_references() {
        assert_eq!(column_index_from_ref("A1"), Some(0));
        assert_eq!(column_index_from_ref("Z9"), Some(25));
        assert_eq!(column_index_from_ref("AA10"), Some(26));
    }

    #[test]
    fn rejects_non_a1_cell_references() {
        assert_eq!(cell_position("$AA$10"), Some((26, 9)));
        assert_eq!(cell_position("GroceryList[QTY]"), None);
        assert_eq!(cell_position("SUM(A1:A2)"), None);
    }

    #[test]
    fn parses_conditional_text_search() {
        assert_eq!(
            parse_conditional_text_search(r#"ISNUMBER(SEARCH("accept",$H$16))=TRUE"#),
            Some((7, 15, "accept".to_owned()))
        );
    }

    #[test]
    fn parses_quoted_sheet_print_area() {
        assert_eq!(
            parse_print_area("'FRI report'!$A$2:$Y$310"),
            Some((
                "FRI report".to_owned(),
                MergeRange {
                    start_col: 0,
                    end_col: 24,
                    start_row: 1,
                    end_row: 309,
                },
            ))
        );
    }

    #[test]
    fn clips_and_rebases_sheet_to_print_area() {
        let mut rows = (0..3)
            .map(|index| RowData {
                index,
                height: 15.0,
                cells: vec![CellData::default(); 3],
            })
            .collect();
        let mut images = Vec::new();
        let mut column_widths = vec![10.0, 20.0, 30.0];
        let mut merges = vec![MergeRange {
            start_col: 0,
            end_col: 2,
            start_row: 0,
            end_row: 2,
        }];
        let mut row_breaks = vec![2];
        let mut print_title_rows = Some((0, 1));

        apply_print_area(
            &mut rows,
            &mut images,
            &mut column_widths,
            &mut merges,
            &mut row_breaks,
            &mut print_title_rows,
            MergeRange {
                start_col: 1,
                end_col: 2,
                start_row: 1,
                end_row: 2,
            },
        );

        assert_eq!(rows.iter().map(|row| row.index).collect::<Vec<_>>(), [0, 1]);
        assert!(rows.iter().all(|row| row.cells.len() == 2));
        assert_eq!(column_widths, [20.0, 30.0]);
        assert_eq!(
            merges,
            [MergeRange {
                start_col: 0,
                end_col: 1,
                start_row: 0,
                end_row: 1,
            }]
        );
        assert_eq!(row_breaks, [1]);
        assert_eq!(print_title_rows, Some((0, 0)));
    }

    #[test]
    fn defaults_unspecified_page_setup_to_a4() {
        let page_setup =
            read_page_setup("<worksheet><sheetData/></worksheet>").expect("worksheet XML is valid");

        assert_eq!(page_setup.page_size, PageSize::A4);
        assert_eq!(page_setup.margin_left, super::MARGIN_X);
        assert!(!page_setup.fit_to_width);
    }

    #[test]
    fn reads_letter_landscape_page_setup() {
        let page_setup = read_page_setup(
            r#"<worksheet><pageSetup paperSize="1" orientation="landscape"/></worksheet>"#,
        )
        .expect("worksheet XML is valid");

        assert_eq!(page_setup.page_size.width, PageSize::LETTER.height);
        assert_eq!(page_setup.page_size.height, PageSize::LETTER.width);
    }

    #[test]
    fn reads_letter_landscape_from_windows_devmode() {
        let mut devmode = vec![0_u8; 220];
        devmode[68..70].copy_from_slice(&220_u16.to_le_bytes());
        devmode[76..78].copy_from_slice(&2_u16.to_le_bytes());
        devmode[78..80].copy_from_slice(&1_u16.to_le_bytes());

        let page_size =
            parse_windows_devmode_page_size(&devmode).expect("DEVMODE contains a paper size");

        assert_eq!(page_size.width, PageSize::LETTER.height);
        assert_eq!(page_size.height, PageSize::LETTER.width);
    }

    #[test]
    fn ignores_trailing_columns_without_visible_content() {
        let sheet = SheetData {
            rows: vec![RowData {
                index: 0,
                height: 15.0,
                cells: vec![
                    CellData {
                        text: "Visible".to_owned(),
                        ..CellData::default()
                    },
                    CellData::default(),
                ],
            }],
            images: Vec::new(),
            column_widths: vec![100.0, 500.0],
            merges: Vec::new(),
            row_breaks: Vec::new(),
            default_row_height: 15.0,
            print_title_rows: None,
            page_setup: SheetPageSetup::default(),
        };

        assert_eq!(rendered_column_count(&sheet), 1);
    }

    #[test]
    fn keeps_printer_layout_rows_at_full_scale_with_print_titles() {
        let mut sheet = SheetData {
            rows: Vec::new(),
            images: Vec::new(),
            column_widths: Vec::new(),
            merges: Vec::new(),
            row_breaks: Vec::new(),
            default_row_height: 15.0,
            print_title_rows: Some((0, 3)),
            page_setup: SheetPageSetup::default(),
        };
        sheet.page_setup.page_size_from_printer = true;

        assert_eq!(xlsx_vertical_scale(&sheet, 0.75), 1.0);
    }

    #[test]
    fn calibrates_width_only_fit_with_print_titles() {
        let mut sheet = SheetData {
            rows: Vec::new(),
            images: Vec::new(),
            column_widths: Vec::new(),
            merges: Vec::new(),
            row_breaks: Vec::new(),
            default_row_height: 15.0,
            print_title_rows: Some((12, 12)),
            page_setup: SheetPageSetup::default(),
        };
        sheet.page_setup.fit_to_width = true;

        assert_eq!(
            xlsx_vertical_scale(&sheet, 0.75),
            super::PRINT_TITLE_WIDTH_ONLY_VERTICAL_SCALE
        );
    }

    #[test]
    fn renders_solid_preset_rectangle() {
        let xml = r#"<twoCellAnchor><from><col>0</col><colOff>0</colOff><row>0</row><rowOff>0</rowOff></from><to><col>2</col><colOff>0</colOff><row>2</row><rowOff>0</rowOff></to><sp><spPr><xfrm><ext cx="25400" cy="12700"/></xfrm><prstGeom prst="rect"/><solidFill><srgbClr val="FF0000"/></solidFill></spPr></sp></twoCellAnchor>"#;
        let document = roxmltree::Document::parse(xml).expect("shape XML is valid");

        let rows = vec![
            RowData {
                index: 0,
                height: 15.0,
                cells: Vec::new(),
            },
            RowData {
                index: 1,
                height: 25.0,
                cells: Vec::new(),
            },
        ];
        let shape = read_two_cell_shape(document.root_element(), &[], &[10.0, 20.0], &rows, 15.0)
            .expect("solid rectangle should render");

        assert_eq!(shape.col, 0);
        assert_eq!(shape.row, 0);
        assert_eq!(shape.width, 30.0);
        assert_eq!(shape.height, 40.0);
        assert!(matches!(shape.data, SheetImageData::Rgba(_)));
    }

    #[test]
    fn composites_transparent_picture_pixels_over_solid_background() {
        let mut image = image::RgbaImage::from_pixel(1, 1, image::Rgba([255, 0, 0, 128]));

        composite_rgba_background(&mut image, PdfColor::new(1.0, 1.0, 1.0));

        assert_eq!(image.get_pixel(0, 0).0, [255, 127, 127, 255]);
    }

    #[test]
    fn recognizes_custom_dollar_formats() {
        let formats = HashMap::from([(164, "\"$\"#,##0.00".to_owned())]);

        assert!(matches!(
            parse_number_format(Some("164"), &formats),
            super::NumberFormat::DollarTwoDecimals
        ));
        assert_eq!(
            super::format_numeric_value(111.0, "111", super::NumberFormat::DollarTwoDecimals),
            "$111.00"
        );
        assert_eq!(
            super::format_numeric_value(0.0, "0", super::NumberFormat::DollarTwoDecimals),
            "$0.00"
        );
    }

    #[test]
    fn recognizes_builtin_percentage_formats() {
        assert!(matches!(
            parse_number_format(Some("9"), &HashMap::new()),
            super::NumberFormat::PercentageZeroDecimals
        ));
        assert!(matches!(
            parse_number_format(Some("10"), &HashMap::new()),
            super::NumberFormat::PercentageTwoDecimals
        ));
        assert_eq!(
            super::format_numeric_value(0.06, "0.06", super::NumberFormat::PercentageTwoDecimals),
            "6.00%"
        );
    }

    #[test]
    fn reads_fit_to_width_scale_and_margins() {
        let page_setup = read_page_setup(
            r#"<worksheet><sheetPr><pageSetUpPr fitToPage="1"/></sheetPr><pageMargins left="0.25" right="0.5" top="0.75" bottom="1"/><pageSetup paperSize="9" scale="74" fitToHeight="0" orientation="landscape"/></worksheet>"#,
        )
        .expect("worksheet XML is valid");

        assert!(page_setup.fit_to_width);
        assert!((page_setup.print_scale - 0.74).abs() < 0.001);
        assert!(!page_setup.fit_to_height);
        assert_eq!(page_setup.margin_left, 18.0);
        assert_eq!(page_setup.margin_right, 36.0);
        assert_eq!(page_setup.margin_top, 54.0);
        assert_eq!(page_setup.margin_bottom, 72.0);
    }

    #[test]
    fn defaults_fit_to_page_to_one_page_tall() {
        let page_setup = read_page_setup(
            r#"<worksheet><sheetPr><pageSetUpPr fitToPage="1"/></sheetPr><pageSetup scale="70"/></worksheet>"#,
        )
        .expect("worksheet XML is valid");

        assert!(page_setup.fit_to_width);
        assert!(page_setup.fit_to_height);
    }

    #[test]
    fn ignores_printer_page_size_for_width_only_fit() {
        let page_setup = read_page_setup(
            r#"<worksheet><sheetPr><pageSetUpPr fitToPage="1"/></sheetPr><pageSetup fitToHeight="0"/></worksheet>"#,
        )
        .expect("worksheet XML is valid");

        assert!(!super::should_use_printer_page_size(page_setup));
    }

    #[test]
    fn formats_excel_date_serial_as_month_day_year() {
        assert_eq!(format_excel_date(46_104.0), "3/23/2026");
    }

    #[test]
    fn applies_equal_conditional_font_color() {
        let xml = r#"<worksheet><conditionalFormatting sqref="A1:B1"><cfRule type="cellIs" dxfId="0" operator="equal"><formula>"✔"</formula></cfRule></conditionalFormatting></worksheet>"#;
        let mut rows = vec![RowData {
            index: 0,
            height: 15.0,
            cells: vec![
                CellData {
                    text: "✔".to_owned(),
                    ..CellData::default()
                },
                CellData {
                    text: "✖".to_owned(),
                    ..CellData::default()
                },
            ],
        }];
        let styles = XlsxStyles {
            differential_fonts: vec![DifferentialFontStyle {
                color: Some(PdfColor::new(0.25, 0.46, 0.17)),
                ..DifferentialFontStyle::default()
            }],
            ..XlsxStyles::default()
        };

        apply_sheet_conditional_formats(xml, &mut rows, &styles)
            .expect("conditional formatting XML is valid");

        assert_eq!(
            rows[0].cells[0].style.font_color,
            PdfColor::new(0.25, 0.46, 0.17)
        );
        assert_eq!(rows[0].cells[1].style.font_color, PdfColor::BLACK);
    }

    #[test]
    fn applies_relative_text_equality_conditional_style() {
        let xml = r#"<worksheet><conditionalFormatting sqref="A6:C7"><cfRule type="expression" dxfId="0"><formula>$A6="yes"</formula></cfRule></conditionalFormatting></worksheet>"#;
        let mut rows = vec![
            RowData {
                index: 5,
                height: 15.0,
                cells: vec![CellData {
                    text: "Yes".to_owned(),
                    ..CellData::default()
                }],
            },
            RowData {
                index: 6,
                height: 15.0,
                cells: vec![CellData {
                    text: "No".to_owned(),
                    ..CellData::default()
                }],
            },
        ];
        let color = PdfColor::new(0.75, 0.2, 0.0);
        let fill = PdfColor::new(0.9, 0.9, 0.85);
        let styles = XlsxStyles {
            differential_fonts: vec![DifferentialFontStyle {
                bold: Some(true),
                strike: Some(true),
                color: Some(color),
                ..DifferentialFontStyle::default()
            }],
            differential_fills: vec![Some(fill)],
            ..XlsxStyles::default()
        };

        apply_sheet_conditional_formats(xml, &mut rows, &styles)
            .expect("conditional formatting XML is valid");

        assert!(rows[0].cells[2].style.bold);
        assert!(rows[0].cells[2].style.strike);
        assert_eq!(rows[0].cells[2].style.font_color, color);
        assert_eq!(rows[0].cells[2].style.fill_color, Some(fill));
        assert!(!rows[1].cells[0].style.bold);
    }

    #[test]
    fn applies_table_font_style_only_within_table_range() {
        let mut rows = vec![RowData {
            index: 4,
            height: 15.0,
            cells: vec![CellData::default(); 4],
        }];
        let color = PdfColor::new(0.12, 0.32, 0.58);

        apply_table_font_layer(
            &mut rows,
            1,
            2,
            4,
            4,
            DifferentialFontStyle {
                bold: Some(true),
                italic: Some(false),
                strike: None,
                color: Some(color),
            },
        );

        assert!(!rows[0].cells[0].style.bold);
        assert_eq!(rows[0].cells[0].style.font_color, PdfColor::BLACK);
        assert!(rows[0].cells[1].style.bold);
        assert!(!rows[0].cells[1].style.italic);
        assert_eq!(rows[0].cells[1].style.font_color, color);
        assert!(rows[0].cells[2].style.bold);
        assert_eq!(rows[0].cells[3].style.font_color, PdfColor::BLACK);
    }

    #[test]
    fn applies_table_fill_style_only_within_table_range() {
        let fill = PdfColor::new(0.78, 0.81, 0.81);
        let mut rows = vec![RowData {
            index: 4,
            height: 15.0,
            cells: vec![CellData::default(); 4],
        }];

        apply_table_fill_layer(&mut rows, 1, 2, 4, 4, fill);

        assert_eq!(rows[0].cells[0].style.fill_color, None);
        assert_eq!(rows[0].cells[1].style.fill_color, Some(fill));
        assert_eq!(rows[0].cells[2].style.fill_color, Some(fill));
        assert_eq!(rows[0].cells[3].style.fill_color, None);
    }

    #[test]
    fn applies_saved_start_date_conditional_fill() {
        let xml = r#"<worksheet><conditionalFormatting sqref="A1:B2"><cfRule type="expression" dxfId="0"><formula>StartDate+0=TODAY()</formula></cfRule></conditionalFormatting></worksheet>"#;
        let mut rows = vec![RowData {
            index: 0,
            height: 15.0,
            cells: vec![CellData::default(), CellData::default()],
        }];
        let fill = PdfColor::new(0.82, 0.93, 0.85);
        let styles = XlsxStyles {
            differential_fills: vec![Some(fill)],
            ..XlsxStyles::default()
        };

        apply_sheet_conditional_formats(xml, &mut rows, &styles)
            .expect("conditional formatting XML is valid");

        assert_eq!(rows[0].cells[0].style.fill_color, Some(fill));
        assert_eq!(rows[0].cells[1].style.fill_color, Some(fill));
    }

    #[test]
    fn reads_horizontal_print_centering() {
        let page_setup = read_page_setup(
            r#"<worksheet><printOptions horizontalCentered="1"/><sheetData/></worksheet>"#,
        )
        .expect("worksheet XML is valid");

        assert!(page_setup.horizontal_centered);
    }

    #[test]
    fn applies_excel_minimum_top_margin_when_fitting_to_pages() {
        let page_setup = read_page_setup(
            r#"<worksheet><sheetPr><pageSetUpPr fitToPage="1"/></sheetPr><pageMargins top="0.05"/><pageSetup fitToHeight="0"/></worksheet>"#,
        )
        .expect("worksheet XML is valid");

        assert_eq!(page_setup.margin_top, 13.0);
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
    fn formats_excel_thousands_with_two_decimals() {
        assert_eq!(format_thousands_two_decimals(42_000.0), "42,000.00");
        assert_eq!(format_thousands_two_decimals(-8.5), "-8.50");
        assert_eq!(format_thousands_two_decimals(1.125), "1.13");
        assert_eq!(format_thousands_two_decimals(270.505), "270.51");
        assert_eq!(
            super::format_numeric_value(42_000.0, "42000", super::NumberFormat::DollarAccounting),
            "$ 42,000.00"
        );
        assert_eq!(
            super::format_numeric_value(0.0, "0", super::NumberFormat::DollarAccounting),
            "$ -"
        );
    }

    #[test]
    fn evaluates_empty_cached_sum_and_addition_formulas() {
        let xml = r#"<worksheet><sheetData>
            <row r="1"><c r="A1" t="n"><v>1000</v></c><c r="B1" t="n"><v>250</v></c></row>
            <row r="2"><c r="A2" t="n"><v>2000</v></c><c r="B2" t="n"><v>750</v></c></row>
            <row r="3"><c r="A3"><f>SUM(A1:A2)</f><v></v></c><c r="B3"><f>SUM(B1:B2)</f><v></v></c></row>
            <row r="4"><c r="A4"><f>A3+B3</f><v></v></c></row>
        </sheetData></worksheet>"#;
        let rows =
            read_sheet_rows(xml, &[], &XlsxStyles::default()).expect("worksheet XML is valid");

        assert_eq!(rows[2].cells[0].text, "3000");
        assert_eq!(rows[2].cells[1].text, "1000");
        assert_eq!(rows[3].cells[0].text, "4000");
        assert!(rows[3].cells[0].is_numeric);
    }

    #[test]
    fn recalculates_today_instead_of_using_stale_cached_value() {
        let date_style = CellStyle {
            number_format: super::NumberFormat::DateMonthDayYear,
            ..CellStyle::default()
        };
        let styles = XlsxStyles {
            cells: vec![date_style],
            ..XlsxStyles::default()
        };
        let xml = r#"<worksheet><sheetData><row r="1"><c r="A1" s="0"><f>TODAY()</f><v>1</v></c></row></sheetData></worksheet>"#;

        let rows = read_sheet_rows(xml, &[], &styles).expect("worksheet XML is valid");

        assert_eq!(
            rows[0].cells[0].text,
            format_excel_date(super::excel_today_serial())
        );
    }

    #[test]
    fn recognizes_cjk_display_fonts_as_emphasis() {
        assert!(font_name_is_emphasis(Some("黑体")));
        assert!(font_name_is_emphasis(Some("方正小标宋_GBK")));
        assert!(!font_name_is_emphasis(Some("仿宋_GB2312")));
    }

    #[test]
    fn maps_franklin_gothic_medium_to_installed_font() {
        assert_eq!(
            super::xlsx_preferred_font(Some("Franklin Gothic Medium")),
            Some("framd")
        );
    }

    #[test]
    fn maps_invoice_fonts_to_installed_fonts() {
        assert_eq!(super::xlsx_preferred_font(Some("Garamond")), Some("gara"));
        assert_eq!(
            super::xlsx_preferred_font(Some("Tw Cen MT")),
            Some("tcm_____")
        );
    }

    #[test]
    fn wraps_explicit_and_width_constrained_cell_text() {
        let explicit = wrap_cell_text("Role\n(Rank)", 100.0, 10.0, false, false);
        let constrained = wrap_cell_text("ABCD", 12.0, 10.0, false, false);
        let words = wrap_cell_text("alpha beta", 30.0, 10.0, false, false);

        assert_eq!(explicit, vec!["Role", "(Rank)"]);
        assert!(constrained.len() > 1);
        assert_eq!(constrained.concat(), "ABCD");
        assert_eq!(words, vec!["alpha", "beta"]);
    }

    #[test]
    fn preserves_custom_row_height_and_bold_cell_style() {
        let xml = r#"<worksheet><sheetData><row r="1" ht="68"><c r="D1" s="1" t="inlineStr"><is><t>Product Name</t></is></c></row></sheetData></worksheet>"#;
        let styles = XlsxStyles {
            cells: vec![
                CellStyle::default(),
                CellStyle {
                    bold: true,
                    indent: 1.0,
                    ..CellStyle::default()
                },
            ],
            ..XlsxStyles::default()
        };
        let rows = read_sheet_rows(xml, &[], &styles).expect("worksheet XML is valid");

        assert_eq!(rows[0].height, 68.0);
        assert!(rows[0].cells[3].style.bold);
        assert_eq!(rows[0].cells[3].style.indent, 1.0);
    }

    #[test]
    fn preserves_hidden_row_values_without_layout_height() {
        let xml = r#"<worksheet><sheetData><row r="2" ht="24" hidden="1"><c r="A2" t="n"><v>42</v></c></row></sheetData></worksheet>"#;
        let rows =
            read_sheet_rows(xml, &[], &XlsxStyles::default()).expect("worksheet XML is valid");

        assert_eq!(rows.len(), 1);
        assert_eq!(rows[0].index, 1);
        assert_eq!(rows[0].height, 0.0);
        assert_eq!(rows[0].cells[0].text, "42");
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
    fn preserves_rows_with_visible_styles_on_empty_cells() {
        let xml =
            r#"<worksheet><sheetData><row r="4"><c r="A4" s="1"/></row></sheetData></worksheet>"#;
        let styles = XlsxStyles {
            cells: vec![
                CellStyle::default(),
                CellStyle {
                    fill_color: Some(PdfColor::new(1.0, 1.0, 0.0)),
                    fill_override: true,
                    ..CellStyle::default()
                },
            ],
            ..XlsxStyles::default()
        };

        let rows = read_sheet_rows(xml, &[], &styles).expect("worksheet XML is valid");

        assert_eq!(rows.len(), 1);
        assert_eq!(rows[0].index, 3);
        assert!(rows[0].cells[0].style.fill_color.is_some());
    }

    #[test]
    fn applies_column_styles_only_to_explicit_cells() {
        let xml = r#"<worksheet><cols><col min="1" max="3" style="1"/></cols><sheetData>
            <row r="1"><c r="C1" s="2" t="inlineStr"><is><t>value</t></is></c></row>
            <row r="3"><c r="C3" t="inlineStr"><is><t>value</t></is></c></row>
        </sheetData></worksheet>"#;
        let fill = PdfColor::new(0.9, 0.9, 0.9);
        let styles = XlsxStyles {
            cells: vec![
                CellStyle::default(),
                CellStyle {
                    fill_color: Some(fill),
                    fill_override: true,
                    ..CellStyle::default()
                },
                CellStyle {
                    bold: true,
                    ..CellStyle::default()
                },
            ],
            ..XlsxStyles::default()
        };

        let rows = read_sheet_rows(xml, &[], &styles).expect("worksheet XML is valid");

        assert_eq!(rows.len(), 3);
        assert_eq!(rows[1].index, 1);
        assert_eq!(rows[0].cells.len(), 3);
        assert_eq!(rows[0].cells[0].style.fill_color, None);
        assert_eq!(rows[0].cells[1].style.fill_color, None);
        assert_eq!(rows[0].cells[2].style.fill_color, None);
        assert!(rows[0].cells[2].style.bold);
        assert_eq!(rows[1].cells[0].style.fill_color, None);
        assert_eq!(rows[2].cells[2].style.fill_color, Some(fill));
    }

    #[test]
    fn trims_only_trailing_empty_rows_after_image_anchors() {
        let mut rows = vec![
            RowData {
                index: 3,
                height: 60.0,
                cells: Vec::new(),
            },
            RowData {
                index: 8,
                height: 60.0,
                cells: vec![super::CellData::default()],
            },
        ];
        let images = vec![SheetImage {
            data: SheetImageData::Jpeg(Vec::new()),
            pixel_width: 1,
            pixel_height: 1,
            col: 0,
            row: 3,
            col_offset: 0.0,
            row_offset: 0.0,
            width: 1.0,
            height: 1.0,
            foreground: false,
        }];

        trim_trailing_empty_rows(&mut rows, &images);

        assert_eq!(rows.len(), 1);
        assert_eq!(rows[0].index, 3);
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

        assert!((widths[0] - 78.75).abs() < 0.001);
        assert_eq!(widths.len(), 14);
        assert_eq!(groups, vec![(0, 5), (5, 11), (11, 14)]);
    }

    #[test]
    fn skips_style_only_horizontal_page_groups() {
        let sheet = SheetData {
            rows: vec![RowData {
                index: 0,
                height: 15.0,
                cells: vec![
                    CellData {
                        text: "Visible".to_owned(),
                        ..CellData::default()
                    },
                    CellData::default(),
                ],
            }],
            images: Vec::new(),
            column_widths: vec![100.0, 100.0],
            merges: Vec::new(),
            row_breaks: Vec::new(),
            default_row_height: 15.0,
            print_title_rows: None,
            page_setup: SheetPageSetup::default(),
        };

        assert!(column_group_has_content(&sheet, 0, 1));
        assert!(!column_group_has_content(&sheet, 1, 2));
    }

    #[test]
    fn automatic_breaks_restart_after_manual_breaks() {
        let sheet = SheetData {
            rows: (0..4)
                .map(|index| RowData {
                    index,
                    height: 25.0,
                    cells: Vec::new(),
                })
                .collect(),
            images: Vec::new(),
            column_widths: Vec::new(),
            merges: Vec::new(),
            row_breaks: vec![2],
            default_row_height: 25.0,
            print_title_rows: Some((0, 0)),
            page_setup: SheetPageSetup {
                page_size: PageSize::new(100.0, 100.0).expect("valid page size"),
                margin_top: 10.0,
                margin_bottom: 10.0,
                ..SheetPageSetup::default()
            },
        };

        assert!(super::automatic_row_breaks(&sheet, sheet.page_setup.page_size, 1.0).is_empty());
    }

    #[test]
    fn uses_excel_digit_width_for_palatino_linotype() {
        assert!(
            (super::excel_max_digit_width(Some("Palatino Linotype"), 11.0) - 7.062).abs() < 0.01
        );
    }

    #[test]
    fn uses_smaller_print_or_fit_to_width_scale() {
        assert_eq!(effective_content_scale(1000.0, 800.0, 0.74, true), 0.8);
        assert_eq!(effective_content_scale(1000.0, 700.0, 0.74, true), 0.7);
        assert_eq!(effective_content_scale(1000.0, 700.0, 0.74, false), 0.74);
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
    fn preserves_borders_from_merged_range_edges() {
        let mut top_left = CellData::default();
        top_left.style.borders.left = Some(PdfColor::BLACK);
        top_left.style.borders.top = Some(PdfColor::BLACK);
        let mut top_right = CellData::default();
        top_right.style.borders.right = Some(PdfColor::BLACK);
        let mut bottom_left = CellData::default();
        bottom_left.style.borders.bottom = Some(PdfColor::BLACK);
        let mut bottom_right = CellData::default();
        bottom_right.style.borders.right = Some(PdfColor::BLACK);
        let rows = vec![
            RowData {
                index: 0,
                height: 10.0,
                cells: vec![top_left, top_right],
            },
            RowData {
                index: 1,
                height: 10.0,
                cells: vec![bottom_left, bottom_right],
            },
        ];
        let borders = merged_cell_borders(
            &rows,
            &MergeRange {
                start_col: 0,
                end_col: 1,
                start_row: 0,
                end_row: 1,
            },
        );

        assert_eq!(borders.left, Some(PdfColor::BLACK));
        assert_eq!(borders.right, Some(PdfColor::BLACK));
        assert_eq!(borders.top, Some(PdfColor::BLACK));
        assert_eq!(borders.bottom, Some(PdfColor::BLACK));
    }

    #[test]
    fn resolves_indexed_and_tinted_theme_colors() {
        assert_eq!(indexed_color(9), Some(PdfColor::new(1.0, 1.0, 1.0)));
        assert_eq!(indexed_color(10), Some(PdfColor::new(1.0, 0.0, 0.0)));
        let scheme = [
            PdfColor::BLACK,
            PdfColor::new(1.0, 1.0, 1.0),
            PdfColor::new(0.1, 0.1, 0.1),
            PdfColor::new(0.9, 0.9, 0.9),
        ];
        let theme = spreadsheet_theme_colors(&scheme);
        assert_eq!(theme[0], scheme[1]);
        assert_eq!(theme[1], scheme[0]);
        assert_eq!(theme[2], scheme[3]);
        assert_eq!(theme[3], scheme[2]);
        let tinted = apply_color_tint(PdfColor::new(0.647, 0.647, 0.647), 0.8);
        assert!((tinted.r - 0.9294).abs() < 0.0001);
        assert!((tinted.g - 0.9294).abs() < 0.0001);
        assert!((tinted.b - 0.9294).abs() < 0.0001);
        let accent = apply_color_tint(
            PdfColor::new(247.0 / 255.0, 245.0 / 255.0, 228.0 / 255.0),
            -0.7499924,
        );
        assert!((accent.r - 92.0 / 255.0).abs() < 0.002);
        assert!((accent.g - 85.0 / 255.0).abs() < 0.002);
        assert!((accent.b - 27.0 / 255.0).abs() < 0.002);
        let light_accent = apply_color_tint(
            PdfColor::new(31.0 / 255.0, 72.0 / 255.0, 124.0 / 255.0),
            0.7999817,
        );
        assert!((light_accent.r - 198.0 / 255.0).abs() < 0.002);
        assert!((light_accent.g - 217.0 / 255.0).abs() < 0.002);
        assert!((light_accent.b - 241.0 / 255.0).abs() < 0.002);
        assert_eq!(
            apply_color_tint(PdfColor::WHITE, -0.05),
            PdfColor::new(0.945, 0.949, 0.945)
        );
        assert_eq!(
            apply_color_tint(PdfColor::WHITE, -0.15),
            PdfColor::new(0.85, 0.851, 0.85)
        );
    }

    #[test]
    fn preserves_individual_border_sides() {
        let xml = roxmltree::Document::parse(
            r#"<border><left/><right/><top style="thin"><color rgb="FF112233"/></top><bottom/></border>"#,
        )
        .expect("border XML is valid");

        let borders = parse_cell_borders(xml.root_element(), &[]);

        assert!(borders.left.is_none());
        assert!(borders.right.is_none());
        assert!(borders.bottom.is_none());
        assert_eq!(
            borders.top,
            Some(PdfColor::new(17.0 / 255.0, 34.0 / 255.0, 51.0 / 255.0))
        );
    }

    #[test]
    fn resolves_themed_border_colors() {
        let xml = roxmltree::Document::parse(
            r#"<border><left style="thin"><color theme="0" tint="-0.05"/></left></border>"#,
        )
        .expect("border XML is valid");
        let borders = parse_cell_borders(xml.root_element(), &[PdfColor::WHITE]);

        assert_eq!(borders.left, Some(PdfColor::new(0.945, 0.949, 0.945)));
    }

    #[test]
    fn reads_table_internal_border_widths() {
        let xml = roxmltree::Document::parse(
            r#"<border><vertical style="thin"><color rgb="FF112233"/></vertical><horizontal style="medium"><color rgb="FF445566"/></horizontal></border>"#,
        )
        .expect("border XML is valid");

        let borders = super::parse_table_borders(xml.root_element(), &[]);

        assert_eq!(
            borders.vertical,
            Some((PdfColor::new(17.0 / 255.0, 34.0 / 255.0, 51.0 / 255.0), 0.5))
        );
        assert_eq!(
            borders.horizontal,
            Some((
                PdfColor::new(68.0 / 255.0, 85.0 / 255.0, 102.0 / 255.0),
                1.0
            ))
        );
    }

    #[test]
    fn reads_zero_based_manual_row_breaks() {
        let xml = r#"<worksheet><rowBreaks count="2" manualBreakCount="2"><brk id="18" max="16383" man="1"/><brk id="35" max="16383" man="1"/></rowBreaks></worksheet>"#;

        let row_breaks = read_row_breaks(xml).expect("worksheet XML is valid");

        assert_eq!(row_breaks, vec![18, 35]);
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

    #[test]
    fn distinguishes_horizontal_alignment_values() {
        assert_eq!(
            parse_horizontal_alignment(Some("left")),
            HorizontalAlignment::Left
        );
        assert_eq!(
            parse_horizontal_alignment(Some("center")),
            HorizontalAlignment::Center
        );
        assert_eq!(
            parse_horizontal_alignment(Some("right")),
            HorizontalAlignment::Right
        );
        assert_eq!(
            parse_horizontal_alignment(None),
            HorizontalAlignment::General
        );
    }

    #[test]
    fn detects_rtl_base_direction_for_general_alignment() {
        assert!(has_rtl_base_direction("كتاب برمجة"));
        assert!(has_rtl_base_direction("₪120 ספר קוד"));
        assert!(!has_rtl_base_direction("Programming Book"));
        assert!(!has_rtl_base_direction("50 SAR"));
    }
}
