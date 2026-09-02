use std::collections::HashMap;
use std::io::{Cursor, Read};

use zip::ZipArchive;

use crate::pdf::{styled_text_width_with_font, PdfColor, PdfDocument, PdfTextStyle};
use crate::{read_zip_text, ConversionOptions, PageSize, Result};

const PAGE_WIDTH: f32 = 595.28;
const PAGE_HEIGHT: f32 = 841.89;
const MARGIN: f32 = 54.0;
const BODY_FONT_SIZE: f32 = 11.0;
const LINE_HEIGHT: f32 = 16.0;
const TABLE_CELL_PADDING_HORIZONTAL: f32 = 5.4;
const TABLE_CELL_PADDING_VERTICAL: f32 = 1.0;
const TABLE_BORDER_WIDTH: f32 = 0.5;

#[derive(Debug, PartialEq)]
struct DocxDocument {
    blocks: Vec<DocxBlock>,
    page_size: PageSize,
    margins: DocxMargins,
}

#[derive(Debug, Clone, Copy, PartialEq)]
struct DocxMargins {
    top: f32,
    right: f32,
    bottom: f32,
    left: f32,
}

#[derive(Debug, PartialEq)]
enum DocxBlock {
    Paragraph(DocxParagraph),
    Table(DocxTable),
    Image(DocxImage),
    PageBreak,
}

#[derive(Debug, Clone, PartialEq)]
struct DocxParagraph {
    runs: Vec<DocxRun>,
    style_id: Option<String>,
    contextual_spacing: bool,
    alignment: TextAlignment,
    indent_left: f32,
    indent_right: f32,
    spacing_before: f32,
    spacing_after: f32,
    line_spacing: f32,
    fill: Option<PdfColor>,
    bottom_border: Option<DocxBorder>,
}

#[derive(Debug, Clone, PartialEq)]
struct DocxRun {
    text: String,
    font_size: f32,
    bold: bool,
    italic: bool,
    underline: bool,
    color: PdfColor,
}

#[derive(Debug, Clone, Copy, PartialEq)]
struct DocxBorder {
    color: PdfColor,
    width: f32,
    space: f32,
    is_double: bool,
}

#[derive(Debug, Clone, Default)]
struct DocxStyles {
    paragraph_defaults: ParagraphProperties,
    run_defaults: RunProperties,
    styles: HashMap<String, DocxStyle>,
}

#[derive(Debug, Clone, Default)]
struct DocxStyle {
    based_on: Option<String>,
    paragraph: ParagraphProperties,
    run: RunProperties,
}

#[derive(Debug, Clone, Default)]
struct ParagraphProperties {
    contextual_spacing: Option<bool>,
    alignment: Option<TextAlignment>,
    indent_left: Option<f32>,
    indent_right: Option<f32>,
    spacing_before: Option<f32>,
    spacing_after: Option<f32>,
    line_spacing: Option<f32>,
    fill: Option<PdfColor>,
    bottom_border: Option<DocxBorder>,
}

#[derive(Debug, Clone, Default)]
struct RunProperties {
    font_size: Option<f32>,
    bold: Option<bool>,
    italic: Option<bool>,
    underline: Option<bool>,
    color: Option<PdfColor>,
}

#[derive(Debug, PartialEq)]
struct DocxImage {
    data: DocxImageData,
    width: f32,
    height: f32,
    alignment: TextAlignment,
}

#[derive(Debug, PartialEq)]
enum DocxImageData {
    Jpeg {
        data: Vec<u8>,
        width: u16,
        height: u16,
    },
    Rgba {
        data: Vec<u8>,
        width: u16,
        height: u16,
    },
}

#[derive(Debug, PartialEq)]
struct DocxTable {
    column_widths: Vec<f32>,
    rows: Vec<DocxTableRow>,
}

#[derive(Debug, PartialEq)]
struct DocxTableRow {
    cells: Vec<DocxTableCell>,
}

#[derive(Debug, PartialEq)]
struct DocxTableCell {
    text: String,
    images: Vec<DocxImage>,
    width: Option<f32>,
    fill: Option<PdfColor>,
    font_size: f32,
    bold: bool,
    italic: bool,
    color: PdfColor,
    alignment: TextAlignment,
}

#[derive(Debug, Clone, Copy, PartialEq)]
enum TextAlignment {
    Left,
    Center,
    Right,
}

pub(crate) fn convert_docx_bytes(input: &[u8], options: &ConversionOptions) -> Result<Vec<u8>> {
    let document = read_docx_document(input)?;
    let mut doc = PdfDocument::new();
    render_docx(&mut doc, &document, options);
    Ok(doc.to_bytes())
}

fn read_docx_document(input: &[u8]) -> Result<DocxDocument> {
    let cursor = Cursor::new(input);
    let mut archive = ZipArchive::new(cursor)?;
    let Some(document_xml) = read_zip_text(&mut archive, "word/document.xml")? else {
        return Ok(empty_docx_document());
    };
    let styles = read_styles(&mut archive)?;
    let relationships = read_document_relationships(&mut archive)?;

    let xml = roxmltree::Document::parse(&document_xml)?;
    let Some(body) = xml.descendants().find(|node| node.has_tag_name("body")) else {
        return Ok(empty_docx_document());
    };
    let (page_size, margins) = read_page_layout(body);
    let mut blocks = Vec::new();

    for node in body.children().filter(|node| node.is_element()) {
        if node.has_tag_name("p") {
            read_paragraph(node, &mut blocks, &styles, &relationships, &mut archive)?;
        } else if node.has_tag_name("tbl") {
            blocks.push(DocxBlock::Table(read_table(
                node,
                &relationships,
                &mut archive,
            )?));
        }
    }

    if blocks.is_empty() {
        blocks.push(DocxBlock::Paragraph(plain_paragraph(
            "Empty DOCX document".to_owned(),
        )));
    }

    Ok(DocxDocument {
        blocks,
        page_size,
        margins,
    })
}

fn empty_docx_document() -> DocxDocument {
    DocxDocument {
        blocks: vec![DocxBlock::Paragraph(plain_paragraph(
            "Empty DOCX document".to_owned(),
        ))],
        page_size: PageSize {
            width: PAGE_WIDTH,
            height: PAGE_HEIGHT,
        },
        margins: DocxMargins {
            top: MARGIN,
            right: MARGIN,
            bottom: MARGIN,
            left: MARGIN,
        },
    }
}

