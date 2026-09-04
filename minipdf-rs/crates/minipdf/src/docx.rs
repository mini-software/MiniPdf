use std::collections::HashMap;
use std::io::{Cursor, Read};

use zip::ZipArchive;

use crate::pdf::{styled_text_width_with_font, PdfColor, PdfDocument};
use crate::{read_zip_text, ConversionOptions, PageSize, Result};

const PAGE_WIDTH: f32 = 595.28;
const PAGE_HEIGHT: f32 = 841.89;
const MARGIN: f32 = 54.0;
const BODY_FONT_SIZE: f32 = 11.0;
const TABLE_CELL_PADDING_HORIZONTAL: f32 = 5.4;
const TABLE_BORDER_WIDTH: f32 = 0.5;
const VML_INLINE_IMAGE_TOP_LEADING: f32 = 2.0;

#[derive(Debug, PartialEq)]
struct DocxDocument {
    blocks: Vec<DocxBlock>,
    page_size: PageSize,
    margins: DocxMargins,
    grid_line_pitch: f32,
    compatibility_mode: u8,
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
    floating_images: Vec<DocxFloatingImage>,
    style_id: Option<String>,
    contextual_spacing: bool,
    alignment: TextAlignment,
    indent_left: f32,
    indent_right: f32,
    spacing_before: f32,
    spacing_after: f32,
    line_spacing: f32,
    line_spacing_explicit: bool,
    fill: Option<PdfColor>,
    bottom_border: Option<DocxBorder>,
}

#[derive(Debug, Clone, PartialEq)]
struct DocxFloatingImage {
    image: DocxImage,
    offset_x: f32,
    offset_y: f32,
    inset_left: f32,
    inset_top: f32,
    line_top_leading: f32,
}

#[derive(Debug, Clone, PartialEq)]
struct DocxRun {
    text: String,
    font_size: f32,
    font_name: Option<String>,
    bold: bool,
    italic: bool,
    underline: bool,
    color: PdfColor,
    highlight: Option<PdfColor>,
}

#[derive(Debug, Clone, Copy, PartialEq)]
struct DocxBorder {
    color: PdfColor,
    width: f32,
    space: f32,
    is_double: bool,
    pattern: BorderPattern,
}

#[derive(Debug, Clone, Copy, PartialEq)]
enum BorderPattern {
    Solid,
    DotDash,
    Dotted,
    Dashed,
}

#[derive(Debug, Clone, Default)]
struct DocxStyles {
    paragraph_defaults: ParagraphProperties,
    run_defaults: RunProperties,
    styles: HashMap<String, DocxStyle>,
    theme_fonts: DocxThemeFonts,
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
    font_name: Option<String>,
    font_theme: Option<ThemeFontReference>,
    bold: Option<bool>,
    italic: Option<bool>,
    underline: Option<bool>,
    color: Option<PdfColor>,
    highlight: Option<PdfColor>,
}

#[derive(Debug, Clone, Copy, PartialEq)]
enum ThemeFontReference {
    MajorEastAsia,
    MinorEastAsia,
}

#[derive(Debug, Clone, Default)]
struct DocxThemeFonts {
    major_east_asia: Option<String>,
    minor_east_asia: Option<String>,
}

#[derive(Debug, Clone, PartialEq)]
struct DocxImage {
    data: DocxImageData,
    width: f32,
    height: f32,
    alignment: TextAlignment,
}

#[derive(Debug, Clone, PartialEq)]
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
    alignment: TextAlignment,
    cell_margin_left: f32,
    cell_margin_right: f32,
    cell_margin_top: f32,
    cell_margin_bottom: f32,
    legacy_cjk_metrics: bool,
}

#[derive(Debug, PartialEq)]
struct DocxTableRow {
    cells: Vec<DocxTableCell>,
    height: Option<f32>,
}

#[derive(Debug, PartialEq)]
struct DocxTableCell {
    runs: Vec<DocxRun>,
    images: Vec<DocxImage>,
    width: Option<f32>,
    spacing_before: f32,
    spacing_after: f32,
    line_spacing: f32,
    fill: Option<PdfColor>,
    alignment: TextAlignment,
    vertical_alignment: VerticalAlignment,
    grid_span: usize,
    vertical_merge: VerticalMerge,
    borders: DocxCellBorders,
}

#[derive(Debug, Clone, Copy, Default, PartialEq)]
struct DocxCellBorders {
    top: Option<DocxBorder>,
    right: Option<DocxBorder>,
    bottom: Option<DocxBorder>,
    left: Option<DocxBorder>,
}

#[derive(Debug, Clone, Copy, PartialEq)]
enum TextAlignment {
    Left,
    Center,
    Right,
}

#[derive(Debug, Clone, Copy, PartialEq)]
enum VerticalAlignment {
    Top,
    Center,
    Bottom,
}

#[derive(Debug, Clone, Copy, PartialEq)]
enum VerticalMerge {
    None,
    Restart,
    Continue,
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
    let compatibility_mode = read_compatibility_mode(&mut archive)?;

    let xml = roxmltree::Document::parse(&document_xml)?;
    let Some(body) = xml.descendants().find(|node| node.has_tag_name("body")) else {
        return Ok(empty_docx_document());
    };
    let (page_size, margins, grid_line_pitch) = read_page_layout(body);
    let mut blocks = Vec::new();