fn plain_paragraph(text: String) -> DocxParagraph {
    DocxParagraph {
        runs: vec![DocxRun {
            text,
            font_size: BODY_FONT_SIZE,
            bold: false,
            italic: false,
            underline: false,
            color: PdfColor::BLACK,
        }],
        style_id: None,
        contextual_spacing: false,
        alignment: TextAlignment::Left,
        indent_left: 0.0,
        indent_right: 0.0,
        spacing_before: 0.0,
        spacing_after: LINE_HEIGHT * 0.35,
        line_spacing: 1.15,
        fill: None,
        bottom_border: None,
    }
}

fn read_page_layout(body: roxmltree::Node<'_, '_>) -> (PageSize, DocxMargins) {
    let section = body.children().find(|node| node.has_tag_name("sectPr"));
    let page_size_node = section.and_then(|node| child(node, "pgSz"));
    let margin_node = section.and_then(|node| child(node, "pgMar"));
    let page_size = PageSize {
        width: twips_attribute(page_size_node, "w").unwrap_or(PAGE_WIDTH),
        height: twips_attribute(page_size_node, "h").unwrap_or(PAGE_HEIGHT),
    };
    let margins = DocxMargins {
        top: twips_attribute(margin_node, "top").unwrap_or(MARGIN),
        right: twips_attribute(margin_node, "right").unwrap_or(MARGIN),
        bottom: twips_attribute(margin_node, "bottom").unwrap_or(MARGIN),
        left: twips_attribute(margin_node, "left").unwrap_or(MARGIN),
    };
    (page_size, margins)
}

fn read_styles(archive: &mut ZipArchive<Cursor<&[u8]>>) -> Result<DocxStyles> {
    let Some(styles_xml) = read_zip_text(archive, "word/styles.xml")? else {
        return Ok(DocxStyles::default());
    };
    let xml = roxmltree::Document::parse(&styles_xml)?;
    let defaults = xml
        .descendants()
        .find(|node| node.has_tag_name("docDefaults"));
    let paragraph_defaults = defaults
        .and_then(|node| node.descendants().find(|child| child.has_tag_name("pPr")))
        .map(read_paragraph_properties)
        .unwrap_or_default();
    let run_defaults = defaults
        .and_then(|node| node.descendants().find(|child| child.has_tag_name("rPr")))
        .map(read_run_properties)
        .unwrap_or_default();
    let styles = xml
        .descendants()
        .filter(|node| node.has_tag_name("style"))
        .filter_map(|node| {
            let id = attribute_by_local_name(node, "styleId")?;
            Some((
                id.to_owned(),
                DocxStyle {
                    based_on: child(node, "basedOn")
                        .and_then(|value| attribute_by_local_name(value, "val"))
                        .map(str::to_owned),
                    paragraph: child(node, "pPr")
                        .map(read_paragraph_properties)
                        .unwrap_or_default(),
                    run: child(node, "rPr")
                        .map(read_run_properties)
                        .unwrap_or_default(),
                },
            ))
        })
        .collect();
    Ok(DocxStyles {
        paragraph_defaults,
        run_defaults,
        styles,
    })
}

fn read_paragraph_properties(properties: roxmltree::Node<'_, '_>) -> ParagraphProperties {
    let spacing = child(properties, "spacing");
    let indentation = child(properties, "ind");
    ParagraphProperties {
        contextual_spacing: child(properties, "contextualSpacing").map(property_enabled),
        alignment: child(properties, "jc")
            .and_then(|node| attribute_by_local_name(node, "val"))
            .map(parse_alignment),
        indent_left: indentation
            .and_then(|node| numeric_attribute(node, "left"))
            .map(|twips| twips / 20.0),
        indent_right: indentation
            .and_then(|node| numeric_attribute(node, "right"))
            .map(|twips| twips / 20.0),
        spacing_before: spacing
            .and_then(|node| numeric_attribute(node, "before"))
            .map(|twips| twips / 20.0),
        spacing_after: spacing
            .and_then(|node| numeric_attribute(node, "after"))
            .map(|twips| twips / 20.0),
        line_spacing: spacing
            .and_then(|node| numeric_attribute(node, "line"))
            .map(|line| line / 240.0),
        fill: child(properties, "shd")
            .and_then(|node| attribute_by_local_name(node, "fill"))
            .and_then(parse_hex_color),
        bottom_border: child(properties, "pBdr")
            .and_then(|node| child(node, "bottom"))
            .and_then(read_border),
    }
}

fn read_run_properties(properties: roxmltree::Node<'_, '_>) -> RunProperties {
    RunProperties {
        font_size: child(properties, "sz")
            .and_then(|node| numeric_attribute(node, "val"))
            .map(|half_points| half_points / 2.0),
        bold: child(properties, "b").map(property_enabled),
        italic: child(properties, "i").map(property_enabled),
        underline: child(properties, "u").map(property_enabled),
        color: child(properties, "color")
            .and_then(|node| attribute_by_local_name(node, "val"))
            .and_then(parse_hex_color),
    }
}

fn read_border(border: roxmltree::Node<'_, '_>) -> Option<DocxBorder> {
    let style = attribute_by_local_name(border, "val")?;
    if matches!(style, "nil" | "none") {
        return None;
    }
    Some(DocxBorder {
        color: attribute_by_local_name(border, "color")
            .and_then(parse_hex_color)
            .unwrap_or(PdfColor::BLACK),
        width: numeric_attribute(border, "sz").unwrap_or(4.0) / 8.0,
        space: numeric_attribute(border, "space").unwrap_or(0.0),
        is_double: style == "double",
    })
}

fn parse_alignment(value: &str) -> TextAlignment {
    match value {
        "center" => TextAlignment::Center,
        "right" | "end" => TextAlignment::Right,
        _ => TextAlignment::Left,
    }
}

fn property_enabled(node: roxmltree::Node<'_, '_>) -> bool {
    !matches!(
        attribute_by_local_name(node, "val"),
        Some("0" | "false" | "off" | "none")
    )
}

fn merge_paragraph_properties(target: &mut ParagraphProperties, source: &ParagraphProperties) {
    if source.contextual_spacing.is_some() {
        target.contextual_spacing = source.contextual_spacing;
    }
    if source.alignment.is_some() {
        target.alignment = source.alignment;
    }
    if source.indent_left.is_some() {
        target.indent_left = source.indent_left;
    }
    if source.indent_right.is_some() {
        target.indent_right = source.indent_right;
    }
    if source.spacing_before.is_some() {
        target.spacing_before = source.spacing_before;
    }
    if source.spacing_after.is_some() {
        target.spacing_after = source.spacing_after;
    }
    if source.line_spacing.is_some() {
        target.line_spacing = source.line_spacing;
    }
    if source.fill.is_some() {
        target.fill = source.fill;
    }
    if source.bottom_border.is_some() {
        target.bottom_border = source.bottom_border;
    }
}

fn merge_run_properties(target: &mut RunProperties, source: &RunProperties) {
    if source.font_size.is_some() {
        target.font_size = source.font_size;
    }
    if source.bold.is_some() {
        target.bold = source.bold;
    }
    if source.italic.is_some() {
        target.italic = source.italic;
    }
    if source.underline.is_some() {
        target.underline = source.underline;
    }
    if source.color.is_some() {
        target.color = source.color;
    }
}

fn merge_style_chain(
    styles: &DocxStyles,
    style_id: &str,
    paragraph: &mut ParagraphProperties,
    run: &mut RunProperties,
    depth: usize,
) {
    if depth > 16 {
        return;
    }
    let Some(style) = styles.styles.get(style_id) else {
        return;
    };
    if let Some(parent) = &style.based_on {
        merge_style_chain(styles, parent, paragraph, run, depth + 1);
    }
    merge_paragraph_properties(paragraph, &style.paragraph);
    merge_run_properties(run, &style.run);
}

fn read_paragraph(
    paragraph: roxmltree::Node<'_, '_>,
    blocks: &mut Vec<DocxBlock>,
    styles: &DocxStyles,
    relationships: &HashMap<String, String>,
    archive: &mut ZipArchive<Cursor<&[u8]>>,
) -> Result<()> {
    if paragraph
        .descendants()
        .any(|node| node.has_tag_name("pageBreakBefore"))
    {
        blocks.push(DocxBlock::PageBreak);
    }

    let direct_properties = child(paragraph, "pPr")
        .map(read_paragraph_properties)
        .unwrap_or_default();
    let style_id = child(paragraph, "pPr")
        .and_then(|node| child(node, "pStyle"))
        .and_then(|node| attribute_by_local_name(node, "val"));
    let mut paragraph_properties = styles.paragraph_defaults.clone();
    let mut base_run_properties = styles.run_defaults.clone();
    if let Some(style_id) = style_id {
        merge_style_chain(
            styles,
            style_id,
            &mut paragraph_properties,
            &mut base_run_properties,
            0,
        );
    }
    merge_paragraph_properties(&mut paragraph_properties, &direct_properties);
    if let Some(properties) = child(paragraph, "pPr").and_then(|node| child(node, "rPr")) {
        merge_run_properties(&mut base_run_properties, &read_run_properties(properties));
    }

    let mut runs = Vec::new();
    let mut emitted_non_text = false;
    for run_node in paragraph
        .descendants()
        .filter(|node| node.has_tag_name("r"))
    {
        if let Some(drawing) = run_node
            .descendants()
            .find(|node| node.has_tag_name("drawing"))
        {
            push_paragraph_runs(blocks, &mut runs, style_id, &paragraph_properties);
            if let Some(image) = read_image(drawing, paragraph, relationships, archive)? {
                blocks.push(DocxBlock::Image(image));
                emitted_non_text = true;
            }
            continue;
        }

        let mut run_properties = base_run_properties.clone();
        if let Some(properties) = child(run_node, "rPr") {
            merge_run_properties(&mut run_properties, &read_run_properties(properties));
        }
        let mut text = String::new();
        for node in run_node.descendants() {
            if node.has_tag_name("t") {
                if let Some(value) = node.text() {
                    text.push_str(value);
                }
            } else if node.has_tag_name("tab") {
                text.push('\t');
            } else if node.has_tag_name("br") {
                if attribute_by_local_name(node, "type") == Some("page") {
                    if !text.is_empty() {
                        runs.push(create_run(std::mem::take(&mut text), &run_properties));
                    }
                    push_paragraph_runs(blocks, &mut runs, style_id, &paragraph_properties);
                    blocks.push(DocxBlock::PageBreak);
                    emitted_non_text = true;
                } else {
                    text.push('\n');
                }
            }
        }
        if !text.is_empty() {
            runs.push(create_run(text, &run_properties));
        }
    }
    if !runs.is_empty() || !emitted_non_text {
        blocks.push(DocxBlock::Paragraph(create_paragraph(
            std::mem::take(&mut runs),
            style_id,
            &paragraph_properties,
        )));
    }
    Ok(())
}

fn create_run(text: String, properties: &RunProperties) -> DocxRun {
    DocxRun {
        text,
        font_size: properties.font_size.unwrap_or(BODY_FONT_SIZE),
        bold: properties.bold.unwrap_or(false),
        italic: properties.italic.unwrap_or(false),
        underline: properties.underline.unwrap_or(false),
        color: properties.color.unwrap_or(PdfColor::BLACK),
    }
}

fn create_paragraph(
    runs: Vec<DocxRun>,
    style_id: Option<&str>,
    properties: &ParagraphProperties,
) -> DocxParagraph {
    let spacing_after = if runs.is_empty() {
        0.0
    } else {
        properties.spacing_after.unwrap_or(LINE_HEIGHT * 0.35)
    };
    DocxParagraph {
        runs,
        style_id: style_id.map(str::to_owned),
        contextual_spacing: properties.contextual_spacing.unwrap_or(false),
        alignment: properties.alignment.unwrap_or(TextAlignment::Left),
        indent_left: properties.indent_left.unwrap_or(0.0),
        indent_right: properties.indent_right.unwrap_or(0.0),
        spacing_before: properties.spacing_before.unwrap_or(0.0),
        spacing_after,
        line_spacing: properties.line_spacing.unwrap_or(1.15),
        fill: properties.fill,
        bottom_border: properties.bottom_border,
    }
}

fn push_paragraph_runs(
    blocks: &mut Vec<DocxBlock>,
    runs: &mut Vec<DocxRun>,
    style_id: Option<&str>,
    properties: &ParagraphProperties,
) {
    if !runs.is_empty() {
        blocks.push(DocxBlock::Paragraph(create_paragraph(
            std::mem::take(runs),
            style_id,
            properties,
        )));
    }
}