    for node in body.children().filter(|node| node.is_element()) {
        if node.has_tag_name("p") {
            read_paragraph(node, &mut blocks, &styles, &relationships, &mut archive)?;
        } else if node.has_tag_name("tbl") {
            blocks.push(DocxBlock::Table(read_table(
                node,
                &styles,
                &relationships,
                &mut archive,
                compatibility_mode <= 12,
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
        grid_line_pitch,
        compatibility_mode,
    })
}

fn read_compatibility_mode(archive: &mut ZipArchive<Cursor<&[u8]>>) -> Result<u8> {
    let Some(settings_xml) = read_zip_text(archive, "word/settings.xml")? else {
        return Ok(15);
    };
    let xml = roxmltree::Document::parse(&settings_xml)?;
    Ok(xml
        .descendants()
        .find(|node| {
            node.has_tag_name("compatSetting")
                && attribute_by_local_name(*node, "name") == Some("compatibilityMode")
        })
        .and_then(|node| attribute_by_local_name(node, "val"))
        .and_then(|value| value.parse().ok())
        .unwrap_or(15))
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
        grid_line_pitch: 0.0,
        compatibility_mode: 15,
    }
}

fn plain_paragraph(text: String) -> DocxParagraph {
    DocxParagraph {
        runs: vec![DocxRun {
            text,
            font_size: BODY_FONT_SIZE,
            font_name: None,
            bold: false,
            italic: false,
            underline: false,
            color: PdfColor::BLACK,
            highlight: None,
        }],
        floating_images: Vec::new(),
        style_id: None,
        contextual_spacing: false,
        alignment: TextAlignment::Left,
        indent_left: 0.0,
        indent_right: 0.0,
        spacing_before: 0.0,
        spacing_after: 0.0,
        line_spacing: 1.15,
        line_spacing_explicit: false,
        fill: None,
        bottom_border: None,
    }
}

fn read_page_layout(body: roxmltree::Node<'_, '_>) -> (PageSize, DocxMargins, f32) {
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
    let grid_line_pitch = section
        .and_then(|node| child(node, "docGrid"))
        .filter(|node| {
            matches!(
                attribute_by_local_name(*node, "type"),
                Some("lines" | "linesAndChars" | "snapToChars")
            )
        })
        .and_then(|node| twips_attribute(Some(node), "linePitch"))
        .unwrap_or(0.0);
    (page_size, margins, grid_line_pitch)
}

fn read_styles(archive: &mut ZipArchive<Cursor<&[u8]>>) -> Result<DocxStyles> {
    let theme_fonts = read_theme_fonts(archive)?;
    let Some(styles_xml) = read_zip_text(archive, "word/styles.xml")? else {
        return Ok(DocxStyles {
            theme_fonts,
            ..DocxStyles::default()
        });
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
        theme_fonts,
    })
}

fn read_theme_fonts(archive: &mut ZipArchive<Cursor<&[u8]>>) -> Result<DocxThemeFonts> {
    let Some(theme_xml) = read_zip_text(archive, "word/theme/theme1.xml")? else {
        return Ok(DocxThemeFonts::default());
    };
    let xml = roxmltree::Document::parse(&theme_xml)?;
    let font_scheme = xml
        .descendants()
        .find(|node| node.has_tag_name("fontScheme"));
    Ok(DocxThemeFonts {
        major_east_asia: font_scheme
            .and_then(|node| child(node, "majorFont"))
            .and_then(theme_east_asia_font),
        minor_east_asia: font_scheme
            .and_then(|node| child(node, "minorFont"))
            .and_then(theme_east_asia_font),
    })
}

fn theme_east_asia_font(group: roxmltree::Node<'_, '_>) -> Option<String> {
    child(group, "ea")
        .and_then(|node| attribute_by_local_name(node, "typeface"))
        .filter(|name| !name.is_empty())
        .or_else(|| {
            group
                .children()
                .find(|node| {
                    node.has_tag_name("font")
                        && attribute_by_local_name(*node, "script") == Some("Hans")
                })
                .and_then(|node| attribute_by_local_name(node, "typeface"))
        })
        .map(str::to_owned)
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
    let fonts = child(properties, "rFonts");
    RunProperties {
        font_size: child(properties, "sz")
            .and_then(|node| numeric_attribute(node, "val"))
            .map(|half_points| half_points / 2.0),
        font_name: fonts
            .and_then(|node| {
                attribute_by_local_name(node, "eastAsia")
                    .or_else(|| attribute_by_local_name(node, "ascii"))
                    .or_else(|| attribute_by_local_name(node, "hAnsi"))
            })
            .map(str::to_owned),
        font_theme: fonts
            .and_then(|node| {
                attribute_by_local_name(node, "eastAsiaTheme")
                    .or_else(|| attribute_by_local_name(node, "asciiTheme"))
                    .or_else(|| attribute_by_local_name(node, "hAnsiTheme"))
            })
            .and_then(|value| match value {
                "majorEastAsia" => Some(ThemeFontReference::MajorEastAsia),
                "minorEastAsia" => Some(ThemeFontReference::MinorEastAsia),
                _ => None,
            }),
        bold: child(properties, "b").map(property_enabled),
        italic: child(properties, "i").map(property_enabled),
        underline: child(properties, "u").map(property_enabled),
        color: child(properties, "color")
            .and_then(|node| attribute_by_local_name(node, "val"))
            .and_then(parse_hex_color),
        highlight: child(properties, "highlight")
            .and_then(|node| attribute_by_local_name(node, "val"))
            .and_then(parse_highlight_color),
    }
}

fn parse_highlight_color(value: &str) -> Option<PdfColor> {
    match value {
        "yellow" => Some(PdfColor::new(1.0, 1.0, 0.0)),
        "green" => Some(PdfColor::new(0.0, 1.0, 0.0)),
        "cyan" => Some(PdfColor::new(0.0, 1.0, 1.0)),
        "magenta" => Some(PdfColor::new(1.0, 0.0, 1.0)),
        "blue" => Some(PdfColor::new(0.0, 0.0, 1.0)),
        "red" => Some(PdfColor::new(1.0, 0.0, 0.0)),
        "darkBlue" => Some(PdfColor::new(0.0, 0.0, 0.5)),
        "darkCyan" => Some(PdfColor::new(0.0, 0.5, 0.5)),
        "darkGreen" => Some(PdfColor::new(0.0, 0.5, 0.0)),
        "darkMagenta" => Some(PdfColor::new(0.5, 0.0, 0.5)),
        "darkRed" => Some(PdfColor::new(0.5, 0.0, 0.0)),
        "darkYellow" => Some(PdfColor::new(0.5, 0.5, 0.0)),
        "darkGray" => Some(PdfColor::new(0.5, 0.5, 0.5)),
        "lightGray" => Some(PdfColor::new(0.75, 0.75, 0.75)),
        "black" => Some(PdfColor::BLACK),
        _ => None,
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
        pattern: match style {
            "dotDash" | "dashDotStroked" => BorderPattern::DotDash,
            "dotted" => BorderPattern::Dotted,
            "dashed" | "dashSmallGap" => BorderPattern::Dashed,
            _ => BorderPattern::Solid,
        },
    })
}