fn read_document_relationships(
    archive: &mut ZipArchive<Cursor<&[u8]>>,
) -> Result<HashMap<String, String>> {
    let Some(relationships_xml) = read_zip_text(archive, "word/_rels/document.xml.rels")? else {
        return Ok(HashMap::new());
    };
    let xml = roxmltree::Document::parse(&relationships_xml)?;
    Ok(xml
        .descendants()
        .filter(|node| node.has_tag_name("Relationship"))
        .filter_map(|node| {
            Some((
                node.attribute("Id")?.to_owned(),
                node.attribute("Target")?.to_owned(),
            ))
        })
        .collect())
}

fn read_image(
    drawing: roxmltree::Node<'_, '_>,
    paragraph: roxmltree::Node<'_, '_>,
    relationships: &HashMap<String, String>,
    archive: &mut ZipArchive<Cursor<&[u8]>>,
) -> Result<Option<DocxImage>> {
    let Some(container) = drawing
        .descendants()
        .find(|node| node.has_tag_name("inline"))
    else {
        return Ok(None);
    };
    let Some(relationship_id) = container
        .descendants()
        .find(|node| node.has_tag_name("blip"))
        .and_then(|node| attribute_by_local_name(node, "embed"))
    else {
        return Ok(None);
    };
    let Some(target) = relationships.get(relationship_id) else {
        return Ok(None);
    };
    let image_path = resolve_part_target("word/document.xml", target);
    let Ok(mut entry) = archive.by_name(&image_path) else {
        return Ok(None);
    };
    let mut bytes = Vec::with_capacity(entry.size() as usize);
    entry.read_to_end(&mut bytes)?;
    drop(entry);

    let (data, pixel_width, pixel_height) = if let Some((width, height)) = jpeg_dimensions(&bytes) {
        (
            DocxImageData::Jpeg {
                data: bytes,
                width,
                height,
            },
            width,
            height,
        )
    } else {
        let Ok(decoded) = image::load_from_memory(&bytes) else {
            return Ok(None);
        };
        let width = u16::try_from(decoded.width()).unwrap_or(u16::MAX);
        let height = u16::try_from(decoded.height()).unwrap_or(u16::MAX);
        (
            DocxImageData::Rgba {
                data: decoded.into_rgba8().into_raw(),
                width,
                height,
            },
            width,
            height,
        )
    };
    let extent = child(container, "extent");
    let width = extent
        .and_then(|node| numeric_attribute(node, "cx"))
        .map(|emu| emu / 12_700.0)
        .unwrap_or(pixel_width as f32 * 0.75);
    let height = extent
        .and_then(|node| numeric_attribute(node, "cy"))
        .map(|emu| emu / 12_700.0)
        .unwrap_or(pixel_height as f32 * 0.75);

    Ok(Some(DocxImage {
        data,
        width,
        height,
        alignment: paragraph_alignment(paragraph),
    }))
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
    let normalized = combined.replace('\\', "/");
    let mut segments = Vec::new();
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
        if index >= data.len() {
            break;
        }
        let marker = data[index];
        index += 1;
        if matches!(marker, 0xd8 | 0xd9) {
            continue;
        }
        if index + 1 >= data.len() {
            break;
        }
        let segment_length = u16::from_be_bytes([data[index], data[index + 1]]) as usize;
        if segment_length < 2 || index + segment_length > data.len() {
            break;
        }
        if matches!(marker, 0xc0..=0xc3 | 0xc5..=0xc7 | 0xc9..=0xcb | 0xcd..=0xcf)
            && segment_length >= 7
        {
            let height = u16::from_be_bytes([data[index + 3], data[index + 4]]);
            let width = u16::from_be_bytes([data[index + 5], data[index + 6]]);
            return Some((width, height));
        }
        index += segment_length;
    }
    None
}

fn read_table(
    table: roxmltree::Node<'_, '_>,
    relationships: &HashMap<String, String>,
    archive: &mut ZipArchive<Cursor<&[u8]>>,
) -> Result<DocxTable> {
    let column_widths = child(table, "tblGrid")
        .map(|grid| {
            grid.children()
                .filter(|node| node.has_tag_name("gridCol"))
                .filter_map(|node| twips_attribute(Some(node), "w"))
                .collect()
        })
        .unwrap_or_default();
    let mut rows = Vec::new();
    for row in table.children().filter(|node| node.has_tag_name("tr")) {
        let mut cells = Vec::new();
        for cell in row.children().filter(|node| node.has_tag_name("tc")) {
            cells.push(read_table_cell(cell, relationships, archive)?);
        }
        rows.push(DocxTableRow { cells });
    }
    Ok(DocxTable {
        column_widths,
        rows,
    })
}

fn read_table_cell(
    cell: roxmltree::Node<'_, '_>,
    relationships: &HashMap<String, String>,
    archive: &mut ZipArchive<Cursor<&[u8]>>,
) -> Result<DocxTableCell> {
    let properties = child(cell, "tcPr");
    let paragraphs: Vec<_> = cell
        .children()
        .filter(|node| node.has_tag_name("p"))
        .collect();
    let text = paragraphs
        .iter()
        .map(|paragraph| paragraph_text(*paragraph))
        .filter(|text| !text.is_empty())
        .collect::<Vec<_>>()
        .join("\n");
    let first_paragraph = paragraphs.first().copied();
    let first_run_properties = first_paragraph.and_then(|paragraph| {
        paragraph
            .descendants()
            .find(|node| node.has_tag_name("rPr"))
    });
    let mut images = Vec::new();
    for paragraph in &paragraphs {
        for drawing in paragraph
            .descendants()
            .filter(|node| node.has_tag_name("drawing"))
        {
            if let Some(image) = read_image(drawing, *paragraph, relationships, archive)? {
                images.push(image);
            }
        }
    }

    Ok(DocxTableCell {
        text,
        images,
        width: properties
            .and_then(|node| child(node, "tcW"))
            .and_then(|node| twips_attribute(Some(node), "w")),
        fill: properties
            .and_then(|node| child(node, "shd"))
            .and_then(|node| attribute_by_local_name(node, "fill"))
            .and_then(parse_hex_color),
        font_size: first_run_properties
            .and_then(|node| child(node, "sz"))
            .and_then(|node| numeric_attribute(node, "val"))
            .map(|half_points| half_points / 2.0)
            .unwrap_or(BODY_FONT_SIZE),
        bold: first_run_properties.is_some_and(|node| enabled_property(node, "b")),
        italic: first_run_properties.is_some_and(|node| enabled_property(node, "i")),
        color: first_run_properties
            .and_then(|node| child(node, "color"))
            .and_then(|node| attribute_by_local_name(node, "val"))
            .and_then(parse_hex_color)
            .unwrap_or(PdfColor::BLACK),
        alignment: first_paragraph
            .and_then(|node| node.descendants().find(|child| child.has_tag_name("jc")))
            .and_then(|node| attribute_by_local_name(node, "val"))
            .map(|value| match value {
                "center" => TextAlignment::Center,
                "right" | "end" => TextAlignment::Right,
                _ => TextAlignment::Left,
            })
            .unwrap_or(TextAlignment::Left),
    })
}

fn paragraph_text(paragraph: roxmltree::Node<'_, '_>) -> String {
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
    text
}

fn paragraph_alignment(paragraph: roxmltree::Node<'_, '_>) -> TextAlignment {
    paragraph
        .descendants()
        .find(|node| node.has_tag_name("jc"))
        .and_then(|node| attribute_by_local_name(node, "val"))
        .map(|value| match value {
            "center" => TextAlignment::Center,
            "right" | "end" => TextAlignment::Right,
            _ => TextAlignment::Left,
        })
        .unwrap_or(TextAlignment::Left)
}

fn child<'a, 'input>(
    node: roxmltree::Node<'a, 'input>,
    local_name: &str,
) -> Option<roxmltree::Node<'a, 'input>> {
    node.children().find(|child| child.has_tag_name(local_name))
}

fn twips_attribute(node: Option<roxmltree::Node<'_, '_>>, name: &str) -> Option<f32> {
    node.and_then(|node| numeric_attribute(node, name))
        .map(|twips| twips / 20.0)
}

fn numeric_attribute(node: roxmltree::Node<'_, '_>, name: &str) -> Option<f32> {
    attribute_by_local_name(node, name)?.parse().ok()
}

fn enabled_property(properties: roxmltree::Node<'_, '_>, name: &str) -> bool {
    child(properties, name).is_some_and(|node| {
        !matches!(
            attribute_by_local_name(node, "val"),
            Some("0" | "false" | "off")
        )
    })
}

fn parse_hex_color(value: &str) -> Option<PdfColor> {
    if value.len() != 6 || value.eq_ignore_ascii_case("auto") {
        return None;
    }
    let red = u8::from_str_radix(&value[0..2], 16).ok()?;
    let green = u8::from_str_radix(&value[2..4], 16).ok()?;
    let blue = u8::from_str_radix(&value[4..6], 16).ok()?;
    Some(PdfColor::new(
        red as f32 / 255.0,
        green as f32 / 255.0,
        blue as f32 / 255.0,
    ))
}

fn attribute_by_local_name<'a, 'input>(
    node: roxmltree::Node<'a, 'input>,
    local_name: &str,
) -> Option<&'a str> {
    node.attributes()
        .find(|attribute| attribute.name() == local_name)
        .map(|attribute| attribute.value())
}

fn render_docx(doc: &mut PdfDocument, document: &DocxDocument, options: &ConversionOptions) {
    let mut page_index = doc.pages().len();
    let page_size = options.page_size.unwrap_or(document.page_size);
    let margins = document.margins;
    doc.add_page(page_size.width, page_size.height);
    let mut y = page_size.height - margins.top - BODY_FONT_SIZE;
    let max_width = page_size.width - margins.left - margins.right;

    for (block_index, block) in document.blocks.iter().enumerate() {
        match block {
            DocxBlock::PageBreak => {
                page_index = doc.pages().len();
                doc.add_page(page_size.width, page_size.height);
                y = page_size.height - margins.top - BODY_FONT_SIZE;
            }
            DocxBlock::Paragraph(paragraph) => {
                if block_index + 1 == document.blocks.len()
                    && paragraph.runs.is_empty()
                    && paragraph.fill.is_none()
                    && paragraph.bottom_border.is_none()
                {
                    continue;
                }
                let previous_paragraph =
                    block_index
                        .checked_sub(1)
                        .and_then(|index| match document.blocks.get(index) {
                            Some(DocxBlock::Paragraph(previous)) => Some(previous),
                            _ => None,
                        });
                let previous_spacing_after = previous_paragraph.map_or(0.0, |previous| {
                    if previous.contextual_spacing && previous.style_id == paragraph.style_id {
                        0.0
                    } else {
                        previous.spacing_after
                    }
                });
                let spacing_before = if previous_paragraph.is_some() {
                    (paragraph.spacing_before - previous_spacing_after).max(0.0)
                } else {
                    paragraph.spacing_before
                };
                let spacing_after = if paragraph.contextual_spacing
                    && matches!(
                        document.blocks.get(block_index + 1),
                        Some(DocxBlock::Paragraph(next)) if next.style_id == paragraph.style_id
                    ) {
                    0.0
                } else {
                    paragraph.spacing_after
                };
                render_paragraph(
                    doc,
                    paragraph,
                    (spacing_before, spacing_after),
                    page_size,
                    margins,
                    &mut page_index,
                    &mut y,
                );
            }
            DocxBlock::Table(table) => render_table(
                doc,
                table,
                page_size,
                margins,
                max_width,
                &mut page_index,
                &mut y,
            ),
            DocxBlock::Image(image) => render_image(
                doc,
                image,
                page_size,
                margins,
                max_width,
                &mut page_index,
                &mut y,
            ),
        }
    }
}