fn default_table_border() -> DocxBorder {
    DocxBorder {
        color: PdfColor::BLACK,
        width: TABLE_BORDER_WIDTH,
        space: 0.0,
        is_double: false,
        pattern: BorderPattern::Solid,
    }
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
    if source.font_name.is_some() {
        target.font_name.clone_from(&source.font_name);
        target.font_theme = None;
    }
    if source.font_theme.is_some() {
        target.font_theme = source.font_theme;
        target.font_name = None;
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
    if source.highlight.is_some() {
        target.highlight = source.highlight;
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

    let floating_images = read_vml_floating_images(paragraph, relationships, archive)?;

    let mut runs = Vec::new();
    let mut emitted_non_text = false;
    for run_node in paragraph
        .descendants()
        .filter(|node| node.has_tag_name("r"))
    {
        if run_node
            .ancestors()
            .any(|node| node.has_tag_name("txbxContent"))
            || run_node
                .descendants()
                .any(|node| node.has_tag_name("txbxContent"))
        {
            continue;
        }
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
                        runs.push(create_run(
                            std::mem::take(&mut text),
                            &run_properties,
                            &styles.theme_fonts,
                        ));
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
            runs.push(create_run(text, &run_properties, &styles.theme_fonts));
        }
    }
    if !runs.is_empty() || !emitted_non_text {
        let mut output =
            create_paragraph(std::mem::take(&mut runs), style_id, &paragraph_properties);
        output.floating_images = floating_images;
        blocks.push(DocxBlock::Paragraph(output));
    }
    Ok(())
}

fn read_vml_floating_images(
    paragraph: roxmltree::Node<'_, '_>,
    relationships: &HashMap<String, String>,
    archive: &mut ZipArchive<Cursor<&[u8]>>,
) -> Result<Vec<DocxFloatingImage>> {
    let mut images = Vec::new();
    for shape in paragraph
        .descendants()
        .filter(|node| node.has_tag_name("shape"))
    {
        let Some(style) = shape.attribute("style") else {
            continue;
        };
        if !vml_style_value(style, "position")
            .is_some_and(|value| value.eq_ignore_ascii_case("absolute"))
        {
            continue;
        }
        let Some(textbox) = shape
            .descendants()
            .find(|node| node.has_tag_name("txbxContent"))
        else {
            continue;
        };
        let offset_x = vml_style_point(style, "margin-left").unwrap_or(0.0);
        let offset_y = vml_style_point(style, "margin-top").unwrap_or(0.0);
        let (inset_left, inset_top) = vml_textbox_insets(shape);
        for drawing in textbox
            .descendants()
            .filter(|node| node.has_tag_name("drawing"))
        {
            let image_paragraph = drawing
                .ancestors()
                .find(|node| node.has_tag_name("p"))
                .unwrap_or(paragraph);
            if let Some(image) = read_image(drawing, image_paragraph, relationships, archive)? {
                images.push(DocxFloatingImage {
                    image,
                    offset_x,
                    offset_y,
                    inset_left,
                    inset_top,
                    line_top_leading: VML_INLINE_IMAGE_TOP_LEADING,
                });
            }
        }
    }
    Ok(images)
}

fn vml_style_value<'a>(style: &'a str, name: &str) -> Option<&'a str> {
    style.split(';').find_map(|declaration| {
        let (property, value) = declaration.split_once(':')?;
        property
            .trim()
            .eq_ignore_ascii_case(name)
            .then(|| value.trim())
    })
}

fn vml_style_point(style: &str, name: &str) -> Option<f32> {
    parse_vml_length(vml_style_value(style, name)?)
}

fn parse_vml_length(value: &str) -> Option<f32> {
    let value = value.trim();
    if let Some(points) = value.strip_suffix("pt") {
        points.trim().parse().ok()
    } else if let Some(inches) = value.strip_suffix("in") {
        inches
            .trim()
            .parse::<f32>()
            .ok()
            .map(|length| length * 72.0)
    } else if let Some(pixels) = value.strip_suffix("px") {
        pixels
            .trim()
            .parse::<f32>()
            .ok()
            .map(|length| length * 0.75)
    } else {
        value.parse().ok()
    }
}

fn vml_textbox_insets(shape: roxmltree::Node<'_, '_>) -> (f32, f32) {
    let inset = shape
        .descendants()
        .find(|node| node.has_tag_name("textbox"))
        .and_then(|node| node.attribute("inset"));
    let Some(inset) = inset else {
        return (7.2, 3.6);
    };
    let values: Vec<_> = inset
        .split(',')
        .filter_map(|value| parse_vml_length(value.trim()))
        .collect();
    (
        values.first().copied().unwrap_or(7.2),
        values.get(1).copied().unwrap_or(3.6),
    )
}

fn create_run(text: String, properties: &RunProperties, theme_fonts: &DocxThemeFonts) -> DocxRun {
    DocxRun {
        text,
        font_size: properties.font_size.unwrap_or(BODY_FONT_SIZE),
        font_name: properties
            .font_name
            .clone()
            .or_else(|| match properties.font_theme {
                Some(ThemeFontReference::MajorEastAsia) => theme_fonts.major_east_asia.clone(),
                Some(ThemeFontReference::MinorEastAsia) => theme_fonts.minor_east_asia.clone(),
                None => None,
            }),
        bold: properties.bold.unwrap_or(false),
        italic: properties.italic.unwrap_or(false),
        underline: properties.underline.unwrap_or(false),
        color: properties.color.unwrap_or(PdfColor::BLACK),
        highlight: properties.highlight,
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
        properties.spacing_after.unwrap_or(0.0)
    };
    DocxParagraph {
        runs,
        floating_images: Vec::new(),
        style_id: style_id.map(str::to_owned),
        contextual_spacing: properties.contextual_spacing.unwrap_or(false),
        alignment: properties.alignment.unwrap_or(TextAlignment::Left),
        indent_left: properties.indent_left.unwrap_or(0.0),
        indent_right: properties.indent_right.unwrap_or(0.0),
        spacing_before: properties.spacing_before.unwrap_or(0.0),
        spacing_after,
        line_spacing: properties.line_spacing.unwrap_or(1.15),
        line_spacing_explicit: properties.line_spacing.is_some(),
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
    styles: &DocxStyles,
    relationships: &HashMap<String, String>,
    archive: &mut ZipArchive<Cursor<&[u8]>>,
    legacy_cjk_metrics: bool,
) -> Result<DocxTable> {
    let properties = child(table, "tblPr");
    let cell_margins = properties.and_then(|node| child(node, "tblCellMar"));
    let style_id = properties
        .and_then(|node| child(node, "tblStyle"))
        .and_then(|node| attribute_by_local_name(node, "val"));
    let style_has_grid = style_id == Some("af2");
    let use_style_borders = style_has_grid
        && properties
            .and_then(|node| child(node, "tblBorders"))
            .is_none();
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
        let height = child(row, "trPr")
            .and_then(|node| child(node, "trHeight"))
            .and_then(|node| twips_attribute(Some(node), "val"));
        let mut cells = Vec::new();
        for cell in row.children().filter(|node| node.has_tag_name("tc")) {
            cells.push(read_table_cell(
                cell,
                styles,
                relationships,
                archive,
                use_style_borders,
            )?);
        }
        rows.push(DocxTableRow { cells, height });
    }
    Ok(DocxTable {
        column_widths,
        rows,
        alignment: properties
            .and_then(|node| child(node, "jc"))
            .and_then(|node| attribute_by_local_name(node, "val"))
            .map(parse_alignment)
            .unwrap_or(TextAlignment::Left),
        cell_margin_left: table_margin(cell_margins, "left", TABLE_CELL_PADDING_HORIZONTAL),
        cell_margin_right: table_margin(cell_margins, "right", TABLE_CELL_PADDING_HORIZONTAL),
        cell_margin_top: table_margin(cell_margins, "top", 0.0),
        cell_margin_bottom: table_margin(cell_margins, "bottom", 0.0),
        legacy_cjk_metrics,
    })
}