fn render_paragraph(
    doc: &mut PdfDocument,
    paragraph: &DocxParagraph,
    spacing: (f32, f32),
    page_size: PageSize,
    margins: DocxMargins,
    page_index: &mut usize,
    y: &mut f32,
) {
    let (spacing_before, spacing_after) = spacing;
    let font_size = paragraph
        .runs
        .iter()
        .map(|run| run.font_size)
        .fold(BODY_FONT_SIZE, f32::max);
    let line_height = font_size * 1.18 * paragraph.line_spacing;
    let available_width = (page_size.width
        - margins.left
        - margins.right
        - paragraph.indent_left
        - paragraph.indent_right)
        .max(1.0);
    let lines = wrap_paragraph_runs(&paragraph.runs, available_width);
    let line_count = lines.len().max(1);
    let content_height = line_count as f32 * line_height;
    let total_height = spacing_before + content_height + spacing_after;
    let mut top = *y + BODY_FONT_SIZE;
    if top - total_height < margins.bottom {
        *page_index = doc.pages().len();
        doc.add_page(page_size.width, page_size.height);
        top = page_size.height - margins.top;
    }
    top -= spacing_before;
    let left = margins.left + paragraph.indent_left;
    let bottom = top - content_height;
    if let Some(fill) = paragraph.fill {
        doc.page_mut(*page_index)
            .expect("page index is valid")
            .add_rect(left, bottom, available_width, content_height, fill);
    }

    for (line_index, line) in lines.iter().enumerate() {
        let line_width: f32 = line.iter().map(|run| run_width(run, &run.text)).sum();
        let mut x = match paragraph.alignment {
            TextAlignment::Left => left,
            TextAlignment::Center => left + (available_width - line_width) / 2.0,
            TextAlignment::Right => left + available_width - line_width,
        };
        let baseline = top - font_size - line_index as f32 * line_height;
        for run in line {
            let width = run_width(run, &run.text);
            let page = doc.page_mut(*page_index).expect("page index is valid");
            page.add_styled_text(
                &run.text,
                x,
                baseline,
                run.font_size,
                PdfTextStyle {
                    color: run.color,
                    bold: run.bold,
                    italic: run.italic,
                    preferred_font: None,
                },
            );
            if run.underline && !run.text.trim().is_empty() {
                page.add_line(x, baseline - 1.5, x + width, baseline - 1.5, run.color, 0.5);
            }
            x += width;
        }
    }

    if let Some(border) = paragraph.bottom_border {
        let border_y = bottom - border.space;
        let page = doc.page_mut(*page_index).expect("page index is valid");
        page.add_line(
            left,
            border_y,
            left + available_width,
            border_y,
            border.color,
            border.width,
        );
        if border.is_double {
            page.add_line(
                left,
                border_y - border.width * 2.0,
                left + available_width,
                border_y - border.width * 2.0,
                border.color,
                border.width,
            );
        }
    }
    *y = top - content_height - spacing_after - BODY_FONT_SIZE;
}

fn wrap_paragraph_runs(runs: &[DocxRun], max_width: f32) -> Vec<Vec<DocxRun>> {
    let mut lines = vec![Vec::new()];
    let mut line_width = 0.0;
    for run in runs {
        let mut segment = String::new();
        for ch in run.text.chars() {
            if ch == '\n' {
                push_run_segment(lines.last_mut().expect("line exists"), run, &mut segment);
                lines.push(Vec::new());
                line_width = 0.0;
                continue;
            }
            let char_width = run_width(run, &ch.to_string());
            if line_width + char_width > max_width && line_width > 0.0 {
                push_run_segment(lines.last_mut().expect("line exists"), run, &mut segment);
                lines.push(Vec::new());
                line_width = 0.0;
            }
            segment.push(ch);
            line_width += char_width;
        }
        push_run_segment(lines.last_mut().expect("line exists"), run, &mut segment);
    }
    lines
}

fn push_run_segment(line: &mut Vec<DocxRun>, run: &DocxRun, segment: &mut String) {
    if segment.is_empty() {
        return;
    }
    let mut output = run.clone();
    output.text = std::mem::take(segment);
    line.push(output);
}

fn run_width(run: &DocxRun, text: &str) -> f32 {
    styled_text_width_with_font(text, run.font_size, run.bold, run.italic, None)
}

fn render_image(
    doc: &mut PdfDocument,
    image: &DocxImage,
    page_size: PageSize,
    margins: DocxMargins,
    max_width: f32,
    page_index: &mut usize,
    y: &mut f32,
) {
    let mut width = image.width;
    let mut height = image.height;
    if width > max_width {
        let scale = max_width / width;
        width *= scale;
        height *= scale;
    }
    let mut top = *y + BODY_FONT_SIZE;
    if top - height < margins.bottom {
        *page_index = doc.pages().len();
        doc.add_page(page_size.width, page_size.height);
        top = page_size.height - margins.top;
    }
    let x = match image.alignment {
        TextAlignment::Left => margins.left,
        TextAlignment::Center => margins.left + (max_width - width) / 2.0,
        TextAlignment::Right => page_size.width - margins.right - width,
    };
    let image_id = match &image.data {
        DocxImageData::Jpeg {
            data,
            width,
            height,
        } => doc.add_jpeg_image(data.clone(), *width, *height),
        DocxImageData::Rgba {
            data,
            width,
            height,
        } => doc.add_rgba_image(data.clone(), *width, *height),
    };
    doc.page_mut(*page_index)
        .expect("page index is valid")
        .add_image(image_id, x, top - height, width, height);
    *y = top - height - BODY_FONT_SIZE;
}