fn table_margin(margins: Option<roxmltree::Node<'_, '_>>, side: &str, default: f32) -> f32 {
    margins
        .and_then(|node| child(node, side))
        .and_then(|node| twips_attribute(Some(node), "w"))
        .unwrap_or(default)
}

fn read_table_cell(
    cell: roxmltree::Node<'_, '_>,
    styles: &DocxStyles,
    relationships: &HashMap<String, String>,
    archive: &mut ZipArchive<Cursor<&[u8]>>,
    use_style_borders: bool,
) -> Result<DocxTableCell> {
    let properties = child(cell, "tcPr");
    let paragraphs: Vec<_> = cell
        .children()
        .filter(|node| node.has_tag_name("p"))
        .collect();
    let mut runs: Vec<DocxRun> = Vec::new();
    for paragraph in &paragraphs {
        let paragraph_runs = read_table_cell_runs(*paragraph, styles);
        if paragraph_runs.is_empty() {
            continue;
        }
        if let Some(previous) = runs.last_mut() {
            previous.text.push('\n');
        }
        runs.extend(paragraph_runs);
    }
    let first_paragraph = paragraphs.first().copied();
    let first_paragraph_properties = first_paragraph
        .and_then(|paragraph| child(paragraph, "pPr"))
        .map(read_paragraph_properties)
        .unwrap_or_default();
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
        runs,
        images,
        width: properties
            .and_then(|node| child(node, "tcW"))
            .and_then(|node| twips_attribute(Some(node), "w")),
        spacing_before: first_paragraph_properties.spacing_before.unwrap_or(0.0),
        spacing_after: first_paragraph_properties.spacing_after.unwrap_or(0.0),
        line_spacing: first_paragraph_properties.line_spacing.unwrap_or(1.0),
        fill: properties
            .and_then(|node| child(node, "shd"))
            .and_then(|node| attribute_by_local_name(node, "fill"))
            .and_then(parse_hex_color),
        alignment: first_paragraph
            .and_then(|node| node.descendants().find(|child| child.has_tag_name("jc")))
            .and_then(|node| attribute_by_local_name(node, "val"))
            .map(|value| match value {
                "center" => TextAlignment::Center,
                "right" | "end" => TextAlignment::Right,
                _ => TextAlignment::Left,
            })
            .unwrap_or(TextAlignment::Left),
        vertical_alignment: properties
            .and_then(|node| child(node, "vAlign"))
            .and_then(|node| attribute_by_local_name(node, "val"))
            .map(|value| match value {
                "center" => VerticalAlignment::Center,
                "bottom" => VerticalAlignment::Bottom,
                _ => VerticalAlignment::Top,
            })
            .unwrap_or(VerticalAlignment::Top),
        grid_span: properties
            .and_then(|node| child(node, "gridSpan"))
            .and_then(|node| numeric_attribute(node, "val"))
            .map(|value| value.max(1.0) as usize)
            .unwrap_or(1),
        vertical_merge: properties
            .and_then(|node| child(node, "vMerge"))
            .map(|node| match attribute_by_local_name(node, "val") {
                Some("restart") => VerticalMerge::Restart,
                _ => VerticalMerge::Continue,
            })
            .unwrap_or(VerticalMerge::None),
        borders: DocxCellBorders {
            top: read_cell_border(properties, "top", use_style_borders),
            right: read_cell_border(properties, "right", use_style_borders),
            bottom: read_cell_border(properties, "bottom", use_style_borders),
            left: read_cell_border(properties, "left", use_style_borders),
        },
    })
}

fn read_table_cell_runs(paragraph: roxmltree::Node<'_, '_>, styles: &DocxStyles) -> Vec<DocxRun> {
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
    if let Some(properties) = child(paragraph, "pPr").and_then(|node| child(node, "rPr")) {
        merge_run_properties(&mut base_run_properties, &read_run_properties(properties));
    }

    paragraph
        .descendants()
        .filter(|node| node.has_tag_name("r"))
        .filter_map(|run_node| {
            let mut properties = base_run_properties.clone();
            if let Some(direct) = child(run_node, "rPr") {
                merge_run_properties(&mut properties, &read_run_properties(direct));
            }
            let text = paragraph_text(run_node);
            (!text.is_empty()).then(|| create_run(text, &properties, &styles.theme_fonts))
        })
        .collect()
}