fn render_table(
    doc: &mut PdfDocument,
    table: &DocxTable,
    page_size: PageSize,
    margins: DocxMargins,
    max_width: f32,
    page_index: &mut usize,
    y: &mut f32,
) {
    let max_columns = table
        .rows
        .iter()
        .map(|row| row.cells.len())
        .max()
        .unwrap_or(0);
    if max_columns == 0 {
        return;
    }
    let mut widths = if table.column_widths.len() >= max_columns {
        table.column_widths[..max_columns].to_vec()
    } else {
        let inferred_widths: Vec<_> = (0..max_columns)
            .map(|index| {
                table
                    .rows
                    .iter()
                    .find_map(|row| row.cells.get(index).and_then(|cell| cell.width))
            })
            .collect();
        if inferred_widths.iter().all(Option::is_some) {
            inferred_widths.into_iter().flatten().collect()
        } else {
            vec![max_width / max_columns as f32; max_columns]
        }
    };
    let width_sum: f32 = widths.iter().sum();
    if width_sum > max_width && width_sum > 0.0 {
        let scale = max_width / width_sum;
        for width in &mut widths {
            *width *= scale;
        }
    }

    let mut row_top = *y + BODY_FONT_SIZE;
    for row in &table.rows {
        let wrapped_cells: Vec<_> = row
            .cells
            .iter()
            .enumerate()
            .map(|(index, cell)| {
                let width = widths.get(index).copied().unwrap_or(0.0);
                wrap_styled_text(
                    &cell.text,
                    (width - TABLE_CELL_PADDING_HORIZONTAL * 2.0).max(1.0),
                    cell.font_size,
                    cell.bold,
                    cell.italic,
                )
            })
            .collect();
        let row_height = row
            .cells
            .iter()
            .zip(&wrapped_cells)
            .map(|(cell, lines)| {
                let text_height = lines.len() as f32 * cell.font_size * 1.18;
                let image_height: f32 = cell
                    .images
                    .iter()
                    .map(|image| {
                        image_render_size(
                            image,
                            (widths
                                .get(
                                    row.cells
                                        .iter()
                                        .position(|candidate| std::ptr::eq(candidate, cell))
                                        .unwrap_or(0),
                                )
                                .copied()
                                .unwrap_or(0.0)
                                - TABLE_CELL_PADDING_HORIZONTAL * 2.0)
                                .max(1.0),
                        )
                        .1
                    })
                    .sum();
                text_height + image_height + TABLE_CELL_PADDING_VERTICAL * 2.0
            })
            .fold(0.0_f32, f32::max)
            .max(BODY_FONT_SIZE);

        if row_top - row_height < margins.bottom {
            *page_index = doc.pages().len();
            doc.add_page(page_size.width, page_size.height);
            row_top = page_size.height - margins.top;
        }

        let mut x = margins.left;
        for (index, cell) in row.cells.iter().enumerate() {
            let width = widths.get(index).copied().unwrap_or(0.0);
            let bottom = row_top - row_height;
            let page = doc.page_mut(*page_index).expect("page index is valid");
            if let Some(fill) = cell.fill {
                page.add_rect(x, bottom, width, row_height, fill);
            }
            let line_height = cell.font_size * 1.18;
            let mut text_y = row_top - TABLE_CELL_PADDING_VERTICAL - cell.font_size;
            for line in &wrapped_cells[index] {
                let text_width =
                    styled_text_width_with_font(line, cell.font_size, cell.bold, cell.italic, None);
                let text_x = match cell.alignment {
                    TextAlignment::Left => x + TABLE_CELL_PADDING_HORIZONTAL,
                    TextAlignment::Center => x + (width - text_width) / 2.0,
                    TextAlignment::Right => x + width - TABLE_CELL_PADDING_HORIZONTAL - text_width,
                };
                page.add_styled_text(
                    line,
                    text_x.max(x + TABLE_BORDER_WIDTH),
                    text_y,
                    cell.font_size,
                    PdfTextStyle {
                        color: cell.color,
                        bold: cell.bold,
                        italic: cell.italic,
                        preferred_font: None,
                    },
                );
                text_y -= line_height;
            }
            let mut image_top = row_top
                - TABLE_CELL_PADDING_VERTICAL
                - wrapped_cells[index].len() as f32 * line_height;
            for image in &cell.images {
                let (image_width, image_height) = image_render_size(
                    image,
                    (width - TABLE_CELL_PADDING_HORIZONTAL * 2.0).max(1.0),
                );
                let image_x = match image.alignment {
                    TextAlignment::Left => x + TABLE_CELL_PADDING_HORIZONTAL,
                    TextAlignment::Center => x + (width - image_width) / 2.0,
                    TextAlignment::Right => x + width - TABLE_CELL_PADDING_HORIZONTAL - image_width,
                };
                let image_id = register_image(doc, image);
                doc.page_mut(*page_index)
                    .expect("page index is valid")
                    .add_image(
                        image_id,
                        image_x,
                        image_top - image_height,
                        image_width,
                        image_height,
                    );
                image_top -= image_height;
            }
            let page = doc.page_mut(*page_index).expect("page index is valid");
            page.add_line(
                x,
                row_top,
                x + width,
                row_top,
                PdfColor::BLACK,
                TABLE_BORDER_WIDTH,
            );
            page.add_line(
                x,
                bottom,
                x + width,
                bottom,
                PdfColor::BLACK,
                TABLE_BORDER_WIDTH,
            );
            page.add_line(x, bottom, x, row_top, PdfColor::BLACK, TABLE_BORDER_WIDTH);
            page.add_line(
                x + width,
                bottom,
                x + width,
                row_top,
                PdfColor::BLACK,
                TABLE_BORDER_WIDTH,
            );
            x += width;
        }
        row_top -= row_height;
    }
    *y = row_top - BODY_FONT_SIZE;
}

fn image_render_size(image: &DocxImage, max_width: f32) -> (f32, f32) {
    if image.width <= max_width {
        return (image.width, image.height);
    }
    let scale = max_width / image.width;
    (image.width * scale, image.height * scale)
}

fn register_image(doc: &mut PdfDocument, image: &DocxImage) -> usize {
    match &image.data {
        DocxImageData::Jpeg {
            data,
            width,
            height,
        } => doc.add_jpeg_image(data.clone(), *width, *height),
        DocxImageData::Rgba {
            data,
            width,
            height,
        } => doc.add_rgba_image(data.clone(), *width, *height),
    }
}

fn wrap_styled_text(
    text: &str,
    max_width: f32,
    font_size: f32,
    bold: bool,
    italic: bool,
) -> Vec<String> {
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

            if styled_text_width_with_font(&candidate, font_size, bold, italic, None) <= max_width {
                current = candidate;
            } else {
                if !current.is_empty() {
                    lines.push(current);
                }
                current = String::new();
                append_wrapped_word(
                    &mut lines,
                    &mut current,
                    word,
                    max_width,
                    font_size,
                    bold,
                    italic,
                );
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
    bold: bool,
    italic: bool,
) {
    if styled_text_width_with_font(word, font_size, bold, italic, None) <= max_width {
        current.push_str(word);
        return;
    }

    for chunk in split_word_to_width(word, max_width, font_size, bold, italic) {
        if !current.is_empty() {
            lines.push(std::mem::take(current));
        }
        current.push_str(&chunk);
    }
}

fn split_word_to_width(
    word: &str,
    max_width: f32,
    font_size: f32,
    bold: bool,
    italic: bool,
) -> Vec<String> {
    let mut chunks = Vec::new();
    let mut current = String::new();

    for ch in word.chars() {
        let mut candidate = current.clone();
        candidate.push(ch);
        if !current.is_empty()
            && styled_text_width_with_font(&candidate, font_size, bold, italic, None) > max_width
        {
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
    use std::io::{Cursor, Write};

    use zip::write::SimpleFileOptions;

    use super::{
        convert_docx_bytes, plain_paragraph, read_docx_document, wrap_styled_text, DocxBlock,
        DocxImageData,
    };
    use crate::ConversionOptions;

    fn create_docx(document_xml: &str) -> Vec<u8> {
        let mut output = Cursor::new(Vec::new());
        {
            let mut archive = zip::ZipWriter::new(&mut output);
            archive
                .start_file("word/document.xml", SimpleFileOptions::default())
                .unwrap();
            archive.write_all(document_xml.as_bytes()).unwrap();
            archive.finish().unwrap();
        }
        output.into_inner()
    }

    fn create_docx_with_image(document_xml: &str, relationships_xml: &str) -> Vec<u8> {
        let image = image::RgbaImage::from_pixel(2, 1, image::Rgba([0, 0, 255, 255]));
        let mut png = Cursor::new(Vec::new());
        image::DynamicImage::ImageRgba8(image)
            .write_to(&mut png, image::ImageFormat::Png)
            .unwrap();

        let mut output = Cursor::new(Vec::new());
        {
            let mut archive = zip::ZipWriter::new(&mut output);
            for (path, data) in [
                ("word/document.xml", document_xml.as_bytes()),
                ("word/_rels/document.xml.rels", relationships_xml.as_bytes()),
                ("word/media/image1.png", png.get_ref()),
            ] {
                archive
                    .start_file(path, SimpleFileOptions::default())
                    .unwrap();
                archive.write_all(data).unwrap();
            }
            archive.finish().unwrap();
        }
        output.into_inner()
    }

    #[test]
    fn wraps_unspaced_text_without_truncating() {
        let text = "abcdefghijklmnopqrstuvwxyz";
        let lines = wrap_styled_text(text, 40.0, 12.0, false, false);

        assert!(lines.len() > 1);
        assert_eq!(lines.concat(), text);
        assert!(lines.iter().all(|line| {
            crate::pdf::styled_text_width_with_font(line, 12.0, false, false, None) <= 40.0
        }));
    }

    #[test]
    fn preserves_explicit_page_breaks() {
        let input = create_docx(
            r#"<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body><w:p><w:r><w:t>First</w:t><w:br w:type="page"/><w:t>Second</w:t></w:r></w:p><w:p><w:pPr><w:pageBreakBefore/></w:pPr><w:r><w:t>Third</w:t></w:r></w:p></w:body></w:document>"#,
        );

        let document = read_docx_document(&input).unwrap();

        assert_eq!(
            document.blocks,
            vec![
                DocxBlock::Paragraph(plain_paragraph("First".to_owned())),
                DocxBlock::PageBreak,
                DocxBlock::Paragraph(plain_paragraph("Second".to_owned())),
                DocxBlock::PageBreak,
                DocxBlock::Paragraph(plain_paragraph("Third".to_owned())),
            ]
        );
        let pdf = convert_docx_bytes(&input, &ConversionOptions::default()).unwrap();
        assert_eq!(
            pdf.windows(b"/Type /Page /Parent".len())
                .filter(|chunk| *chunk == b"/Type /Page /Parent")
                .count(),
            3
        );
    }

    #[test]
    fn reads_and_renders_basic_table() {
        let input = create_docx(
            r#"<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body><w:tbl><w:tblGrid><w:gridCol w:w="2000"/><w:gridCol w:w="3000"/></w:tblGrid><w:tr><w:tc><w:tcPr><w:tcW w:w="2000" w:type="dxa"/><w:shd w:fill="336699"/></w:tcPr><w:p><w:pPr><w:jc w:val="center"/></w:pPr><w:r><w:rPr><w:b/><w:sz w:val="16"/><w:color w:val="FFFFFF"/></w:rPr><w:t>Header</w:t></w:r></w:p></w:tc><w:tc><w:tcPr><w:tcW w:w="3000" w:type="dxa"/></w:tcPr><w:p><w:r><w:t>Value</w:t></w:r></w:p></w:tc></w:tr></w:tbl><w:sectPr><w:pgSz w:w="12240" w:h="15840"/><w:pgMar w:top="1440" w:right="1800" w:bottom="1440" w:left="1800"/></w:sectPr></w:body></w:document>"#,
        );

        let document = read_docx_document(&input).unwrap();

        assert_eq!(document.page_size, crate::PageSize::LETTER);
        assert_eq!(document.margins.left, 90.0);
        let DocxBlock::Table(table) = &document.blocks[0] else {
            panic!("expected table block");
        };
        assert_eq!(table.column_widths, vec![100.0, 150.0]);
        assert_eq!(table.rows[0].cells.len(), 2);
        assert_eq!(table.rows[0].cells[0].font_size, 8.0);
        assert!(table.rows[0].cells[0].bold);
        assert_eq!(table.rows[0].cells[0].text, "Header");
        assert!(table.rows[0].cells[0].fill.is_some());

        let pdf = convert_docx_bytes(&input, &ConversionOptions::default()).unwrap();
        assert_eq!(
            pdf.windows(b"/Type /Page /Parent".len())
                .filter(|chunk| *chunk == b"/Type /Page /Parent")
                .count(),
            1
        );
    }

    #[test]
    fn reads_and_renders_inline_png() {
        let input = create_docx_with_image(
            r#"<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main" xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><w:body><w:p><w:pPr><w:jc w:val="center"/></w:pPr><w:r><w:drawing><wp:inline><wp:extent cx="914400" cy="457200"/><a:graphic><a:graphicData><a:blip r:embed="rId1"/></a:graphicData></a:graphic></wp:inline></w:drawing></w:r></w:p></w:body></w:document>"#,
            r#"<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/image" Target="media/image1.png"/></Relationships>"#,
        );

        let document = read_docx_document(&input).unwrap();

        let DocxBlock::Image(image) = &document.blocks[0] else {
            panic!("expected image block");
        };
        assert_eq!((image.width, image.height), (72.0, 36.0));
        let DocxImageData::Rgba {
            data,
            width,
            height,
        } = &image.data
        else {
            panic!("expected RGBA image");
        };
        assert_eq!((*width, *height, data.len()), (2, 1, 8));

        let pdf = convert_docx_bytes(&input, &ConversionOptions::default()).unwrap();
        assert!(pdf
            .windows(b"/Subtype /Image".len())
            .any(|chunk| chunk == b"/Subtype /Image"));
    }
}