fn read_cell_border(
    properties: Option<roxmltree::Node<'_, '_>>,
    side: &str,
    use_style_border: bool,
) -> Option<DocxBorder> {
    let border = properties
        .and_then(|node| child(node, "tcBorders"))
        .and_then(|node| child(node, side));
    match border {
        Some(border) => read_border(border),
        None if use_style_border => Some(default_table_border()),
        None => None,
    }
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
                let spacing_before = if document.compatibility_mode <= 12 {
                    paragraph.spacing_before
                } else if previous_paragraph.is_some() {
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
                let previous_grid_ascent = previous_paragraph
                    .map(|previous| paragraph_line_metrics(previous, document.grid_line_pitch).2);
                render_paragraph(
                    doc,
                    paragraph,
                    (
                        spacing_before,
                        spacing_after,
                        document.grid_line_pitch,
                        previous_grid_ascent,
                    ),
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
    layout: (f32, f32, f32, Option<f32>),
    page_size: PageSize,
    margins: DocxMargins,
    page_index: &mut usize,
    y: &mut f32,
) {
    let (spacing_before, spacing_after, grid_line_pitch, previous_grid_ascent) = layout;
    let (_, line_height, baseline_offset) = paragraph_line_metrics(paragraph, grid_line_pitch);
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
    if grid_line_pitch > 0.0 {
        if let Some(previous_ascent) = previous_grid_ascent.filter(|ascent| *ascent > 0.0) {
            top -= baseline_offset - previous_ascent;
        }
    }
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
        let baseline = top - baseline_offset - line_index as f32 * line_height;
        for run in line {
            let width = run_width(run, &run.text);
            let page = doc.page_mut(*page_index).expect("page index is valid");
            page.add_styled_text_with_font(
                &run.text,
                x,
                baseline,
                run.font_size,
                run.color,
                run.bold,
                run.italic,
                run.font_name.as_deref(),
            );
            if run.underline && !run.text.trim().is_empty() {
                page.add_line(x, baseline - 1.5, x + width, baseline - 1.5, run.color, 0.5);
            }
            x += width;
        }
    }

    for floating in &paragraph.floating_images {
        let image_id = register_image(doc, &floating.image);
        doc.page_mut(*page_index)
            .expect("page index is valid")
            .add_image(
                image_id,
                margins.left + floating.offset_x + floating.inset_left,
                top - floating.offset_y
                    - floating.inset_top
                    - floating.line_top_leading
                    - floating.image.height,
                floating.image.width,
                floating.image.height,
            );
    }

    if let Some(border) = paragraph.bottom_border {
        let border_y = bottom - border.space;
        let page = doc.page_mut(*page_index).expect("page index is valid");
        draw_border(
            page,
            (left, border_y),
            (left + available_width, border_y),
            border,
        );
        if border.is_double {
            draw_border(
                page,
                (left, border_y - border.width * 2.0),
                (left + available_width, border_y - border.width * 2.0),
                border,
            );
        }
    }
    *y = top - content_height - spacing_after - BODY_FONT_SIZE;
}

fn paragraph_line_metrics(paragraph: &DocxParagraph, grid_line_pitch: f32) -> (f32, f32, f32) {
    let font_size = paragraph
        .runs
        .iter()
        .map(|run| run.font_size)
        .fold(BODY_FONT_SIZE, f32::max);
    let mut line_height = font_size * 1.18 * paragraph.line_spacing;
    if grid_line_pitch <= 0.0 {
        return (font_size, line_height, font_size);
    }

    if paragraph.line_spacing_explicit {
        line_height = line_height.max(grid_line_pitch * paragraph.line_spacing);
        let text_length: usize = paragraph
            .runs
            .iter()
            .map(|run| run.text.chars().count())
            .sum();
        let is_centered_cjk_heading = paragraph.alignment == TextAlignment::Center
            && font_size >= 14.0
            && text_length <= 12
            && paragraph
                .runs
                .iter()
                .any(|run| run.text.chars().any(is_cjk_character));
        if is_centered_cjk_heading {
            line_height =
                line_height.max((font_size * 1.29 / grid_line_pitch).ceil() * grid_line_pitch);
        }
    } else if font_size <= grid_line_pitch {
        line_height = grid_line_pitch;
    } else {
        line_height = line_height.max(grid_line_pitch);
    }

    let font_name = paragraph
        .runs
        .iter()
        .find_map(|run| run.font_name.as_deref());
    let baseline_offset = if font_name.is_some_and(is_tall_cjk_font_name) {
        (line_height - font_size) / 2.0 + font_size * 0.86
    } else {
        (line_height + font_size) / 2.0
    };
    (font_size, line_height, baseline_offset)
}

fn is_cjk_character(ch: char) -> bool {
    matches!(ch as u32, 0x2e80..=0x9fff | 0xf900..=0xfaff)
}

fn is_tall_cjk_font_name(font_name: &str) -> bool {
    let normalized = font_name.trim().to_ascii_lowercase();
    [
        "kaiti",
        "simsun",
        "simhei",
        "nsimsun",
        "fangsong",
        "dengxian",
        "microsoft yahei",
    ]
    .iter()
    .any(|name| normalized.contains(name))
        || ["宋体", "黑体", "楷体", "仿宋", "等线", "微软雅黑"]
            .iter()
            .any(|name| font_name.contains(name))
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
    styled_text_width_with_font(
        text,
        run.font_size,
        run.bold,
        run.italic,
        run.font_name.as_deref(),
    )
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
        .map(|row| row.cells.iter().map(|cell| cell.grid_span).sum())
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

    let table_width: f32 = widths.iter().sum();
    let table_left = match table.alignment {
        TextAlignment::Left => margins.left,
        TextAlignment::Center => margins.left + (max_width - table_width) / 2.0,
        TextAlignment::Right => page_size.width - margins.right - table_width,
    };

    let mut row_heights: Vec<f32> = table
        .rows
        .iter()
        .map(|row| {
            let mut grid_index = 0;
            let content_height = row
                .cells
                .iter()
                .map(|cell| {
                    let width = spanned_width(&widths, grid_index, cell.grid_span);
                    grid_index += cell.grid_span;
                    if cell.vertical_merge != VerticalMerge::None {
                        return 0.0;
                    }
                    table_cell_content_height(table, cell, width)
                })
                .fold(0.0_f32, f32::max)
                .max(BODY_FONT_SIZE);
            row.height
                .map_or(content_height, |height| height.max(content_height))
        })
        .collect();

    for (row_index, row) in table.rows.iter().enumerate() {
        let mut grid_index = 0;
        for cell in &row.cells {
            if cell.vertical_merge == VerticalMerge::Restart {
                let mut end_row = row_index;
                for next_row in (row_index + 1)..table.rows.len() {
                    let Some(next_cell) = cell_at_grid(&table.rows[next_row], grid_index) else {
                        break;
                    };
                    if next_cell.vertical_merge != VerticalMerge::Continue {
                        break;
                    }
                    end_row = next_row;
                }
                let width = spanned_width(&widths, grid_index, cell.grid_span);
                let required_height = table_cell_content_height(table, cell, width);
                let current_height: f32 = row_heights[row_index..=end_row].iter().sum();
                if required_height > current_height {
                    row_heights[end_row] += required_height - current_height;
                }
            }
            grid_index += cell.grid_span;
        }
    }

    let mut row_top = *y + BODY_FONT_SIZE;
    for (row_index, row) in table.rows.iter().enumerate() {
        let row_height = row_heights[row_index];

        if row_top - row_height < margins.bottom {
            *page_index = doc.pages().len();
            doc.add_page(page_size.width, page_size.height);
            row_top = page_size.height - margins.top;
        }

        let mut grid_index = 0;
        for cell in &row.cells {
            let x = table_left + widths.iter().take(grid_index).sum::<f32>();
            let width = spanned_width(&widths, grid_index, cell.grid_span);
            if cell.vertical_merge == VerticalMerge::Continue {
                grid_index += cell.grid_span;
                continue;
            }
            let cell_height = if cell.vertical_merge == VerticalMerge::Restart {
                merged_cell_height(table, &row_heights, row_index, grid_index)
            } else {
                row_height
            };
            let bottom = row_top - cell_height;
            let page = doc.page_mut(*page_index).expect("page index is valid");
            if let Some(fill) = cell.fill {
                page.add_rect(x, bottom, width, cell_height, fill);
            }
            let wrapped_lines = wrap_table_cell_runs(
                &cell.runs,
                (width - table.cell_margin_left - table.cell_margin_right).max(1.0),
            );
            let line_height = table_cell_line_height(table, cell);
            let text_height = wrapped_lines.len() as f32 * line_height;
            let font_size = table_cell_font_size(cell);
            let image_height: f32 = cell
                .images
                .iter()
                .map(|image| {
                    image_render_size(
                        image,
                        (width - table.cell_margin_left - table.cell_margin_right).max(1.0),
                    )
                    .1
                })
                .sum();
            let content_height =
                cell.spacing_before + text_height + image_height + cell.spacing_after;
            let content_top = match cell.vertical_alignment {
                VerticalAlignment::Top => row_top - table.cell_margin_top,
                VerticalAlignment::Center => bottom + (cell_height + content_height) / 2.0,
                VerticalAlignment::Bottom => bottom + table.cell_margin_bottom + content_height,
            };
            let mut text_y = content_top - cell.spacing_before - font_size;
            for line in &wrapped_lines {
                let text_width: f32 = line.iter().map(|run| run_width(run, &run.text)).sum();
                let text_x = match cell.alignment {
                    TextAlignment::Left => x + table.cell_margin_left,
                    TextAlignment::Center => x + (width - text_width) / 2.0,
                    TextAlignment::Right => x + width - table.cell_margin_right - text_width,
                };
                let mut run_x = text_x.max(x + TABLE_BORDER_WIDTH);
                for run in line {
                    let width = run_width(run, &run.text);
                    if let Some(highlight) = run.highlight {
                        page.add_rect(
                            run_x - 0.7,
                            text_y - run.font_size * 0.24,
                            width + 1.4,
                            run.font_size * 1.18,
                            highlight,
                        );
                    }
                    page.add_styled_text_with_font(
                        &run.text,
                        run_x,
                        text_y,
                        run.font_size,
                        run.color,
                        run.bold,
                        run.italic,
                        run.font_name.as_deref(),
                    );
                    run_x += width;
                }
                text_y -= line_height;
            }
            let mut image_top = content_top - cell.spacing_before - text_height;
            for image in &cell.images {
                let (image_width, image_height) = image_render_size(
                    image,
                    (width - table.cell_margin_left - table.cell_margin_right).max(1.0),
                );
                let image_x = match image.alignment {
                    TextAlignment::Left => x + table.cell_margin_left,
                    TextAlignment::Center => x + (width - image_width) / 2.0,
                    TextAlignment::Right => x + width - table.cell_margin_right - image_width,
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
            if let Some(border) = cell.borders.top {
                draw_border(page, (x, row_top), (x + width, row_top), border);
            }
            if let Some(border) = cell.borders.bottom {
                draw_border(page, (x, bottom), (x + width, bottom), border);
            }
            if let Some(border) = cell.borders.left {
                draw_border(page, (x, bottom), (x, row_top), border);
            }
            if let Some(border) = cell.borders.right {
                draw_border(page, (x + width, bottom), (x + width, row_top), border);
            }
            grid_index += cell.grid_span;
        }
        row_top -= row_height;
    }
    *y = row_top - BODY_FONT_SIZE;
}

fn draw_border(
    page: &mut crate::pdf::PdfPage,
    start: (f32, f32),
    end: (f32, f32),
    border: DocxBorder,
) {
    let pattern: &[f32] = match border.pattern {
        BorderPattern::Solid => &[],
        BorderPattern::DotDash => &[1.0, 1.0, 3.0, 1.0],
        BorderPattern::Dotted => &[0.8, 1.2],
        BorderPattern::Dashed => &[3.0, 2.0],
    };
    page.add_line_with_dash_pattern(start, end, border.color, border.width, pattern);
}

fn spanned_width(widths: &[f32], grid_index: usize, grid_span: usize) -> f32 {
    widths.iter().skip(grid_index).take(grid_span).sum()
}

fn merged_cell_height(
    table: &DocxTable,
    row_heights: &[f32],
    start_row: usize,
    grid_index: usize,
) -> f32 {
    let mut height = row_heights[start_row];
    for (row_index, row) in table.rows.iter().enumerate().skip(start_row + 1) {
        let Some(cell) = cell_at_grid(row, grid_index) else {
            break;
        };
        if cell.vertical_merge != VerticalMerge::Continue {
            break;
        }
        height += row_heights[row_index];
    }
    height
}

fn cell_at_grid(row: &DocxTableRow, target_grid_index: usize) -> Option<&DocxTableCell> {
    let mut grid_index = 0;
    for cell in &row.cells {
        if grid_index == target_grid_index {
            return Some(cell);
        }
        grid_index += cell.grid_span;
    }
    None
}

fn table_cell_content_height(table: &DocxTable, cell: &DocxTableCell, width: f32) -> f32 {
    let content_width = (width - table.cell_margin_left - table.cell_margin_right).max(1.0);
    let lines = wrap_table_cell_runs(&cell.runs, content_width);
    let text_height = cell.spacing_before
        + lines.len() as f32 * table_cell_line_height(table, cell)
        + cell.spacing_after;
    let image_height: f32 = cell
        .images
        .iter()
        .map(|image| image_render_size(image, content_width).1)
        .sum();
    text_height + image_height + table.cell_margin_top + table.cell_margin_bottom
}

fn table_cell_line_height(table: &DocxTable, cell: &DocxTableCell) -> f32 {
    let metrics_factor = if table.legacy_cjk_metrics
        && cell
            .runs
            .iter()
            .filter_map(|run| run.font_name.as_deref())
            .any(is_tall_cjk_font_name)
    {
        1.35
    } else {
        1.18
    };
    table_cell_font_size(cell) * metrics_factor * cell.line_spacing
}

fn table_cell_font_size(cell: &DocxTableCell) -> f32 {
    cell.runs
        .iter()
        .map(|run| run.font_size)
        .reduce(f32::max)
        .unwrap_or(BODY_FONT_SIZE)
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

fn wrap_table_cell_runs(runs: &[DocxRun], max_width: f32) -> Vec<Vec<DocxRun>> {
    let Some(first_run) = runs.first() else {
        return Vec::new();
    };
    let mut measurement_run = first_run.clone();
    measurement_run.font_name = runs.iter().find_map(|run| run.font_name.clone());
    let (text, styled_characters) = normalize_table_cell_runs(runs);
    let wrapped_text = wrap_styled_text_with_font(
        &text,
        max_width,
        measurement_run.font_size,
        measurement_run.bold,
        measurement_run.italic,
        measurement_run.font_name.as_deref(),
    );
    let mut characters = styled_characters.into_iter();
    wrapped_text
        .into_iter()
        .map(|line| {
            let mut output = Vec::new();
            for _ in line.chars() {
                if let Some(character) = characters.next() {
                    push_styled_character(&mut output, character);
                }
            }
            output
        })
        .collect()
}

fn normalize_table_cell_runs(runs: &[DocxRun]) -> (String, Vec<DocxRun>) {
    let mut text = String::new();
    let mut characters = Vec::new();
    let mut pending_space = false;
    let mut line_has_text = false;
    for run in runs {
        for character in run.text.replace('\t', "    ").chars() {
            if character == '\n' {
                text.push('\n');
                pending_space = false;
                line_has_text = false;
            } else if character.is_whitespace() {
                pending_space = line_has_text;
            } else {
                if pending_space {
                    let mut space = run.clone();
                    space.text = " ".to_owned();
                    text.push(' ');
                    characters.push(space);
                }
                let mut styled_character = run.clone();
                styled_character.text = character.to_string();
                text.push(character);
                characters.push(styled_character);
                pending_space = false;
                line_has_text = true;
            }
        }
    }
    (text, characters)
}

fn push_styled_character(line: &mut Vec<DocxRun>, character: DocxRun) {
    if let Some(last) = line.last_mut() {
        if same_run_style(last, &character) {
            last.text.push_str(&character.text);
            return;
        }
    }
    line.push(character);
}

fn same_run_style(left: &DocxRun, right: &DocxRun) -> bool {
    left.font_size == right.font_size
        && left.font_name == right.font_name
        && left.bold == right.bold
        && left.italic == right.italic
        && left.underline == right.underline
        && left.color == right.color
        && left.highlight == right.highlight
}

fn wrap_styled_text_with_font(
    text: &str,
    max_width: f32,
    font_size: f32,
    bold: bool,
    italic: bool,
    font_name: Option<&str>,
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

            if styled_text_width_with_font(&candidate, font_size, bold, italic, font_name)
                <= max_width
            {
                current = candidate;
            } else if word.chars().next().is_some_and(is_forbidden_line_start)
                && !current.is_empty()
            {
                let trailing = current.pop().expect("preceding character exists");
                if !current.is_empty() {
                    lines.push(std::mem::take(&mut current));
                }
                current.push(trailing);
                current.push_str(word);
            } else {
                if !current.is_empty() {
                    lines.push(current);
                }
                current = append_wrapped_word(
                    &mut lines, word, max_width, font_size, bold, italic, font_name,
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
    word: &str,
    max_width: f32,
    font_size: f32,
    bold: bool,
    italic: bool,
    font_name: Option<&str>,
) -> String {
    if styled_text_width_with_font(word, font_size, bold, italic, font_name) <= max_width {
        return word.to_owned();
    }

    let mut chunks = split_word_to_width(word, max_width, font_size, bold, italic, font_name);
    let current = chunks.pop().unwrap_or_default();
    for chunk in chunks {
        lines.push(chunk);
    }
    current
}

fn split_word_to_width(
    word: &str,
    max_width: f32,
    font_size: f32,
    bold: bool,
    italic: bool,
    font_name: Option<&str>,
) -> Vec<String> {
    let mut chunks = Vec::new();
    let mut current = String::new();

    for ch in word.chars() {
        let mut candidate = current.clone();
        candidate.push(ch);
        if !current.is_empty()
            && styled_text_width_with_font(&candidate, font_size, bold, italic, font_name)
                > max_width
        {
            if is_forbidden_line_start(ch) {
                let trailing = current.pop().expect("preceding character exists");
                if !current.is_empty() {
                    chunks.push(std::mem::take(&mut current));
                }
                current.push(trailing);
            } else if current.chars().last().is_some_and(is_forbidden_line_end) {
                let opening = current.pop().expect("line-ending character exists");
                if !current.is_empty() {
                    chunks.push(std::mem::take(&mut current));
                }
                current.push(opening);
            } else {
                chunks.push(std::mem::take(&mut current));
            }
        }
        current.push(ch);
    }

    if !current.is_empty() {
        chunks.push(current);
    }

    chunks
}

fn is_forbidden_line_end(ch: char) -> bool {
    matches!(
        ch,
        '(' | '[' | '{' | '（' | '［' | '｛' | '《' | '〈' | '「' | '『'
    )
}

fn is_forbidden_line_start(ch: char) -> bool {
    matches!(
        ch,
        ')' | ']' | '}' | '）' | '］' | '｝' | '》' | '〉' | '」' | '』'
    )
}

#[cfg(test)]
mod tests {
    use std::io::{Cursor, Write};

    use zip::write::SimpleFileOptions;

    use super::{
        convert_docx_bytes, plain_paragraph, read_docx_document, wrap_styled_text_with_font,
        DocxBlock, DocxImageData,
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

    fn create_docx_with_settings(document_xml: &str, settings_xml: &str) -> Vec<u8> {
        let mut output = Cursor::new(Vec::new());
        {
            let mut archive = zip::ZipWriter::new(&mut output);
            for (path, data) in [
                ("word/document.xml", document_xml.as_bytes()),
                ("word/settings.xml", settings_xml.as_bytes()),
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
        let lines = wrap_styled_text_with_font(text, 40.0, 12.0, false, false, None);

        assert!(lines.len() > 1);
        assert_eq!(lines.concat(), text);
        assert!(lines.iter().all(|line| {
            crate::pdf::styled_text_width_with_font(line, 12.0, false, false, None) <= 40.0
        }));
    }

    #[test]
    fn keeps_cjk_opening_punctuation_off_line_end() {
        let text = "机房（扶梯上机舱 ）";
        let max_width =
            crate::pdf::styled_text_width_with_font("机房（", 8.0, true, false, Some("宋体"));

        let lines = wrap_styled_text_with_font(text, max_width, 8.0, true, false, Some("宋体"));

        assert_eq!(lines, vec!["机房", "（扶梯", "上机", "舱）"]);
        assert!(lines.iter().all(|line| !line.ends_with('（')));
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
        assert_eq!(table.alignment, super::TextAlignment::Left);
        assert_eq!(table.rows[0].cells.len(), 2);
        assert_eq!(table.rows[0].cells[0].runs[0].font_size, 8.0);
        assert!(table.rows[0].cells[0].runs[0].bold);
        assert_eq!(table.rows[0].cells[0].runs[0].text, "Header");
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
    fn reads_table_spans_merges_and_declared_height() {
        let input = create_docx(
            r#"<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body><w:tbl><w:tblPr><w:jc w:val="center"/></w:tblPr><w:tblGrid><w:gridCol w:w="500"/><w:gridCol w:w="1000"/><w:gridCol w:w="1200"/></w:tblGrid><w:tr><w:trPr><w:trHeight w:val="240"/></w:trPr><w:tc><w:tcPr><w:tcW w:w="500" w:type="dxa"/><w:vMerge w:val="restart"/><w:vAlign w:val="center"/></w:tcPr><w:p><w:r><w:t>A</w:t></w:r></w:p></w:tc><w:tc><w:tcPr><w:gridSpan w:val="2"/></w:tcPr><w:p><w:r><w:t>B</w:t></w:r></w:p></w:tc></w:tr><w:tr><w:tc><w:tcPr><w:vMerge/></w:tcPr><w:p/></w:tc><w:tc><w:tcPr><w:gridSpan w:val="2"/></w:tcPr><w:p><w:r><w:t>C</w:t></w:r></w:p></w:tc></w:tr></w:tbl></w:body></w:document>"#,
        );

        let document = read_docx_document(&input).unwrap();
        let DocxBlock::Table(table) = &document.blocks[0] else {
            panic!("expected table block");
        };

        assert_eq!(table.alignment, super::TextAlignment::Center);
        assert_eq!(table.rows[0].height, Some(12.0));
        assert_eq!(table.rows[0].cells[1].grid_span, 2);
        assert_eq!(
            table.rows[0].cells[0].vertical_merge,
            super::VerticalMerge::Restart
        );
        assert_eq!(
            table.rows[1].cells[0].vertical_merge,
            super::VerticalMerge::Continue
        );
        assert_eq!(
            table.rows[0].cells[0].vertical_alignment,
            super::VerticalAlignment::Center
        );
    }

    #[test]
    fn reads_table_font_highlight_margins_and_borders() {
        let input = create_docx(
            r#"<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body><w:tbl><w:tblPr><w:tblCellMar><w:left w:w="0" w:type="dxa"/><w:right w:w="0" w:type="dxa"/></w:tblCellMar></w:tblPr><w:tblGrid><w:gridCol w:w="2000"/></w:tblGrid><w:tr><w:tc><w:tcPr><w:tcBorders><w:top w:val="dotDash" w:sz="4"/><w:right w:val="nil"/></w:tcBorders></w:tcPr><w:p><w:pPr><w:spacing w:before="180" w:after="120" w:line="276"/></w:pPr><w:r><w:t>□</w:t></w:r><w:r><w:rPr><w:rFonts w:ascii="Arial" w:eastAsia="宋体"/><w:highlight w:val="yellow"/><w:sz w:val="18"/></w:rPr><w:t>高亮</w:t></w:r></w:p></w:tc></w:tr></w:tbl></w:body></w:document>"#,
        );

        let document = read_docx_document(&input).unwrap();
        let DocxBlock::Table(table) = &document.blocks[0] else {
            panic!("expected table block");
        };
        let cell = &table.rows[0].cells[0];

        assert_eq!(table.cell_margin_left, 0.0);
        assert_eq!(table.cell_margin_right, 0.0);
        assert_eq!(cell.spacing_before, 9.0);
        assert_eq!(cell.spacing_after, 6.0);
        assert_eq!(cell.line_spacing, 1.15);
        assert_eq!(cell.runs.len(), 2);
        assert_eq!(cell.runs[0].font_name, None);
        assert_eq!(cell.runs[1].font_name.as_deref(), Some("宋体"));
        assert_eq!(cell.runs[1].font_size, 9.0);
        assert_eq!(
            cell.runs[1].highlight,
            Some(crate::PdfColor::new(1.0, 1.0, 0.0))
        );
        assert_eq!(
            cell.borders.top.map(|border| border.pattern),
            Some(super::BorderPattern::DotDash)
        );
        assert_eq!(cell.borders.right, None);
    }

    #[test]
    fn reads_word_2007_compatibility_mode() {
        let input = create_docx_with_settings(
            r#"<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body><w:p><w:r><w:t>Legacy</w:t></w:r></w:p></w:body></w:document>"#,
            r#"<w:settings xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:compat><w:compatSetting w:name="compatibilityMode" w:uri="http://schemas.microsoft.com/office/word" w:val="12"/></w:compat></w:settings>"#,
        );

        let document = read_docx_document(&input).unwrap();

        assert_eq!(document.compatibility_mode, 12);
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

    #[test]
    fn anchors_vml_textbox_image_once() {
        let input = create_docx_with_image(
            r#"<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><w:body><w:p><w:pPr><w:jc w:val="center"/></w:pPr><w:r><w:t>Title</w:t></w:r><w:r><w:pict><v:shape style="position:absolute;margin-left:393.95pt;margin-top:-1.9pt;width:116.95pt;height:52.7pt"><v:textbox><w:txbxContent><w:p><w:r><w:drawing><wp:inline><wp:extent cx="914400" cy="457200"/><a:graphic><a:graphicData><a:blip r:embed="rId1"/></a:graphicData></a:graphic></wp:inline></w:drawing></w:r></w:p></w:txbxContent></v:textbox></v:shape></w:pict></w:r></w:p></w:body></w:document>"#,
            r#"<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/image" Target="media/image1.png"/></Relationships>"#,
        );

        let document = read_docx_document(&input).unwrap();

        assert_eq!(document.blocks.len(), 1);
        let DocxBlock::Paragraph(paragraph) = &document.blocks[0] else {
            panic!("expected paragraph block");
        };
        assert_eq!(paragraph.runs.len(), 1);
        assert_eq!(paragraph.runs[0].text, "Title");
        assert_eq!(paragraph.floating_images.len(), 1);
        assert_eq!(paragraph.floating_images[0].offset_x, 393.95);
        assert_eq!(paragraph.floating_images[0].offset_y, -1.9);
        assert_eq!(paragraph.floating_images[0].line_top_leading, 2.0);
    }
}
