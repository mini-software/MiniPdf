use std::collections::HashMap;
use std::io::{Cursor, Read};

use roxmltree::{Document, Node};
use zip::ZipArchive;

use crate::pdf::styled_text_width_with_font;
use crate::pdf::PdfPathCommand;
use crate::{ConversionOptions, MiniPdfError, PdfColor, PdfDocument, Result};

const EMUS_PER_POINT: f32 = 12_700.0;
const DEFAULT_SLIDE_WIDTH_EMU: i64 = 9_144_000;
const DEFAULT_SLIDE_HEIGHT_EMU: i64 = 6_858_000;
const OFFICE_RELATIONSHIPS_NS: &str =
    "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

#[derive(Debug)]
struct PptxDocument {
    width: f32,
    height: f32,
    slides: Vec<PptxSlide>,
}

#[derive(Debug)]
struct PptxSlide {
    width: f32,
    height: f32,
    background: Option<PdfColor>,
    elements: Vec<PptxElement>,
}

#[derive(Debug)]
enum PptxElement {
    Shape(PptxShape),
    Picture(PptxPicture),
    Line(PptxLine),
}

#[derive(Debug)]
struct PptxShape {
    shape_type: String,
    bounds: PptxRect,
    fill: Option<PdfColor>,
    outline: Option<PptxOutline>,
    paragraphs: Vec<PptxParagraph>,
    text_body: PptxTextBody,
}

#[derive(Debug)]
struct PptxPicture {
    bounds: PptxRect,
    data: PptxImageData,
}

#[derive(Debug)]
enum PptxImageData {
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
    Svg {
        data: Vec<u8>,
        crop: PptxCrop,
    },
}

#[derive(Debug, Clone, Copy, Default)]
struct PptxCrop {
    left: f32,
    top: f32,
    right: f32,
    bottom: f32,
}

#[derive(Debug)]
struct PptxLine {
    x1: f32,
    y1: f32,
    x2: f32,
    y2: f32,
    outline: PptxOutline,
}

#[derive(Debug, Clone)]
struct PptxOutline {
    color: PdfColor,
    width: f32,
    dash_pattern: Vec<f32>,
}

#[derive(Debug)]
struct PptxParagraph {
    runs: Vec<PptxRun>,
    alignment: TextAlignment,
    margin_left: f32,
    indent: f32,
    space_before: f32,
    line_spacing: f32,
}

#[derive(Debug, Clone)]
struct PptxRun {
    text: String,
    font_size: f32,
    color: PdfColor,
    bold: bool,
    italic: bool,
    underline: bool,
    font_name: Option<String>,
}

#[derive(Debug, Clone, Copy)]
enum TextAlignment {
    Left,
    Center,
    Right,
}

#[derive(Debug, Clone, Copy)]
enum VerticalAnchor {
    Top,
    Middle,
    Bottom,
}

#[derive(Debug, Clone, Copy)]
struct PptxTextBody {
    left_inset: f32,
    top_inset: f32,
    right_inset: f32,
    bottom_inset: f32,
    anchor: VerticalAnchor,
}

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
struct PlaceholderKey {
    placeholder_type: String,
    index: Option<String>,
}

#[derive(Debug, Clone)]
struct PlaceholderStyle {
    bounds: Option<PptxRect>,
    shape_type: Option<String>,
    fill: Option<PdfColor>,
    outline: Option<PptxOutline>,
    text_body: PptxTextBody,
    paragraphs: HashMap<usize, ParagraphDefaults>,
}

#[derive(Debug, Clone, Default)]
struct ParagraphDefaults {
    alignment: Option<TextAlignment>,
    margin_left: Option<f32>,
    indent: Option<f32>,
    space_before: Option<f32>,
    line_spacing: Option<f32>,
    bullet: Option<bool>,
    run: RunDefaults,
}

#[derive(Debug, Clone, Default)]
struct RunDefaults {
    font_size: Option<f32>,
    color: Option<PdfColor>,
    bold: Option<bool>,
    italic: Option<bool>,
    underline: Option<bool>,
    latin_font: Option<String>,
    east_asian_font: Option<String>,
    complex_font: Option<String>,
}

impl Default for PptxTextBody {
    fn default() -> Self {
        Self {
            left_inset: 0.0,
            top_inset: 0.0,
            right_inset: 0.0,
            bottom_inset: 0.0,
            anchor: VerticalAnchor::Top,
        }
    }
}

#[derive(Debug, Clone, Copy)]
struct PptxRect {
    x: f32,
    y: f32,
    width: f32,
    height: f32,
}

#[derive(Debug, Clone, Copy)]
struct CoordinateMap {
    target_x: f64,
    target_y: f64,
    target_width: f64,
    target_height: f64,
    source_x: f64,
    source_y: f64,
    source_width: f64,
    source_height: f64,
}

impl CoordinateMap {
    fn root(width: i64, height: i64) -> Self {
        Self {
            target_x: 0.0,
            target_y: 0.0,
            target_width: width as f64,
            target_height: height as f64,
            source_x: 0.0,
            source_y: 0.0,
            source_width: width as f64,
            source_height: height as f64,
        }
    }

    fn map_x(self, value: i64) -> f64 {
        self.target_x
            + (value as f64 - self.source_x) * self.target_width / self.source_width.max(1.0)
    }

    fn map_y(self, value: i64) -> f64 {
        self.target_y
            + (value as f64 - self.source_y) * self.target_height / self.source_height.max(1.0)
    }

    fn map_rect(self, x: i64, y: i64, width: i64, height: i64) -> PptxRect {
        let left = self.map_x(x);
        let top = self.map_y(y);
        let right = self.map_x(x + width);
        let bottom = self.map_y(y + height);
        PptxRect {
            x: (left / EMUS_PER_POINT as f64) as f32,
            y: (top / EMUS_PER_POINT as f64) as f32,
            width: ((right - left) / EMUS_PER_POINT as f64) as f32,
            height: ((bottom - top) / EMUS_PER_POINT as f64) as f32,
        }
    }
}

#[derive(Debug, Clone)]
struct Relationship {
    target: String,
    rel_type: String,
    external: bool,
    relationship_path: String,
}

pub(crate) fn convert_pptx_bytes(input: &[u8], options: &ConversionOptions) -> Result<Vec<u8>> {
    let presentation = read_pptx(input)?;
    Ok(render_pptx(&presentation, options).to_bytes())
}

fn read_pptx(input: &[u8]) -> Result<PptxDocument> {
    let mut archive = ZipArchive::new(Cursor::new(input))?;
    let presentation_xml =
        read_zip_bytes(&mut archive, "ppt/presentation.xml")?.ok_or_else(|| {
            MiniPdfError::InvalidInput("PPTX package is missing ppt/presentation.xml".to_owned())
        })?;
    let presentation_text = String::from_utf8(presentation_xml)
        .map_err(|_| MiniPdfError::InvalidInput("presentation.xml is not UTF-8".to_owned()))?;
    let presentation = Document::parse(&presentation_text)?;
    let relationships = read_relationships(&mut archive, "ppt/presentation.xml")?;
    let theme = read_theme_colors(&mut archive, &relationships)?;

    let slide_size = presentation
        .descendants()
        .find(|node| is_element(*node, "sldSz"));
    let width_emu = integer_attribute(slide_size, "cx").unwrap_or(DEFAULT_SLIDE_WIDTH_EMU);
    let height_emu = integer_attribute(slide_size, "cy").unwrap_or(DEFAULT_SLIDE_HEIGHT_EMU);
    let width = width_emu as f32 / EMUS_PER_POINT;
    let height = height_emu as f32 / EMUS_PER_POINT;

    let mut slide_paths = Vec::new();
    for slide_id in presentation
        .descendants()
        .filter(|node| is_element(*node, "sldId"))
    {
        let Some(id) = slide_id
            .attributes()
            .find(|attribute| {
                attribute.name() == "id" && attribute.namespace() == Some(OFFICE_RELATIONSHIPS_NS)
            })
            .map(|attribute| attribute.value())
        else {
            continue;
        };
        let Some(relationship) = relationships.get(id) else {
            continue;
        };
        slide_paths.push(resolve_part_target(
            "ppt/presentation.xml",
            &relationship.target,
        ));
    }
    if slide_paths.is_empty() {
        slide_paths = archive
            .file_names()
            .filter(|name| name.starts_with("ppt/slides/slide") && name.ends_with(".xml"))
            .map(str::to_owned)
            .collect();
        slide_paths.sort_by_key(|path| natural_slide_number(path));
    }

    let mut slides = Vec::new();
    for slide_path in slide_paths {
        if let Some(slide) = read_slide(
            &mut archive,
            &slide_path,
            width,
            height,
            width_emu,
            height_emu,
            &theme,
        )? {
            slides.push(slide);
        }
    }
    if slides.is_empty() {
        let mut fallback_paths: Vec<_> = archive
            .file_names()
            .filter(|name| name.starts_with("ppt/slides/slide") && name.ends_with(".xml"))
            .map(str::to_owned)
            .collect();
        fallback_paths.sort_by_key(|path| natural_slide_number(path));
        for slide_path in fallback_paths {
            if let Some(slide) = read_slide(
                &mut archive,
                &slide_path,
                width,
                height,
                width_emu,
                height_emu,
                &theme,
            )? {
                slides.push(slide);
            }
        }
    }

    Ok(PptxDocument {
        width,
        height,
        slides,
    })
}

#[allow(clippy::too_many_arguments)]
fn read_slide(
    archive: &mut ZipArchive<Cursor<&[u8]>>,
    slide_path: &str,
    width: f32,
    height: f32,
    width_emu: i64,
    height_emu: i64,
    theme: &HashMap<String, PdfColor>,
) -> Result<Option<PptxSlide>> {
    let Some(slide_bytes) = read_zip_bytes(archive, slide_path)? else {
        return Ok(None);
    };
    let slide_text = String::from_utf8(slide_bytes)
        .map_err(|_| MiniPdfError::InvalidInput(format!("{slide_path} is not UTF-8")))?;
    let slide = Document::parse(&slide_text)?;
    let relationships = read_relationships(archive, slide_path)?;
    let root_map = CoordinateMap::root(width_emu, height_emu);
    let mut elements = Vec::new();

    let layout = read_related_xml(archive, slide_path, &relationships, "/slideLayout")?;
    let master = if let Some((layout_path, _, layout_relationships)) = &layout {
        read_related_xml(archive, layout_path, layout_relationships, "/slideMaster")?
    } else {
        None
    };

    let master_document = master
        .as_ref()
        .map(|(_, xml, _)| Document::parse(xml))
        .transpose()?;
    let layout_document = layout
        .as_ref()
        .map(|(_, xml, _)| Document::parse(xml))
        .transpose()?;
    let slide_theme = apply_color_map(
        theme,
        &slide,
        layout_document.as_ref(),
        master_document.as_ref(),
    );
    let mut placeholder_defaults = HashMap::new();
    if let Some(document) = &master_document {
        add_placeholder_defaults(document, &slide_theme, root_map, &mut placeholder_defaults);
    }
    if let Some(document) = &layout_document {
        add_placeholder_defaults(document, &slide_theme, root_map, &mut placeholder_defaults);
    }

    if let (Some(master_document), Some((_, _, master_relationships))) = (&master_document, &master)
    {
        read_shape_tree(
            archive,
            master_document,
            master_relationships,
            &slide_theme,
            root_map,
            &mut elements,
            false,
            None,
        )?;
    }
    if let (Some(layout_document), Some((_, _, layout_relationships))) = (&layout_document, &layout)
    {
        read_shape_tree(
            archive,
            layout_document,
            layout_relationships,
            &slide_theme,
            root_map,
            &mut elements,
            false,
            None,
        )?;
    }
    read_shape_tree(
        archive,
        &slide,
        &relationships,
        &slide_theme,
        root_map,
        &mut elements,
        true,
        Some(&placeholder_defaults),
    )?;

    let background = read_background(&slide, &slide_theme)
        .or_else(|| {
            layout_document
                .as_ref()
                .and_then(|doc| read_background(doc, &slide_theme))
        })
        .or_else(|| {
            master_document
                .as_ref()
                .and_then(|doc| read_background(doc, &slide_theme))
        });

    Ok(Some(PptxSlide {
        width,
        height,
        background,
        elements,
    }))
}

#[allow(clippy::too_many_arguments)]
fn read_shape_tree(
    archive: &mut ZipArchive<Cursor<&[u8]>>,
    document: &Document<'_>,
    relationships: &HashMap<String, Relationship>,
    theme: &HashMap<String, PdfColor>,
    coordinate_map: CoordinateMap,
    elements: &mut Vec<PptxElement>,
    include_placeholders: bool,
    placeholder_defaults: Option<&HashMap<PlaceholderKey, PlaceholderStyle>>,
) -> Result<()> {
    let Some(shape_tree) = document
        .descendants()
        .find(|node| is_element(*node, "spTree"))
    else {
        return Ok(());
    };
    read_shape_children(
        archive,
        shape_tree,
        relationships,
        theme,
        coordinate_map,
        elements,
        include_placeholders,
        placeholder_defaults,
    )
}

#[allow(clippy::too_many_arguments)]
fn read_shape_children(
    archive: &mut ZipArchive<Cursor<&[u8]>>,
    container: Node<'_, '_>,
    relationships: &HashMap<String, Relationship>,
    theme: &HashMap<String, PdfColor>,
    coordinate_map: CoordinateMap,
    elements: &mut Vec<PptxElement>,
    include_placeholders: bool,
    placeholder_defaults: Option<&HashMap<PlaceholderKey, PlaceholderStyle>>,
) -> Result<()> {
    for child_node in container.children().filter(Node::is_element) {
        let placeholder = child_node.descendants().any(|node| is_element(node, "ph"));
        if placeholder && !include_placeholders {
            continue;
        }
        match child_node.tag_name().name() {
            "sp" => read_shape(
                child_node,
                theme,
                coordinate_map,
                elements,
                placeholder_defaults,
            ),
            "cxnSp" => read_connector(child_node, theme, coordinate_map, elements),
            "pic" => read_picture(archive, child_node, relationships, coordinate_map, elements)?,
            "graphicFrame" => read_graphic_frame(
                archive,
                child_node,
                relationships,
                theme,
                coordinate_map,
                elements,
            )?,
            "grpSp" => {
                let group_map = child(child_node, "grpSpPr")
                    .and_then(|node| child(node, "xfrm"))
                    .map(|transform| create_group_map(transform, coordinate_map))
                    .unwrap_or(coordinate_map);
                read_shape_children(
                    archive,
                    child_node,
                    relationships,
                    theme,
                    group_map,
                    elements,
                    include_placeholders,
                    placeholder_defaults,
                )?;
            }
            _ => {}
        }
    }
    Ok(())
}

fn read_shape(
    shape: Node<'_, '_>,
    theme: &HashMap<String, PdfColor>,
    coordinate_map: CoordinateMap,
    elements: &mut Vec<PptxElement>,
    placeholder_defaults: Option<&HashMap<PlaceholderKey, PlaceholderStyle>>,
) {
    let inherited = read_placeholder_key(shape).and_then(|key| {
        placeholder_defaults.and_then(|defaults| {
            defaults.get(&key).or_else(|| {
                defaults.get(&PlaceholderKey {
                    placeholder_type: key.placeholder_type,
                    index: None,
                })
            })
        })
    });
    let shape_properties = child(shape, "spPr");
    let Some(bounds) = shape_properties
        .and_then(|node| child(node, "xfrm"))
        .map(|transform| read_bounds(transform, coordinate_map))
        .or_else(|| inherited.and_then(|style| style.bounds))
    else {
        return;
    };
    if bounds.width <= 0.0 || bounds.height <= 0.0 {
        return;
    }
    let shape_type = shape_properties
        .and_then(|node| child(node, "prstGeom"))
        .and_then(|node| node.attribute("prst"))
        .map(str::to_owned)
        .or_else(|| inherited.and_then(|style| style.shape_type.clone()))
        .unwrap_or_else(|| "rect".to_owned());
    let outline = shape_properties
        .and_then(|node| child(node, "ln"))
        .and_then(|node| read_outline(node, theme))
        .or_else(|| read_style_outline(shape, theme))
        .or_else(|| inherited.and_then(|style| style.outline.clone()));
    if shape_type.eq_ignore_ascii_case("line") {
        add_line_from_bounds(
            shape_properties.and_then(|node| child(node, "xfrm")),
            bounds,
            outline.unwrap_or_else(default_outline),
            elements,
        );
        return;
    }
    let fill = shape_properties
        .and_then(|node| read_fill(node, theme))
        .or_else(|| read_style_fill(shape, theme))
        .or_else(|| inherited.and_then(|style| style.fill));
    let paragraphs = child(shape, "txBody")
        .map(|body| read_paragraphs(body, theme, inherited))
        .unwrap_or_default();
    let text_body = child(shape, "txBody")
        .and_then(|body| child(body, "bodyPr"))
        .map(|body| read_text_body(body, inherited.map(|style| style.text_body)))
        .or_else(|| inherited.map(|style| style.text_body))
        .unwrap_or_default();
    if fill.is_some() || outline.is_some() || !paragraphs.is_empty() {
        elements.push(PptxElement::Shape(PptxShape {
            shape_type,
            bounds,
            fill,
            outline,
            paragraphs,
            text_body,
        }));
    }
}

fn read_connector(
    connector: Node<'_, '_>,
    theme: &HashMap<String, PdfColor>,
    coordinate_map: CoordinateMap,
    elements: &mut Vec<PptxElement>,
) {
    let Some(properties) = child(connector, "spPr") else {
        return;
    };
    let Some(transform) = child(properties, "xfrm") else {
        return;
    };
    let bounds = read_bounds(transform, coordinate_map);
    let outline = child(properties, "ln")
        .and_then(|node| read_outline(node, theme))
        .unwrap_or_else(default_outline);
    add_line_from_bounds(Some(transform), bounds, outline, elements);
}

fn read_picture(
    archive: &mut ZipArchive<Cursor<&[u8]>>,
    picture: Node<'_, '_>,
    relationships: &HashMap<String, Relationship>,
    coordinate_map: CoordinateMap,
    elements: &mut Vec<PptxElement>,
) -> Result<()> {
    if picture
        .descendants()
        .find(|node| is_element(*node, "cNvPr"))
        .and_then(|node| node.attribute("hidden"))
        .is_some_and(read_bool)
    {
        return Ok(());
    }
    let Some(transform) = picture
        .children()
        .find(|node| is_element(*node, "spPr"))
        .and_then(|node| child(node, "xfrm"))
    else {
        return Ok(());
    };
    let bounds = read_bounds(transform, coordinate_map);
    if bounds.width <= 0.0 || bounds.height <= 0.0 {
        return Ok(());
    }
    let Some(embed_id) = picture
        .descendants()
        .filter(|node| is_element(*node, "blip") || is_element(*node, "svgBlip"))
        .find_map(|node| attribute_by_local_name(node, "embed"))
    else {
        return Ok(());
    };
    let Some(relationship) = relationships.get(embed_id) else {
        return Ok(());
    };
    if relationship.external {
        return Ok(());
    }
    let source_path = relationship_source_path(relationship);
    let media_path = resolve_part_target(&source_path, &relationship.target);
    let Some(bytes) = read_zip_bytes(archive, &media_path)? else {
        return Ok(());
    };
    let crop = picture
        .descendants()
        .find(|node| is_element(*node, "srcRect"))
        .map(|node| PptxCrop {
            left: crop_attribute(node, "l"),
            top: crop_attribute(node, "t"),
            right: crop_attribute(node, "r"),
            bottom: crop_attribute(node, "b"),
        })
        .unwrap_or_default();
    let data = if media_path.to_ascii_lowercase().ends_with(".svg") || bytes.starts_with(b"<svg") {
        PptxImageData::Svg { data: bytes, crop }
    } else if let Some((width, height)) = jpeg_dimensions(&bytes) {
        PptxImageData::Jpeg {
            data: bytes,
            width,
            height,
        }
    } else if let Ok(image) = image::load_from_memory(&bytes) {
        let width = u16::try_from(image.width()).unwrap_or(u16::MAX);
        let height = u16::try_from(image.height()).unwrap_or(u16::MAX);
        PptxImageData::Rgba {
            data: image.into_rgba8().into_raw(),
            width,
            height,
        }
    } else {
        return Ok(());
    };
    elements.push(PptxElement::Picture(PptxPicture { bounds, data }));
    Ok(())
}

fn read_graphic_frame(
    archive: &mut ZipArchive<Cursor<&[u8]>>,
    frame: Node<'_, '_>,
    relationships: &HashMap<String, Relationship>,
    theme: &HashMap<String, PdfColor>,
    coordinate_map: CoordinateMap,
    elements: &mut Vec<PptxElement>,
) -> Result<()> {
    let Some(transform) = child(frame, "xfrm") else {
        return Ok(());
    };
    let bounds = read_bounds(transform, coordinate_map);
    let graphic_data = frame
        .descendants()
        .find(|node| is_element(*node, "graphicData"));
    if graphic_data
        .and_then(|node| node.attribute("uri"))
        .is_some_and(|uri| uri.ends_with("/diagram"))
    {
        read_diagram(archive, relationships, theme, transform, bounds, elements)?;
        return Ok(());
    }
    let Some(table) = frame.descendants().find(|node| is_element(*node, "tbl")) else {
        return Ok(());
    };
    let widths: Vec<i64> = table
        .descendants()
        .filter(|node| is_element(*node, "gridCol"))
        .filter_map(|node| node.attribute("w").and_then(|value| value.parse().ok()))
        .collect();
    let rows: Vec<_> = table
        .children()
        .filter(|node| is_element(*node, "tr"))
        .collect();
    let total_width: i64 = widths.iter().sum();
    let total_height: i64 = rows
        .iter()
        .filter_map(|row| {
            row.attribute("h")
                .and_then(|value| value.parse::<i64>().ok())
        })
        .sum();
    if widths.is_empty() || total_width <= 0 || total_height <= 0 {
        return Ok(());
    }
    let mut top = bounds.y;
    for row in rows {
        let row_height = bounds.height
            * row
                .attribute("h")
                .and_then(|value| value.parse::<f32>().ok())
                .unwrap_or(0.0)
            / total_height as f32;
        let mut left = bounds.x;
        let mut column = 0;
        for cell_node in row.children().filter(|node| is_element(*node, "tc")) {
            if column >= widths.len() {
                break;
            }
            let span = cell_node
                .attribute("gridSpan")
                .and_then(|value| value.parse::<usize>().ok())
                .unwrap_or(1)
                .max(1);
            let spanned_width: i64 = widths.iter().skip(column).take(span).sum();
            let cell_width = bounds.width * spanned_width as f32 / total_width as f32;
            let cell_bounds = PptxRect {
                x: left,
                y: top,
                width: cell_width,
                height: row_height,
            };
            let properties = child(cell_node, "tcPr");
            let fill = properties.and_then(|node| read_fill(node, theme));
            let paragraphs = child(cell_node, "txBody")
                .map(|body| read_paragraphs(body, theme, None))
                .unwrap_or_default();
            elements.push(PptxElement::Shape(PptxShape {
                shape_type: "rect".to_owned(),
                bounds: cell_bounds,
                fill,
                outline: None,
                paragraphs,
                text_body: PptxTextBody::default(),
            }));
            add_table_borders(properties, cell_bounds, theme, elements);
            left += cell_width;
            column += span;
        }
        top += row_height;
    }
    Ok(())
}

fn add_table_borders(
    properties: Option<Node<'_, '_>>,
    bounds: PptxRect,
    theme: &HashMap<String, PdfColor>,
    elements: &mut Vec<PptxElement>,
) {
    let Some(properties) = properties else {
        return;
    };
    for (name, x1, y1, x2, y2) in [
        (
            "lnL",
            bounds.x,
            bounds.y,
            bounds.x,
            bounds.y + bounds.height,
        ),
        (
            "lnR",
            bounds.x + bounds.width,
            bounds.y,
            bounds.x + bounds.width,
            bounds.y + bounds.height,
        ),
        ("lnT", bounds.x, bounds.y, bounds.x + bounds.width, bounds.y),
        (
            "lnB",
            bounds.x,
            bounds.y + bounds.height,
            bounds.x + bounds.width,
            bounds.y + bounds.height,
        ),
    ] {
        if let Some(outline) = child(properties, name).and_then(|node| read_outline(node, theme)) {
            elements.push(PptxElement::Line(PptxLine {
                x1,
                y1,
                x2,
                y2,
                outline,
            }));
        }
    }
}

fn read_diagram(
    archive: &mut ZipArchive<Cursor<&[u8]>>,
    relationships: &HashMap<String, Relationship>,
    theme: &HashMap<String, PdfColor>,
    frame_transform: Node<'_, '_>,
    bounds: PptxRect,
    elements: &mut Vec<PptxElement>,
) -> Result<()> {
    let Some(relationship) = relationships.values().find(|relationship| {
        relationship.rel_type.ends_with("/diagramDrawing") && !relationship.external
    }) else {
        return Ok(());
    };
    let drawing_path = resolve_part_target(
        &relationship_source_path(relationship),
        &relationship.target,
    );
    let Some(bytes) = read_zip_bytes(archive, &drawing_path)? else {
        return Ok(());
    };
    let text = String::from_utf8(bytes)
        .map_err(|_| MiniPdfError::InvalidInput(format!("{drawing_path} is not UTF-8")))?;
    let drawing = Document::parse(&text)?;
    let extent = child(frame_transform, "ext");
    let frame_width = integer_attribute(extent, "cx").unwrap_or(0);
    let frame_height = integer_attribute(extent, "cy").unwrap_or(0);
    if frame_width <= 0 || frame_height <= 0 {
        return Ok(());
    }
    let map = CoordinateMap {
        target_x: bounds.x as f64 * EMUS_PER_POINT as f64,
        target_y: bounds.y as f64 * EMUS_PER_POINT as f64,
        target_width: bounds.width as f64 * EMUS_PER_POINT as f64,
        target_height: bounds.height as f64 * EMUS_PER_POINT as f64,
        source_x: 0.0,
        source_y: 0.0,
        source_width: frame_width as f64,
        source_height: frame_height as f64,
    };
    let Some(shape_tree) = drawing
        .descendants()
        .find(|node| is_element(*node, "spTree"))
    else {
        return Ok(());
    };
    for shape in shape_tree.children().filter(|node| is_element(*node, "sp")) {
        read_shape(shape, theme, map, elements, None);
    }
    Ok(())
}

fn add_placeholder_defaults(
    document: &Document<'_>,
    theme: &HashMap<String, PdfColor>,
    coordinate_map: CoordinateMap,
    defaults: &mut HashMap<PlaceholderKey, PlaceholderStyle>,
) {
    let Some(shape_tree) = document
        .descendants()
        .find(|node| is_element(*node, "spTree"))
    else {
        return;
    };
    for shape in shape_tree
        .descendants()
        .filter(|node| is_element(*node, "sp"))
    {
        let Some(key) = read_placeholder_key(shape) else {
            continue;
        };
        let properties = child(shape, "spPr");
        let text_body = child(shape, "txBody");
        let body_properties = text_body.and_then(|node| child(node, "bodyPr"));
        let style = PlaceholderStyle {
            bounds: properties
                .and_then(|node| child(node, "xfrm"))
                .map(|node| read_bounds(node, coordinate_map)),
            shape_type: properties
                .and_then(|node| child(node, "prstGeom"))
                .and_then(|node| node.attribute("prst"))
                .map(str::to_owned),
            fill: properties
                .and_then(|node| read_fill(node, theme))
                .or_else(|| read_style_fill(shape, theme)),
            outline: properties
                .and_then(|node| child(node, "ln"))
                .and_then(|node| read_outline(node, theme))
                .or_else(|| read_style_outline(shape, theme)),
            text_body: body_properties
                .map(|node| read_text_body(node, None))
                .unwrap_or_default(),
            paragraphs: text_body
                .map(|node| read_paragraph_defaults(node, theme))
                .unwrap_or_default(),
        };
        defaults.insert(key.clone(), style.clone());
        defaults
            .entry(PlaceholderKey {
                placeholder_type: key.placeholder_type,
                index: None,
            })
            .or_insert(style);
    }
}

fn read_placeholder_key(shape: Node<'_, '_>) -> Option<PlaceholderKey> {
    let placeholder = shape.descendants().find(|node| is_element(*node, "ph"))?;
    Some(PlaceholderKey {
        placeholder_type: placeholder.attribute("type").unwrap_or("body").to_owned(),
        index: placeholder.attribute("idx").map(str::to_owned),
    })
}

fn read_paragraph_defaults(
    text_body: Node<'_, '_>,
    theme: &HashMap<String, PdfColor>,
) -> HashMap<usize, ParagraphDefaults> {
    let Some(list_style) = child(text_body, "lstStyle") else {
        return HashMap::new();
    };
    list_style
        .children()
        .filter(Node::is_element)
        .filter_map(|properties| {
            let name = properties.tag_name().name();
            let level = name
                .strip_prefix("lvl")?
                .strip_suffix("pPr")?
                .parse::<usize>()
                .ok()?
                .saturating_sub(1);
            Some((
                level,
                ParagraphDefaults {
                    alignment: read_alignment(Some(properties)),
                    margin_left: emu_attribute(Some(properties), "marL"),
                    indent: emu_attribute(Some(properties), "indent"),
                    space_before: read_space_before(Some(properties)),
                    line_spacing: read_line_spacing(Some(properties)),
                    bullet: read_bullet_setting(properties).or_else(|| (level > 0).then_some(true)),
                    run: child(properties, "defRPr")
                        .map(|node| read_run_defaults(node, theme))
                        .unwrap_or_default(),
                },
            ))
        })
        .collect()
}

fn read_paragraphs(
    text_body: Node<'_, '_>,
    theme: &HashMap<String, PdfColor>,
    inherited_style: Option<&PlaceholderStyle>,
) -> Vec<PptxParagraph> {
    text_body
        .children()
        .filter(|node| is_element(*node, "p"))
        .map(|paragraph| {
            let properties = child(paragraph, "pPr");
            let level = properties
                .and_then(|node| node.attribute("lvl"))
                .and_then(|value| value.parse::<usize>().ok())
                .unwrap_or(0)
                .min(8);
            let inherited = inherited_style.and_then(|style| {
                style
                    .paragraphs
                    .get(&level)
                    .or_else(|| style.paragraphs.get(&0))
            });
            let default_run = properties.and_then(|node| child(node, "defRPr"));
            let is_bullet = properties
                .and_then(read_bullet_setting)
                .or_else(|| inherited.and_then(|style| style.bullet))
                .unwrap_or(false);
            let mut runs = Vec::new();
            let mut has_text = false;
            for run_node in paragraph.children().filter(Node::is_element) {
                match run_node.tag_name().name() {
                    "r" | "fld" => {
                        let Some(text_node) =
                            run_node.descendants().find(|node| is_element(*node, "t"))
                        else {
                            continue;
                        };
                        let mut text = text_node.text().unwrap_or_default().to_owned();
                        if is_bullet && !has_text {
                            text.insert_str(0, "\u{2022} ");
                        }
                        if !text.is_empty() {
                            runs.push(read_run(
                                text,
                                child(run_node, "rPr"),
                                default_run,
                                inherited.map(|style| &style.run),
                                theme,
                            ));
                            has_text = true;
                        }
                    }
                    "br" => runs.push(read_run(
                        "\n".to_owned(),
                        child(run_node, "rPr"),
                        default_run,
                        inherited.map(|style| &style.run),
                        theme,
                    )),
                    "tab" => runs.push(read_run(
                        "\t".to_owned(),
                        child(run_node, "rPr"),
                        default_run,
                        inherited.map(|style| &style.run),
                        theme,
                    )),
                    _ => {}
                }
            }
            PptxParagraph {
                runs,
                alignment: read_alignment(properties)
                    .or_else(|| inherited.and_then(|style| style.alignment))
                    .unwrap_or(TextAlignment::Left),
                margin_left: emu_attribute(properties, "marL")
                    .or_else(|| inherited.and_then(|style| style.margin_left))
                    .unwrap_or(0.0),
                indent: emu_attribute(properties, "indent")
                    .or_else(|| inherited.and_then(|style| style.indent))
                    .unwrap_or(0.0),
                space_before: read_space_before(properties)
                    .or_else(|| inherited.and_then(|style| style.space_before))
                    .unwrap_or(0.0),
                line_spacing: read_line_spacing(properties)
                    .or_else(|| inherited.and_then(|style| style.line_spacing))
                    .unwrap_or(1.15),
            }
        })
        .collect()
}

fn read_run(
    text: String,
    properties: Option<Node<'_, '_>>,
    defaults: Option<Node<'_, '_>>,
    inherited: Option<&RunDefaults>,
    theme: &HashMap<String, PdfColor>,
) -> PptxRun {
    let font_size = properties
        .and_then(|node| node.attribute("sz"))
        .or_else(|| defaults.and_then(|node| node.attribute("sz")))
        .and_then(|value| value.parse::<f32>().ok())
        .map(|value| (value / 100.0).max(1.0))
        .or_else(|| inherited.and_then(|style| style.font_size))
        .unwrap_or(18.0);
    let color = properties
        .and_then(|node| read_fill(node, theme))
        .or_else(|| defaults.and_then(|node| read_fill(node, theme)))
        .or_else(|| inherited.and_then(|style| style.color))
        .or_else(|| theme.get("tx1").copied())
        .unwrap_or(PdfColor::BLACK);
    let bold = optional_bool_attribute(properties, "b")
        .or_else(|| optional_bool_attribute(defaults, "b"))
        .or_else(|| inherited.and_then(|style| style.bold))
        .unwrap_or(false);
    let italic = optional_bool_attribute(properties, "i")
        .or_else(|| optional_bool_attribute(defaults, "i"))
        .or_else(|| inherited.and_then(|style| style.italic))
        .unwrap_or(false);
    let underline = properties
        .and_then(|node| node.attribute("u"))
        .or_else(|| defaults.and_then(|node| node.attribute("u")))
        .map(|value| !value.eq_ignore_ascii_case("none"))
        .or_else(|| inherited.and_then(|style| style.underline))
        .unwrap_or(false)
        || properties.is_some_and(|node| {
            node.children()
                .any(|child_node| is_element(child_node, "hlinkClick"))
        });
    let font_name = select_font_name(&text, properties)
        .or_else(|| select_font_name(&text, defaults))
        .or_else(|| inherited.and_then(|style| inherited_font_name(&text, style)));
    PptxRun {
        text,
        font_size,
        color,
        bold,
        italic,
        underline,
        font_name,
    }
}

fn read_text_body(properties: Node<'_, '_>, inherited: Option<PptxTextBody>) -> PptxTextBody {
    let fallback = inherited.unwrap_or_default();
    PptxTextBody {
        left_inset: emu_attribute(Some(properties), "lIns").unwrap_or(fallback.left_inset),
        top_inset: emu_attribute(Some(properties), "tIns").unwrap_or(fallback.top_inset),
        right_inset: emu_attribute(Some(properties), "rIns").unwrap_or(fallback.right_inset),
        bottom_inset: emu_attribute(Some(properties), "bIns").unwrap_or(fallback.bottom_inset),
        anchor: match properties.attribute("anchor") {
            Some("ctr") => VerticalAnchor::Middle,
            Some("b") => VerticalAnchor::Bottom,
            Some(_) => VerticalAnchor::Top,
            None => fallback.anchor,
        },
    }
}

fn read_alignment(properties: Option<Node<'_, '_>>) -> Option<TextAlignment> {
    match properties?.attribute("algn")? {
        "ctr" => Some(TextAlignment::Center),
        "r" => Some(TextAlignment::Right),
        _ => Some(TextAlignment::Left),
    }
}

fn read_space_before(properties: Option<Node<'_, '_>>) -> Option<f32> {
    child(properties?, "spcBef")
        .and_then(|node| child(node, "spcPts"))
        .and_then(|node| node.attribute("val"))
        .and_then(|value| value.parse::<f32>().ok())
        .map(|value| (value / 100.0).max(0.0))
}

fn read_line_spacing(properties: Option<Node<'_, '_>>) -> Option<f32> {
    child(properties?, "lnSpc")
        .and_then(|node| child(node, "spcPct"))
        .and_then(|node| node.attribute("val"))
        .and_then(|value| value.parse::<f32>().ok())
        .map(|value| (value / 100_000.0).max(0.1))
}

fn read_bullet_setting(properties: Node<'_, '_>) -> Option<bool> {
    if child(properties, "buNone").is_some() {
        Some(false)
    } else if child(properties, "buChar").is_some() || child(properties, "buAutoNum").is_some() {
        Some(true)
    } else {
        None
    }
}

fn read_run_defaults(node: Node<'_, '_>, theme: &HashMap<String, PdfColor>) -> RunDefaults {
    RunDefaults {
        font_size: node
            .attribute("sz")
            .and_then(|value| value.parse::<f32>().ok())
            .map(|value| (value / 100.0).max(1.0)),
        color: read_fill(node, theme),
        bold: node.attribute("b").map(read_bool),
        italic: node.attribute("i").map(read_bool),
        underline: node
            .attribute("u")
            .map(|value| !value.eq_ignore_ascii_case("none")),
        latin_font: child(node, "latin")
            .and_then(|font| font.attribute("typeface"))
            .map(str::to_owned),
        east_asian_font: child(node, "ea")
            .and_then(|font| font.attribute("typeface"))
            .map(str::to_owned),
        complex_font: child(node, "cs")
            .and_then(|font| font.attribute("typeface"))
            .map(str::to_owned),
    }
}

fn inherited_font_name(text: &str, style: &RunDefaults) -> Option<String> {
    if contains_east_asian(text) {
        style.east_asian_font.clone()
    } else if contains_complex_script(text) {
        style.complex_font.clone()
    } else {
        style.latin_font.clone()
    }
    .or_else(|| style.latin_font.clone())
    .or_else(|| style.east_asian_font.clone())
    .or_else(|| style.complex_font.clone())
    .filter(|name| !name.starts_with('+'))
}

fn read_bounds(transform: Node<'_, '_>, coordinate_map: CoordinateMap) -> PptxRect {
    let offset = child(transform, "off");
    let extent = child(transform, "ext");
    coordinate_map.map_rect(
        integer_attribute(offset, "x").unwrap_or(0),
        integer_attribute(offset, "y").unwrap_or(0),
        integer_attribute(extent, "cx").unwrap_or(0),
        integer_attribute(extent, "cy").unwrap_or(0),
    )
}

fn create_group_map(transform: Node<'_, '_>, parent: CoordinateMap) -> CoordinateMap {
    let offset = child(transform, "off");
    let extent = child(transform, "ext");
    let child_offset = child(transform, "chOff");
    let child_extent = child(transform, "chExt");
    let group_x = integer_attribute(offset, "x").unwrap_or(0);
    let group_y = integer_attribute(offset, "y").unwrap_or(0);
    let group_width = integer_attribute(extent, "cx").unwrap_or(0);
    let group_height = integer_attribute(extent, "cy").unwrap_or(0);
    if group_width <= 0 || group_height <= 0 {
        return parent;
    }
    CoordinateMap {
        target_x: parent.map_x(group_x),
        target_y: parent.map_y(group_y),
        target_width: parent.map_x(group_x + group_width) - parent.map_x(group_x),
        target_height: parent.map_y(group_y + group_height) - parent.map_y(group_y),
        source_x: integer_attribute(child_offset, "x").unwrap_or(group_x) as f64,
        source_y: integer_attribute(child_offset, "y").unwrap_or(group_y) as f64,
        source_width: integer_attribute(child_extent, "cx").unwrap_or(group_width) as f64,
        source_height: integer_attribute(child_extent, "cy").unwrap_or(group_height) as f64,
    }
}

fn add_line_from_bounds(
    transform: Option<Node<'_, '_>>,
    bounds: PptxRect,
    outline: PptxOutline,
    elements: &mut Vec<PptxElement>,
) {
    let flip_horizontal = transform
        .and_then(|node| node.attribute("flipH"))
        .is_some_and(read_bool);
    let flip_vertical = transform
        .and_then(|node| node.attribute("flipV"))
        .is_some_and(read_bool);
    elements.push(PptxElement::Line(PptxLine {
        x1: if flip_horizontal {
            bounds.x + bounds.width
        } else {
            bounds.x
        },
        y1: if flip_vertical {
            bounds.y + bounds.height
        } else {
            bounds.y
        },
        x2: if flip_horizontal {
            bounds.x
        } else {
            bounds.x + bounds.width
        },
        y2: if flip_vertical {
            bounds.y
        } else {
            bounds.y + bounds.height
        },
        outline,
    }));
}

fn read_outline(node: Node<'_, '_>, theme: &HashMap<String, PdfColor>) -> Option<PptxOutline> {
    if child(node, "noFill").is_some() {
        return None;
    }
    let width = node
        .attribute("w")
        .and_then(|value| value.parse::<f32>().ok())
        .map(|value| value / EMUS_PER_POINT)
        .filter(|value| *value > 0.0)
        .unwrap_or(1.0);
    let dash_pattern = child(node, "prstDash")
        .and_then(|dash| dash.attribute("val"))
        .map(|value| match value {
            "dot" | "sysDot" => vec![1.0, 2.0],
            "dash" | "sysDash" => vec![3.0, 3.0],
            "lgDash" => vec![6.0, 3.0],
            "dashDot" | "sysDashDot" => vec![3.0, 2.0, 1.0, 2.0],
            _ => Vec::new(),
        })
        .unwrap_or_default();
    Some(PptxOutline {
        color: read_fill(node, theme).unwrap_or(PdfColor::BLACK),
        width,
        dash_pattern,
    })
}

fn default_outline() -> PptxOutline {
    PptxOutline {
        color: PdfColor::BLACK,
        width: 1.0,
        dash_pattern: Vec::new(),
    }
}

fn read_fill(node: Node<'_, '_>, theme: &HashMap<String, PdfColor>) -> Option<PdfColor> {
    if child(node, "noFill").is_some() {
        return None;
    }
    let fill = if is_element(node, "solidFill") {
        Some(node)
    } else {
        child(node, "solidFill")
    }?;
    let color_node = fill.children().find(Node::is_element)?;
    let color = match color_node.tag_name().name() {
        "srgbClr" => color_node.attribute("val").and_then(parse_hex_color),
        "schemeClr" => color_node
            .attribute("val")
            .and_then(|name| theme.get(name).copied()),
        "sysClr" => color_node
            .attribute("lastClr")
            .or_else(|| color_node.attribute("val"))
            .and_then(parse_hex_color),
        "prstClr" => color_node.attribute("val").and_then(preset_color),
        _ => None,
    }?;
    Some(apply_luminance(color, color_node))
}

fn read_style_fill(shape: Node<'_, '_>, theme: &HashMap<String, PdfColor>) -> Option<PdfColor> {
    child(shape, "style")
        .and_then(|style| child(style, "fillRef"))
        .and_then(|reference| reference.children().find(Node::is_element))
        .and_then(|color| read_color_node(color, theme))
}

fn read_style_outline(
    shape: Node<'_, '_>,
    theme: &HashMap<String, PdfColor>,
) -> Option<PptxOutline> {
    let color = child(shape, "style")
        .and_then(|style| child(style, "lnRef"))
        .and_then(|reference| reference.children().find(Node::is_element))
        .and_then(|color| read_color_node(color, theme))?;
    Some(PptxOutline {
        color,
        width: 1.0,
        dash_pattern: Vec::new(),
    })
}

fn read_background(document: &Document<'_>, theme: &HashMap<String, PdfColor>) -> Option<PdfColor> {
    let background = document
        .descendants()
        .find(|node| is_element(*node, "bg"))?;
    child(background, "bgPr")
        .and_then(|node| read_fill(node, theme))
        .or_else(|| {
            child(background, "bgRef")
                .and_then(|node| node.children().find(Node::is_element))
                .and_then(|node| read_color_node(node, theme))
        })
}

fn apply_color_map(
    theme: &HashMap<String, PdfColor>,
    slide: &Document<'_>,
    layout: Option<&Document<'_>>,
    master: Option<&Document<'_>>,
) -> HashMap<String, PdfColor> {
    let mut mapped = theme.clone();
    let color_map = override_color_map(slide)
        .or_else(|| layout.and_then(override_color_map))
        .or_else(|| {
            master.and_then(|document| {
                document
                    .root_element()
                    .children()
                    .find(|node| is_element(*node, "clrMap"))
            })
        });
    let Some(color_map) = color_map else {
        return mapped;
    };
    for alias in [
        "bg1", "tx1", "bg2", "tx2", "accent1", "accent2", "accent3", "accent4", "accent5",
        "accent6", "hlink", "folHlink",
    ] {
        if let Some(source) = color_map.attribute(alias) {
            if let Some(color) = theme.get(source).copied() {
                mapped.insert(alias.to_owned(), color);
            }
        }
    }
    mapped
}

fn override_color_map<'a, 'input>(document: &'a Document<'input>) -> Option<Node<'a, 'input>> {
    document
        .root_element()
        .children()
        .find(|node| is_element(*node, "clrMapOvr"))
        .and_then(|node| child(node, "overrideClrMapping"))
}

fn read_theme_colors(
    archive: &mut ZipArchive<Cursor<&[u8]>>,
    relationships: &HashMap<String, Relationship>,
) -> Result<HashMap<String, PdfColor>> {
    let mut colors = default_theme_colors();
    let theme_path = relationships
        .values()
        .find(|relationship| relationship.rel_type.ends_with("/theme"))
        .map(|relationship| resolve_part_target("ppt/presentation.xml", &relationship.target))
        .unwrap_or_else(|| "ppt/theme/theme1.xml".to_owned());
    let Some(bytes) = read_zip_bytes(archive, &theme_path)? else {
        return Ok(colors);
    };
    let Ok(text) = String::from_utf8(bytes) else {
        return Ok(colors);
    };
    let Ok(document) = Document::parse(&text) else {
        return Ok(colors);
    };
    if let Some(scheme) = document
        .descendants()
        .find(|node| is_element(*node, "clrScheme"))
    {
        for slot in scheme.children().filter(Node::is_element) {
            if let Some(color_node) = slot.children().find(Node::is_element) {
                if let Some(color) = read_color_node(color_node, &colors) {
                    colors.insert(slot.tag_name().name().to_owned(), color);
                }
            }
        }
    }
    add_theme_aliases(&mut colors);
    Ok(colors)
}

fn read_color_node(node: Node<'_, '_>, theme: &HashMap<String, PdfColor>) -> Option<PdfColor> {
    let color = match node.tag_name().name() {
        "srgbClr" => node.attribute("val").and_then(parse_hex_color),
        "schemeClr" => node
            .attribute("val")
            .and_then(|name| theme.get(name).copied()),
        "sysClr" => node
            .attribute("lastClr")
            .or_else(|| node.attribute("val"))
            .and_then(parse_hex_color),
        "prstClr" => node.attribute("val").and_then(preset_color),
        _ => None,
    }?;
    Some(apply_luminance(color, node))
}

fn default_theme_colors() -> HashMap<String, PdfColor> {
    let mut colors = HashMap::from([
        ("lt1".to_owned(), PdfColor::WHITE),
        ("dk1".to_owned(), PdfColor::BLACK),
        ("lt2".to_owned(), parse_hex_color("EEECE1").unwrap()),
        ("dk2".to_owned(), parse_hex_color("1F497D").unwrap()),
        ("accent1".to_owned(), parse_hex_color("4F81BD").unwrap()),
        ("accent2".to_owned(), parse_hex_color("C0504D").unwrap()),
        ("accent3".to_owned(), parse_hex_color("9BBB59").unwrap()),
        ("accent4".to_owned(), parse_hex_color("8064A2").unwrap()),
        ("accent5".to_owned(), parse_hex_color("4BACC6").unwrap()),
        ("accent6".to_owned(), parse_hex_color("F79646").unwrap()),
        ("hlink".to_owned(), parse_hex_color("0000FF").unwrap()),
        ("folHlink".to_owned(), parse_hex_color("800080").unwrap()),
    ]);
    add_theme_aliases(&mut colors);
    colors
}

fn add_theme_aliases(colors: &mut HashMap<String, PdfColor>) {
    for (alias, source) in [
        ("bg1", "lt1"),
        ("tx1", "dk1"),
        ("bg2", "lt2"),
        ("tx2", "dk2"),
    ] {
        if let Some(color) = colors.get(source).copied() {
            colors.insert(alias.to_owned(), color);
        }
    }
}

fn render_pptx(presentation: &PptxDocument, options: &ConversionOptions) -> PdfDocument {
    let mut document = PdfDocument::new();
    if presentation.slides.is_empty() {
        let size = options.page_size.unwrap_or(crate::PageSize {
            width: presentation.width,
            height: presentation.height,
        });
        document.add_page(size.width, size.height);
        return document;
    }
    for slide in &presentation.slides {
        render_slide(&mut document, slide, options);
    }
    document
}

fn render_slide(document: &mut PdfDocument, slide: &PptxSlide, options: &ConversionOptions) {
    let page_size = options.page_size.unwrap_or(crate::PageSize {
        width: slide.width,
        height: slide.height,
    });
    let scale_x = page_size.width / slide.width;
    let scale_y = page_size.height / slide.height;
    let image_ids: HashMap<usize, usize> = slide
        .elements
        .iter()
        .enumerate()
        .filter_map(|(index, element)| {
            let PptxElement::Picture(picture) = element else {
                return None;
            };
            let image_id = match &picture.data {
                PptxImageData::Jpeg {
                    data,
                    width,
                    height,
                } => document.add_jpeg_image(data.clone(), *width, *height),
                PptxImageData::Rgba {
                    data,
                    width,
                    height,
                } => document.add_rgba_image(data.clone(), *width, *height),
                PptxImageData::Svg { .. } => return None,
            };
            Some((index, image_id))
        })
        .collect();
    let page = document.add_page(page_size.width, page_size.height);
    if let Some(background) = slide.background {
        page.add_rect(0.0, 0.0, page_size.width, page_size.height, background);
    }
    for (index, element) in slide.elements.iter().enumerate() {
        match element {
            PptxElement::Picture(picture) => {
                let bounds = scale_rect(picture.bounds, scale_x, scale_y);
                if let PptxImageData::Svg { data, crop } = &picture.data {
                    render_svg(
                        page,
                        data,
                        *crop,
                        bounds,
                        page_size.height,
                        slide.background,
                    );
                } else if let Some(image_id) = image_ids.get(&index) {
                    page.add_image(
                        *image_id,
                        bounds.x,
                        page_size.height - bounds.y - bounds.height,
                        bounds.width,
                        bounds.height,
                    );
                }
            }
            PptxElement::Line(line) => {
                page.add_line_with_dash_pattern(
                    (line.x1 * scale_x, page_size.height - line.y1 * scale_y),
                    (line.x2 * scale_x, page_size.height - line.y2 * scale_y),
                    line.outline.color,
                    line.outline.width * scale_x.min(scale_y),
                    &line.outline.dash_pattern,
                );
            }
            PptxElement::Shape(shape) => {
                render_shape(page, shape, page_size.height, scale_x, scale_y);
            }
        }
    }
}

fn render_svg(
    page: &mut crate::pdf::PdfPage,
    data: &[u8],
    crop: PptxCrop,
    bounds: PptxRect,
    page_height: f32,
    background: Option<PdfColor>,
) {
    let Ok(text) = std::str::from_utf8(data) else {
        return;
    };
    let Ok(document) = Document::parse(text) else {
        return;
    };
    let root = document.root_element();
    let Some(mut view_box) = read_svg_view_box(root) else {
        return;
    };
    view_box.x += view_box.width * crop.left;
    view_box.y += view_box.height * crop.top;
    view_box.width *= (1.0 - crop.left - crop.right).max(0.01);
    view_box.height *= (1.0 - crop.top - crop.bottom).max(0.01);
    if view_box.width <= 0.0 || view_box.height <= 0.0 {
        return;
    }
    let scale_x = bounds.width / view_box.width;
    let scale_y = bounds.height / view_box.height;
    let pdf_bottom = page_height - bounds.y - bounds.height;
    for path in root.descendants().filter(|node| is_element(*node, "path")) {
        let Some(path_data) = path.attribute("d") else {
            continue;
        };
        let Some(fill) = svg_fill(path, background.unwrap_or(PdfColor::WHITE)) else {
            continue;
        };
        let commands = parse_svg_path(path_data, |x, y| {
            (
                bounds.x + (x - view_box.x) * scale_x,
                pdf_bottom + (view_box.height - (y - view_box.y)) * scale_y,
            )
        });
        page.add_path(commands, fill);
    }
}

fn read_svg_view_box(root: Node<'_, '_>) -> Option<PptxRect> {
    if let Some(value) = root.attribute("viewBox") {
        let values = parse_number_list(value);
        if values.len() >= 4 {
            return Some(PptxRect {
                x: values[0],
                y: values[1],
                width: values[2],
                height: values[3],
            });
        }
    }
    Some(PptxRect {
        x: 0.0,
        y: 0.0,
        width: parse_svg_length(root.attribute("width")?)?,
        height: parse_svg_length(root.attribute("height")?)?,
    })
}

fn parse_svg_length(value: &str) -> Option<f32> {
    let end = value
        .char_indices()
        .find(|(_, character)| !character.is_ascii_digit() && !matches!(character, '.' | '-' | '+'))
        .map(|(index, _)| index)
        .unwrap_or(value.len());
    value[..end].parse().ok()
}

fn svg_fill(path: Node<'_, '_>, background: PdfColor) -> Option<PdfColor> {
    let value = path.attribute("fill")?.trim();
    if value.eq_ignore_ascii_case("none") {
        return None;
    }
    let color = if value.starts_with('#') {
        let mut hex = value.trim_start_matches('#').to_owned();
        if hex.len() == 3 {
            hex = hex
                .chars()
                .flat_map(|character| [character, character])
                .collect();
        }
        parse_hex_color(&hex)
    } else {
        preset_color(value)
    }?;
    let opacity = path
        .attribute("fill-opacity")
        .and_then(|value| value.parse::<f32>().ok())
        .unwrap_or(1.0)
        .clamp(0.0, 1.0);
    Some(PdfColor::new(
        color.r * opacity + background.r * (1.0 - opacity),
        color.g * opacity + background.g * (1.0 - opacity),
        color.b * opacity + background.b * (1.0 - opacity),
    ))
}

fn parse_svg_path(path_data: &str, map: impl Fn(f32, f32) -> (f32, f32)) -> Vec<PdfPathCommand> {
    let tokens = tokenize_svg_path(path_data);
    let mut commands = Vec::new();
    let mut index = 0;
    let mut command = ' ';
    let mut current = (0.0, 0.0);
    let mut start = (0.0, 0.0);
    let mut last_control = None;
    while index < tokens.len() {
        let iteration_start = index;
        if let Some(value) = command_token(&tokens[index]) {
            command = value;
            index += 1;
        }
        if command == ' ' {
            break;
        }
        let relative = command.is_ascii_lowercase();
        match command.to_ascii_uppercase() {
            'M' => {
                let mut first = true;
                while let Some((x, y)) = read_pair(&tokens, &mut index) {
                    current = if relative {
                        (current.0 + x, current.1 + y)
                    } else {
                        (x, y)
                    };
                    let point = map(current.0, current.1);
                    if first {
                        commands.push(PdfPathCommand::MoveTo(point.0, point.1));
                        start = current;
                        first = false;
                    } else {
                        commands.push(PdfPathCommand::LineTo(point.0, point.1));
                    }
                    last_control = None;
                }
            }
            'L' => {
                while let Some((x, y)) = read_pair(&tokens, &mut index) {
                    current = if relative {
                        (current.0 + x, current.1 + y)
                    } else {
                        (x, y)
                    };
                    let point = map(current.0, current.1);
                    commands.push(PdfPathCommand::LineTo(point.0, point.1));
                    last_control = None;
                }
            }
            'H' | 'V' => {
                while let Some(value) = read_number(&tokens, &mut index) {
                    if command.eq_ignore_ascii_case(&'H') {
                        current.0 = if relative { current.0 + value } else { value };
                    } else {
                        current.1 = if relative { current.1 + value } else { value };
                    }
                    let point = map(current.0, current.1);
                    commands.push(PdfPathCommand::LineTo(point.0, point.1));
                    last_control = None;
                }
            }
            'C' => {
                while let (Some((mut x1, mut y1)), Some((mut x2, mut y2)), Some((mut x, mut y))) = (
                    read_pair(&tokens, &mut index),
                    read_pair(&tokens, &mut index),
                    read_pair(&tokens, &mut index),
                ) {
                    if relative {
                        x1 += current.0;
                        y1 += current.1;
                        x2 += current.0;
                        y2 += current.1;
                        x += current.0;
                        y += current.1;
                    }
                    let first = map(x1, y1);
                    let second = map(x2, y2);
                    let end = map(x, y);
                    commands.push(PdfPathCommand::CurveTo(
                        first.0, first.1, second.0, second.1, end.0, end.1,
                    ));
                    current = (x, y);
                    last_control = Some((x2, y2));
                }
            }
            'S' => {
                while let (Some((mut x2, mut y2)), Some((mut x, mut y))) = (
                    read_pair(&tokens, &mut index),
                    read_pair(&tokens, &mut index),
                ) {
                    let first_control = last_control
                        .map(|point| (current.0 * 2.0 - point.0, current.1 * 2.0 - point.1))
                        .unwrap_or(current);
                    if relative {
                        x2 += current.0;
                        y2 += current.1;
                        x += current.0;
                        y += current.1;
                    }
                    let first = map(first_control.0, first_control.1);
                    let second = map(x2, y2);
                    let end = map(x, y);
                    commands.push(PdfPathCommand::CurveTo(
                        first.0, first.1, second.0, second.1, end.0, end.1,
                    ));
                    current = (x, y);
                    last_control = Some((x2, y2));
                }
            }
            'Z' => {
                commands.push(PdfPathCommand::Close);
                current = start;
                last_control = None;
            }
            _ => break,
        }
        if index == iteration_start {
            break;
        }
    }
    commands
}

fn tokenize_svg_path(value: &str) -> Vec<String> {
    let mut tokens = Vec::new();
    let mut number = String::new();
    for character in value.chars() {
        if matches!(character, 'e' | 'E') && !number.is_empty() && !number.ends_with(['e', 'E']) {
            number.push(character);
        } else if character.is_ascii_alphabetic() {
            if !number.is_empty() {
                tokens.push(std::mem::take(&mut number));
            }
            tokens.push(character.to_string());
        } else if character == ',' || character.is_whitespace() {
            if !number.is_empty() {
                tokens.push(std::mem::take(&mut number));
            }
        } else if matches!(character, '-' | '+')
            && !number.is_empty()
            && !number.ends_with('e')
            && !number.ends_with('E')
        {
            tokens.push(std::mem::take(&mut number));
            number.push(character);
        } else {
            number.push(character);
        }
    }
    if !number.is_empty() {
        tokens.push(number);
    }
    tokens
}

fn command_token(value: &str) -> Option<char> {
    let mut characters = value.chars();
    let character = characters.next()?;
    (characters.next().is_none() && character.is_ascii_alphabetic()).then_some(character)
}

fn read_number(tokens: &[String], index: &mut usize) -> Option<f32> {
    if *index >= tokens.len() || command_token(&tokens[*index]).is_some() {
        return None;
    }
    let value = tokens[*index].parse().ok()?;
    *index += 1;
    Some(value)
}

fn read_pair(tokens: &[String], index: &mut usize) -> Option<(f32, f32)> {
    let original = *index;
    let first = read_number(tokens, index)?;
    let Some(second) = read_number(tokens, index) else {
        *index = original;
        return None;
    };
    Some((first, second))
}

fn parse_number_list(value: &str) -> Vec<f32> {
    value
        .split(|character: char| character == ',' || character.is_whitespace())
        .filter(|part| !part.is_empty())
        .filter_map(|part| part.parse().ok())
        .collect()
}

fn render_shape(
    page: &mut crate::pdf::PdfPage,
    shape: &PptxShape,
    page_height: f32,
    scale_x: f32,
    scale_y: f32,
) {
    let bounds = scale_rect(shape.bounds, scale_x, scale_y);
    let bottom = page_height - bounds.y - bounds.height;
    let ellipse = matches!(shape.shape_type.as_str(), "ellipse" | "arc");
    if let Some(fill) = shape.fill {
        if ellipse {
            page.add_ellipse(bounds.x, bottom, bounds.width, bounds.height, fill);
        } else {
            page.add_rect(bounds.x, bottom, bounds.width, bounds.height, fill);
        }
    }
    if !ellipse {
        if let Some(outline) = &shape.outline {
            let left = bounds.x;
            let right = bounds.x + bounds.width;
            let top = page_height - bounds.y;
            let bottom = top - bounds.height;
            for (start, end) in [
                ((left, top), (right, top)),
                ((right, top), (right, bottom)),
                ((right, bottom), (left, bottom)),
                ((left, bottom), (left, top)),
            ] {
                page.add_line_with_dash_pattern(
                    start,
                    end,
                    outline.color,
                    outline.width * scale_x.min(scale_y),
                    &outline.dash_pattern,
                );
            }
        }
    }
    render_text(page, shape, bounds, page_height, scale_x.min(scale_y));
}

fn render_text(
    page: &mut crate::pdf::PdfPage,
    shape: &PptxShape,
    bounds: PptxRect,
    page_height: f32,
    font_scale: f32,
) {
    if shape.paragraphs.is_empty() {
        return;
    }
    let text_body = shape.text_body;
    let left = bounds.x + text_body.left_inset * font_scale;
    let top = bounds.y + text_body.top_inset * font_scale;
    let width =
        (bounds.width - (text_body.left_inset + text_body.right_inset) * font_scale).max(1.0);
    let available_height =
        (bounds.height - (text_body.top_inset + text_body.bottom_inset) * font_scale).max(1.0);
    let total_height: f32 = shape
        .paragraphs
        .iter()
        .map(|paragraph| paragraph_height(paragraph, width, font_scale))
        .sum();
    let mut current_top = match text_body.anchor {
        VerticalAnchor::Top => top,
        VerticalAnchor::Middle => top + ((available_height - total_height) / 2.0).max(0.0),
        VerticalAnchor::Bottom => top + (available_height - total_height).max(0.0),
    };
    page.push_clip(
        bounds.x,
        page_height - bounds.y - bounds.height,
        bounds.width,
        bounds.height,
    );
    for paragraph in &shape.paragraphs {
        current_top += paragraph.space_before * font_scale;
        let paragraph_left =
            left + (paragraph.margin_left + paragraph.indent.min(0.0)).max(0.0) * font_scale;
        let paragraph_width = (width
            - (paragraph.margin_left + paragraph.indent.min(0.0)).max(0.0) * font_scale)
            .max(1.0);
        let lines = wrap_paragraph(paragraph, paragraph_width, font_scale);
        for line in lines {
            let line_width: f32 = line.iter().map(|segment| segment.width).sum();
            let line_height = line
                .iter()
                .map(|segment| segment.run.font_size * font_scale)
                .fold(12.0, f32::max)
                * paragraph.line_spacing;
            let mut x = match paragraph.alignment {
                TextAlignment::Left => paragraph_left,
                TextAlignment::Center => {
                    paragraph_left + ((paragraph_width - line_width) / 2.0).max(0.0)
                }
                TextAlignment::Right => paragraph_left + (paragraph_width - line_width).max(0.0),
            };
            for segment in line {
                let font_size = segment.run.font_size * font_scale;
                let baseline = page_height - current_top - font_size;
                page.add_styled_text_with_font(
                    &segment.text,
                    x,
                    baseline,
                    font_size,
                    segment.run.color,
                    segment.run.bold,
                    segment.run.italic,
                    segment.run.font_name.as_deref(),
                );
                if segment.run.underline && !segment.text.trim().is_empty() {
                    page.add_line(
                        x,
                        baseline - font_size * 0.08,
                        x + segment.width,
                        baseline - font_size * 0.08,
                        segment.run.color,
                        (font_size * 0.05).max(0.5),
                    );
                }
                x += segment.width;
            }
            current_top += line_height;
        }
        if current_top > bounds.y + bounds.height {
            break;
        }
    }
    page.pop_clip();
}

#[derive(Debug)]
struct TextSegment<'a> {
    text: String,
    run: &'a PptxRun,
    width: f32,
}

fn wrap_paragraph<'a>(
    paragraph: &'a PptxParagraph,
    max_width: f32,
    font_scale: f32,
) -> Vec<Vec<TextSegment<'a>>> {
    let mut lines = Vec::new();
    let mut line = Vec::new();
    let mut line_width = 0.0;
    for run in &paragraph.runs {
        for token in split_text_tokens(&run.text) {
            if token == "\n" {
                lines.push(std::mem::take(&mut line));
                line_width = 0.0;
                continue;
            }
            let font_size = run.font_size * font_scale;
            let width = styled_text_width_with_font(
                &token,
                font_size,
                run.bold,
                run.italic,
                run.font_name.as_deref(),
            );
            if !line.is_empty() && line_width + width > max_width {
                lines.push(std::mem::take(&mut line));
                line_width = 0.0;
            }
            line.push(TextSegment {
                text: token,
                run,
                width,
            });
            line_width += width;
        }
    }
    if !line.is_empty() || lines.is_empty() {
        lines.push(line);
    }
    lines
}

fn paragraph_height(paragraph: &PptxParagraph, width: f32, font_scale: f32) -> f32 {
    let lines = wrap_paragraph(paragraph, width, font_scale);
    paragraph.space_before * font_scale
        + lines
            .iter()
            .map(|line| {
                line.iter()
                    .map(|segment| segment.run.font_size * font_scale)
                    .fold(12.0, f32::max)
                    * paragraph.line_spacing
            })
            .sum::<f32>()
}

fn split_text_tokens(text: &str) -> Vec<String> {
    let mut tokens = Vec::new();
    let mut current = String::new();
    let mut whitespace = false;
    for character in text.replace('\t', "    ").chars() {
        if character == '\r' {
            continue;
        }
        if character == '\n' {
            if !current.is_empty() {
                tokens.push(std::mem::take(&mut current));
            }
            tokens.push("\n".to_owned());
            continue;
        }
        let is_whitespace = character.is_whitespace();
        if !current.is_empty() && is_whitespace != whitespace {
            tokens.push(std::mem::take(&mut current));
        }
        current.push(character);
        whitespace = is_whitespace;
    }
    if !current.is_empty() {
        tokens.push(current);
    }
    tokens
}

fn scale_rect(rect: PptxRect, scale_x: f32, scale_y: f32) -> PptxRect {
    PptxRect {
        x: rect.x * scale_x,
        y: rect.y * scale_y,
        width: rect.width * scale_x,
        height: rect.height * scale_y,
    }
}

fn read_relationships(
    archive: &mut ZipArchive<Cursor<&[u8]>>,
    part_path: &str,
) -> Result<HashMap<String, Relationship>> {
    let relationship_path = relationship_part_path(part_path);
    let Some(bytes) = read_zip_bytes(archive, &relationship_path)? else {
        return Ok(HashMap::new());
    };
    let text = String::from_utf8(bytes)
        .map_err(|_| MiniPdfError::InvalidInput(format!("{relationship_path} is not UTF-8")))?;
    let document = Document::parse(&text)?;
    Ok(document
        .descendants()
        .filter(|node| is_element(*node, "Relationship"))
        .filter_map(|node| {
            Some((
                node.attribute("Id")?.to_owned(),
                Relationship {
                    target: node.attribute("Target")?.to_owned(),
                    rel_type: node.attribute("Type").unwrap_or_default().to_owned(),
                    external: node
                        .attribute("TargetMode")
                        .is_some_and(|value| value.eq_ignore_ascii_case("External")),
                    relationship_path: relationship_path.clone(),
                },
            ))
        })
        .collect())
}

type RelatedXml = (String, String, HashMap<String, Relationship>);

fn read_related_xml(
    archive: &mut ZipArchive<Cursor<&[u8]>>,
    source_path: &str,
    relationships: &HashMap<String, Relationship>,
    type_suffix: &str,
) -> Result<Option<RelatedXml>> {
    let Some(relationship) = relationships.values().find(|relationship| {
        relationship.rel_type.ends_with(type_suffix) && !relationship.external
    }) else {
        return Ok(None);
    };
    let path = resolve_part_target(source_path, &relationship.target);
    let Some(bytes) = read_zip_bytes(archive, &path)? else {
        return Ok(None);
    };
    let xml = String::from_utf8(bytes)
        .map_err(|_| MiniPdfError::InvalidInput(format!("{path} is not UTF-8")))?;
    let part_relationships = read_relationships(archive, &path)?;
    Ok(Some((path, xml, part_relationships)))
}

fn read_zip_bytes(archive: &mut ZipArchive<Cursor<&[u8]>>, path: &str) -> Result<Option<Vec<u8>>> {
    let Ok(mut entry) = archive.by_name(path) else {
        return Ok(None);
    };
    let mut bytes = Vec::with_capacity(entry.size() as usize);
    entry.read_to_end(&mut bytes)?;
    Ok(Some(bytes))
}

fn relationship_part_path(part_path: &str) -> String {
    match part_path.rsplit_once('/') {
        Some((directory, file_name)) => format!("{directory}/_rels/{file_name}.rels"),
        None => format!("_rels/{part_path}.rels"),
    }
}

fn relationship_source_path(relationship: &Relationship) -> String {
    let Some((directory, file_name)) = relationship.relationship_path.split_once("/_rels/") else {
        return relationship.relationship_path.clone();
    };
    format!("{directory}/{}", file_name.trim_end_matches(".rels"))
}

fn resolve_part_target(source_path: &str, target: &str) -> String {
    if target.starts_with('/') {
        return target.trim_start_matches('/').to_owned();
    }
    let directory = source_path
        .rsplit_once('/')
        .map(|(path, _)| path)
        .unwrap_or_default();
    let mut normalized = Vec::new();
    let combined = format!("{directory}/{target}");
    for segment in combined.split('/') {
        match segment {
            "" | "." => {}
            ".." => {
                normalized.pop();
            }
            value => normalized.push(value),
        }
    }
    normalized.join("/")
}

fn child<'a, 'input>(node: Node<'a, 'input>, local_name: &str) -> Option<Node<'a, 'input>> {
    node.children()
        .find(|child_node| is_element(*child_node, local_name))
}

fn is_element(node: Node<'_, '_>, local_name: &str) -> bool {
    node.is_element() && node.tag_name().name() == local_name
}

fn attribute_by_local_name<'a, 'input>(node: Node<'a, 'input>, name: &str) -> Option<&'a str> {
    node.attributes()
        .find(|attribute| attribute.name() == name)
        .map(|attribute| attribute.value())
}

fn integer_attribute(node: Option<Node<'_, '_>>, name: &str) -> Option<i64> {
    node?.attribute(name)?.parse().ok()
}

fn emu_attribute(node: Option<Node<'_, '_>>, name: &str) -> Option<f32> {
    integer_attribute(node, name).map(|value| value as f32 / EMUS_PER_POINT)
}

fn crop_attribute(node: Node<'_, '_>, name: &str) -> f32 {
    node.attribute(name)
        .and_then(|value| value.parse::<f32>().ok())
        .map(|value| (value / 100_000.0).clamp(0.0, 0.95))
        .unwrap_or(0.0)
}

fn optional_bool_attribute(node: Option<Node<'_, '_>>, name: &str) -> Option<bool> {
    node?.attribute(name).map(read_bool)
}

fn read_bool(value: &str) -> bool {
    value == "1" || value.eq_ignore_ascii_case("true")
}

fn parse_hex_color(value: &str) -> Option<PdfColor> {
    let value = value.trim_start_matches('#');
    if value.len() != 6 {
        return None;
    }
    Some(PdfColor::new(
        u8::from_str_radix(&value[0..2], 16).ok()? as f32 / 255.0,
        u8::from_str_radix(&value[2..4], 16).ok()? as f32 / 255.0,
        u8::from_str_radix(&value[4..6], 16).ok()? as f32 / 255.0,
    ))
}

fn preset_color(value: &str) -> Option<PdfColor> {
    match value.to_ascii_lowercase().as_str() {
        "black" => Some(PdfColor::BLACK),
        "white" => Some(PdfColor::WHITE),
        "red" => Some(PdfColor::new(1.0, 0.0, 0.0)),
        "green" => Some(PdfColor::new(0.0, 0.5, 0.0)),
        "blue" => Some(PdfColor::new(0.0, 0.0, 1.0)),
        "yellow" => Some(PdfColor::new(1.0, 1.0, 0.0)),
        _ => None,
    }
}

fn apply_luminance(color: PdfColor, node: Node<'_, '_>) -> PdfColor {
    let modifier = child(node, "lumMod")
        .and_then(|node| node.attribute("val"))
        .and_then(|value| value.parse::<f32>().ok())
        .map(|value| value / 100_000.0)
        .unwrap_or(1.0);
    let offset = child(node, "lumOff")
        .and_then(|node| node.attribute("val"))
        .and_then(|value| value.parse::<f32>().ok())
        .map(|value| value / 100_000.0)
        .unwrap_or(0.0);
    if (modifier - 1.0).abs() < f32::EPSILON && offset.abs() < f32::EPSILON {
        return color;
    }
    let (hue, saturation, luminance) = rgb_to_hsl(color);
    hsl_to_rgb(
        hue,
        saturation,
        (luminance * modifier + offset).clamp(0.0, 1.0),
    )
}

fn rgb_to_hsl(color: PdfColor) -> (f32, f32, f32) {
    let max = color.r.max(color.g).max(color.b);
    let min = color.r.min(color.g).min(color.b);
    let luminance = (max + min) / 2.0;
    if (max - min).abs() < 0.000_01 {
        return (0.0, 0.0, luminance);
    }
    let delta = max - min;
    let saturation = if luminance > 0.5 {
        delta / (2.0 - max - min)
    } else {
        delta / (max + min)
    };
    let hue = if (max - color.r).abs() < 0.000_01 {
        (color.g - color.b) / delta + if color.g < color.b { 6.0 } else { 0.0 }
    } else if (max - color.g).abs() < 0.000_01 {
        (color.b - color.r) / delta + 2.0
    } else {
        (color.r - color.g) / delta + 4.0
    };
    (hue / 6.0, saturation, luminance)
}

fn hsl_to_rgb(hue: f32, saturation: f32, luminance: f32) -> PdfColor {
    if saturation <= 0.000_01 {
        return PdfColor::new(luminance, luminance, luminance);
    }
    let upper = if luminance < 0.5 {
        luminance * (1.0 + saturation)
    } else {
        luminance + saturation - luminance * saturation
    };
    let lower = 2.0 * luminance - upper;
    PdfColor::new(
        hue_to_rgb(lower, upper, hue + 1.0 / 3.0),
        hue_to_rgb(lower, upper, hue),
        hue_to_rgb(lower, upper, hue - 1.0 / 3.0),
    )
}

fn hue_to_rgb(lower: f32, upper: f32, mut hue: f32) -> f32 {
    if hue < 0.0 {
        hue += 1.0;
    }
    if hue > 1.0 {
        hue -= 1.0;
    }
    if hue < 1.0 / 6.0 {
        lower + (upper - lower) * 6.0 * hue
    } else if hue < 0.5 {
        upper
    } else if hue < 2.0 / 3.0 {
        lower + (upper - lower) * (2.0 / 3.0 - hue) * 6.0
    } else {
        lower
    }
}

fn select_font_name(text: &str, properties: Option<Node<'_, '_>>) -> Option<String> {
    let properties = properties?;
    let preferred = if contains_east_asian(text) {
        child(properties, "ea")
    } else if contains_complex_script(text) {
        child(properties, "cs")
    } else {
        child(properties, "latin")
    };
    preferred
        .or_else(|| child(properties, "latin"))
        .or_else(|| child(properties, "ea"))
        .or_else(|| child(properties, "cs"))
        .and_then(|node| node.attribute("typeface"))
        .filter(|name| !name.starts_with('+'))
        .map(str::to_owned)
}

fn contains_east_asian(text: &str) -> bool {
    text.chars().any(|character| {
        matches!(character as u32, 0x3000..=0x30ff | 0x3400..=0x4dbf | 0x4e00..=0x9fff | 0xac00..=0xd7af | 0xf900..=0xfaff | 0xff00..=0xffef)
    })
}

fn contains_complex_script(text: &str) -> bool {
    text.chars().any(
        |character| matches!(character as u32, 0x0590..=0x08ff | 0x0900..=0x0d7f | 0x0e00..=0x0eff),
    )
}

fn natural_slide_number(path: &str) -> u32 {
    path.rsplit_once("slide")
        .and_then(|(_, suffix)| suffix.trim_end_matches(".xml").parse().ok())
        .unwrap_or(u32::MAX)
}

fn jpeg_dimensions(data: &[u8]) -> Option<(u16, u16)> {
    if data.len() < 4 || data[..2] != [0xff, 0xd8] {
        return None;
    }
    let mut offset = 2;
    while offset + 4 <= data.len() {
        if data[offset] != 0xff {
            offset += 1;
            continue;
        }
        let marker = data[offset + 1];
        offset += 2;
        if marker == 0xd9 || marker == 0xda {
            break;
        }
        if offset + 2 > data.len() {
            break;
        }
        let length = u16::from_be_bytes([data[offset], data[offset + 1]]) as usize;
        if length < 2 || offset + length > data.len() {
            break;
        }
        if matches!(marker, 0xc0..=0xc3 | 0xc5..=0xc7 | 0xc9..=0xcb | 0xcd..=0xcf) && length >= 7 {
            return Some((
                u16::from_be_bytes([data[offset + 5], data[offset + 6]]),
                u16::from_be_bytes([data[offset + 3], data[offset + 4]]),
            ));
        }
        offset += length;
    }
    None
}

#[cfg(test)]
mod tests {
    use std::io::{Cursor, Write};

    use zip::write::SimpleFileOptions;

    use super::{convert_pptx_bytes, read_pptx};
    use crate::{convert_bytes_to_pdf, detect_office_format, ConversionOptions, OfficeFormat};

    #[test]
    fn converts_text_shapes_and_custom_slide_size() {
        let pptx = create_pptx();
        assert_eq!(detect_office_format(&pptx).unwrap(), OfficeFormat::Pptx);
        let presentation = read_pptx(&pptx).expect("PPTX parses");
        assert_eq!(presentation.slides.len(), 1);
        assert!((presentation.width - 720.0).abs() < 0.01);
        assert!((presentation.height - 540.0).abs() < 0.01);
        assert_eq!(presentation.slides[0].elements.len(), 2);
        let super::PptxElement::Shape(shape) = &presentation.slides[0].elements[0] else {
            panic!("first element is a shape");
        };
        assert_eq!(shape.paragraphs[0].runs[0].text, "Hello Rust PPTX");

        let pdf = convert_pptx_bytes(&pptx, &ConversionOptions::default()).expect("PPTX converts");
        let text = String::from_utf8_lossy(&pdf);
        assert!(pdf.starts_with(b"%PDF-1.4"));
        assert!(pdf.ends_with(b"%%EOF\n"));
        assert!(pdf.len() > 500);
        assert!(text.contains("/MediaBox [0 0 720.00 540.00]"));

        let detected_pdf = convert_bytes_to_pdf(&pptx).expect("detected PPTX converts");
        assert!(detected_pdf.starts_with(b"%PDF-1.4"));
    }

    #[test]
    fn renders_office_svg_and_smartart_fallback() {
        let pptx = create_svg_and_smartart_pptx();
        let presentation = read_pptx(&pptx).expect("PPTX parses");
        assert!(presentation.slides[0].elements.iter().any(|element| {
            matches!(
                element,
                super::PptxElement::Picture(super::PptxPicture {
                    data: super::PptxImageData::Svg { .. },
                    ..
                })
            )
        }));
        assert!(presentation.slides[0].elements.iter().any(|element| {
            let super::PptxElement::Shape(shape) = element else {
                return false;
            };
            shape.paragraphs.iter().any(|paragraph| {
                paragraph
                    .runs
                    .iter()
                    .any(|run| run.text.contains("SmartArt Text"))
            })
        }));

        let pdf = convert_pptx_bytes(&pptx, &ConversionOptions::default()).expect("PPTX converts");
        assert!(pdf.starts_with(b"%PDF-1.4"));
        assert!(pdf.len() > 700);
    }

    #[test]
    fn inherits_placeholder_bounds_and_text_layout() {
        let pptx = create_placeholder_pptx();
        let presentation = read_pptx(&pptx).expect("PPTX parses");
        let shapes: Vec<_> = presentation.slides[0]
            .elements
            .iter()
            .filter_map(|element| {
                let super::PptxElement::Shape(shape) = element else {
                    return None;
                };
                Some(shape)
            })
            .collect();
        assert_eq!(shapes.len(), 2);
        assert!((shapes[0].bounds.x - 72.0).abs() < 0.01);
        assert!((shapes[0].bounds.width - 288.0).abs() < 0.01);
        assert!(matches!(
            shapes[0].paragraphs[0].alignment,
            super::TextAlignment::Center
        ));
        assert!((shapes[0].paragraphs[0].runs[0].font_size - 24.0).abs() < 0.01);
        assert!(shapes[1].paragraphs[1].runs[0].text.starts_with('\u{2022}'));

        let pdf = convert_pptx_bytes(&pptx, &ConversionOptions::default()).expect("PPTX converts");
        assert!(pdf.starts_with(b"%PDF-1.4"));
    }

    #[test]
    fn applies_master_color_map_aliases() {
        let theme = super::default_theme_colors();
        let slide = roxmltree::Document::parse(
            r#"<p:sld xmlns:p="urn:p"><p:clrMapOvr><p:masterClrMapping/></p:clrMapOvr></p:sld>"#,
        )
        .unwrap();
        let master = roxmltree::Document::parse(
            r#"<p:sldMaster xmlns:p="urn:p"><p:clrMap bg1="dk1" tx1="lt1"/></p:sldMaster>"#,
        )
        .unwrap();
        let mapped = super::apply_color_map(&theme, &slide, None, Some(&master));
        assert_eq!(mapped["bg1"], super::PdfColor::BLACK);
        assert_eq!(mapped["tx1"], super::PdfColor::WHITE);
    }

    #[test]
    fn preserves_presentation_slide_order() {
        let pptx = create_reordered_pptx();
        let presentation = read_pptx(&pptx).expect("PPTX parses");
        let slide_text = |index: usize| {
            presentation.slides[index]
                .elements
                .iter()
                .find_map(|element| {
                    let super::PptxElement::Shape(shape) = element else {
                        return None;
                    };
                    shape
                        .paragraphs
                        .first()?
                        .runs
                        .first()
                        .map(|run| run.text.as_str())
                })
                .unwrap()
        };
        assert_eq!(slide_text(0), "Second");
        assert_eq!(slide_text(1), "First");
    }

    #[test]
    fn falls_back_when_slide_relationship_targets_are_missing() {
        let pptx = create_broken_relationship_pptx();
        let presentation = read_pptx(&pptx).expect("PPTX parses");
        assert_eq!(presentation.slides.len(), 1);
        let super::PptxElement::Shape(shape) = &presentation.slides[0].elements[0] else {
            panic!("fallback slide element is a shape");
        };
        assert_eq!(shape.paragraphs[0].runs[0].text, "Recovered");
    }

    #[test]
    fn skips_pictures_with_non_positive_bounds() {
        let pptx = create_invalid_picture_bounds_pptx();
        let presentation = read_pptx(&pptx).expect("PPTX parses");
        assert!(presentation.slides[0]
            .elements
            .iter()
            .all(|element| !matches!(element, super::PptxElement::Picture(_))));
    }

    #[test]
    fn parses_svg_path_scientific_notation_without_stalling() {
        let commands = super::parse_svg_path("M1e-3 2E+2 L3.5e1 4e0 Z", |x, y| (x, y));
        assert_eq!(commands.len(), 3);
        assert!(
            matches!(commands[0], super::PdfPathCommand::MoveTo(x, y) if (x - 0.001).abs() < f32::EPSILON && (y - 200.0).abs() < f32::EPSILON)
        );
        assert!(
            matches!(commands[1], super::PdfPathCommand::LineTo(x, y) if (x - 35.0).abs() < f32::EPSILON && (y - 4.0).abs() < f32::EPSILON)
        );
        assert!(matches!(commands[2], super::PdfPathCommand::Close));
    }

    fn create_pptx() -> Vec<u8> {
        let mut output = Cursor::new(Vec::new());
        {
            let mut archive = zip::ZipWriter::new(&mut output);
            let options = SimpleFileOptions::default();
            write_entry(
                &mut archive,
                options,
                "ppt/presentation.xml",
                r#"<p:presentation xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><p:sldIdLst><p:sldId id="256" r:id="rId1"/></p:sldIdLst><p:sldSz cx="9144000" cy="6858000"/></p:presentation>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/_rels/presentation.xml.rels",
                r#"<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide" Target="slides/slide1.xml"/></Relationships>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/slides/slide1.xml",
                r#"<p:sld xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"><p:cSld><p:spTree><p:sp><p:spPr><a:xfrm><a:off x="127000" y="254000"/><a:ext cx="2540000" cy="1270000"/></a:xfrm><a:prstGeom prst="rect"/><a:solidFill><a:srgbClr val="336699"/></a:solidFill><a:ln w="12700"><a:solidFill><a:srgbClr val="000000"/></a:solidFill></a:ln></p:spPr><p:txBody><a:bodyPr/><a:p><a:r><a:rPr sz="1800" b="1"/><a:t>Hello Rust PPTX</a:t></a:r></a:p></p:txBody></p:sp><p:cxnSp><p:spPr><a:xfrm><a:off x="127000" y="1778000"/><a:ext cx="2540000" cy="127000"/></a:xfrm><a:ln w="25400"><a:solidFill><a:srgbClr val="FF0000"/></a:solidFill><a:prstDash val="dash"/></a:ln></p:spPr></p:cxnSp></p:spTree></p:cSld></p:sld>"#,
            );
            archive.finish().expect("finish PPTX ZIP");
        }
        output.into_inner()
    }

    fn create_svg_and_smartart_pptx() -> Vec<u8> {
        let mut output = Cursor::new(Vec::new());
        {
            let mut archive = zip::ZipWriter::new(&mut output);
            let options = SimpleFileOptions::default();
            write_entry(
                &mut archive,
                options,
                "ppt/presentation.xml",
                r#"<p:presentation xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><p:sldIdLst><p:sldId id="256" r:id="rId1"/></p:sldIdLst><p:sldSz cx="9144000" cy="6858000"/></p:presentation>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/_rels/presentation.xml.rels",
                r#"<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide" Target="slides/slide1.xml"/></Relationships>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/slides/slide1.xml",
                r#"<p:sld xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships" xmlns:asvg="http://schemas.microsoft.com/office/drawing/2016/SVG/main" xmlns:dgm="http://schemas.openxmlformats.org/drawingml/2006/diagram"><p:cSld><p:spTree><p:pic><p:nvPicPr><p:cNvPr id="2" name="SVG"/></p:nvPicPr><p:blipFill><a:blip><a:extLst><a:ext><asvg:svgBlip r:embed="rIdImage"/></a:ext></a:extLst></a:blip><a:srcRect l="25000" t="25000" r="25000" b="25000"/></p:blipFill><p:spPr><a:xfrm><a:off x="914400" y="914400"/><a:ext cx="1828800" cy="1828800"/></a:xfrm></p:spPr></p:pic><p:graphicFrame><p:xfrm><a:off x="3657600" y="914400"/><a:ext cx="3657600" cy="1828800"/></p:xfrm><a:graphic><a:graphicData uri="http://schemas.openxmlformats.org/drawingml/2006/diagram"><dgm:relIds/></a:graphicData></a:graphic></p:graphicFrame></p:spTree></p:cSld></p:sld>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/slides/_rels/slide1.xml.rels",
                r#"<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rIdImage" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/image" Target="../media/image1.svg"/><Relationship Id="rIdDiagram" Type="http://schemas.microsoft.com/office/2007/relationships/diagramDrawing" Target="../diagrams/drawing1.xml"/></Relationships>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/media/image1.svg",
                r##"<svg viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg"><path d="M0 0 L100 0 L100 100 Z" fill="#E56925"/></svg>"##,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/diagrams/drawing1.xml",
                r#"<dsp:drawing xmlns:dsp="http://schemas.microsoft.com/office/drawing/2008/diagram" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"><dsp:spTree><dsp:sp><dsp:spPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="3657600" cy="1828800"/></a:xfrm><a:prstGeom prst="rect"/><a:solidFill><a:srgbClr val="2F5597"/></a:solidFill></dsp:spPr><dsp:txBody><a:bodyPr/><a:p><a:r><a:rPr sz="1800"><a:solidFill><a:srgbClr val="FFFFFF"/></a:solidFill></a:rPr><a:t>SmartArt Text</a:t></a:r></a:p></dsp:txBody></dsp:sp></dsp:spTree></dsp:drawing>"#,
            );
            archive.finish().expect("finish PPTX ZIP");
        }
        output.into_inner()
    }

    fn create_placeholder_pptx() -> Vec<u8> {
        let mut output = Cursor::new(Vec::new());
        {
            let mut archive = zip::ZipWriter::new(&mut output);
            let options = SimpleFileOptions::default();
            write_entry(
                &mut archive,
                options,
                "ppt/presentation.xml",
                r#"<p:presentation xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><p:sldIdLst><p:sldId id="256" r:id="rId1"/></p:sldIdLst><p:sldSz cx="9144000" cy="6858000"/></p:presentation>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/_rels/presentation.xml.rels",
                r#"<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide" Target="slides/slide1.xml"/></Relationships>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/slides/slide1.xml",
                r#"<p:sld xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"><p:cSld><p:spTree><p:sp><p:nvSpPr><p:nvPr><p:ph type="title"/></p:nvPr></p:nvSpPr><p:spPr/><p:txBody><a:bodyPr anchor="ctr"/><a:p><a:r><a:t>Centered Title</a:t></a:r></a:p></p:txBody></p:sp><p:sp><p:nvSpPr><p:nvPr><p:ph type="body" idx="1"/></p:nvPr></p:nvSpPr><p:spPr/><p:txBody><a:bodyPr/><a:p><a:r><a:t>Heading</a:t></a:r></a:p><a:p><a:pPr lvl="1"/><a:r><a:t>Indented bullet</a:t></a:r></a:p></p:txBody></p:sp></p:spTree></p:cSld></p:sld>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/slides/_rels/slide1.xml.rels",
                r#"<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideLayout" Target="../slideLayouts/slideLayout1.xml"/></Relationships>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/slideLayouts/slideLayout1.xml",
                r#"<p:sldLayout xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"><p:cSld><p:spTree><p:sp><p:nvSpPr><p:nvPr><p:ph type="title"/></p:nvPr></p:nvSpPr><p:spPr><a:xfrm><a:off x="914400" y="914400"/><a:ext cx="3657600" cy="914400"/></a:xfrm></p:spPr><p:txBody><a:bodyPr anchor="ctr"/><a:lstStyle><a:lvl1pPr algn="ctr"><a:defRPr sz="2400"/></a:lvl1pPr></a:lstStyle></p:txBody></p:sp><p:sp><p:nvSpPr><p:nvPr><p:ph type="body" idx="1"/></p:nvPr></p:nvSpPr><p:spPr><a:xfrm><a:off x="914400" y="2286000"/><a:ext cx="3657600" cy="2743200"/></a:xfrm></p:spPr><p:txBody><a:bodyPr/><a:lstStyle><a:lvl1pPr marL="0" indent="0"><a:buNone/><a:defRPr sz="1800"/></a:lvl1pPr><a:lvl2pPr marL="283464" indent="-283464"><a:defRPr sz="1800"/></a:lvl2pPr></a:lstStyle></p:txBody></p:sp></p:spTree></p:cSld></p:sldLayout>"#,
            );
            archive.finish().expect("finish PPTX ZIP");
        }
        output.into_inner()
    }

    fn create_reordered_pptx() -> Vec<u8> {
        let mut output = Cursor::new(Vec::new());
        {
            let mut archive = zip::ZipWriter::new(&mut output);
            let options = SimpleFileOptions::default();
            write_entry(
                &mut archive,
                options,
                "ppt/presentation.xml",
                r#"<p:presentation xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><p:sldIdLst><p:sldId id="256" r:id="rId2"/><p:sldId id="257" r:id="rId1"/></p:sldIdLst><p:sldSz cx="9144000" cy="6858000"/></p:presentation>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/_rels/presentation.xml.rels",
                r#"<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide" Target="slides/slide1.xml"/><Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide" Target="slides/slide2.xml"/></Relationships>"#,
            );
            for (index, text) in [(1, "First"), (2, "Second")] {
                write_entry(
                    &mut archive,
                    options,
                    &format!("ppt/slides/slide{index}.xml"),
                    &format!(
                        r#"<p:sld xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"><p:cSld><p:spTree><p:sp><p:spPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="1270000" cy="1270000"/></a:xfrm></p:spPr><p:txBody><a:bodyPr/><a:p><a:r><a:t>{text}</a:t></a:r></a:p></p:txBody></p:sp></p:spTree></p:cSld></p:sld>"#
                    ),
                );
            }
            archive.finish().expect("finish PPTX ZIP");
        }
        output.into_inner()
    }

    fn create_broken_relationship_pptx() -> Vec<u8> {
        let mut output = Cursor::new(Vec::new());
        {
            let mut archive = zip::ZipWriter::new(&mut output);
            let options = SimpleFileOptions::default();
            write_entry(
                &mut archive,
                options,
                "ppt/presentation.xml",
                r#"<p:presentation xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><p:sldIdLst><p:sldId id="256" r:id="rIdMissing"/></p:sldIdLst></p:presentation>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/_rels/presentation.xml.rels",
                r#"<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rIdMissing" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide" Target="slides/missing.xml"/></Relationships>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/slides/slide1.xml",
                r#"<p:sld xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"><p:cSld><p:spTree><p:sp><p:spPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="1270000" cy="1270000"/></a:xfrm></p:spPr><p:txBody><a:bodyPr/><a:p><a:r><a:t>Recovered</a:t></a:r></a:p></p:txBody></p:sp></p:spTree></p:cSld></p:sld>"#,
            );
            archive.finish().expect("finish PPTX ZIP");
        }
        output.into_inner()
    }

    fn create_invalid_picture_bounds_pptx() -> Vec<u8> {
        let mut output = Cursor::new(Vec::new());
        {
            let mut archive = zip::ZipWriter::new(&mut output);
            let options = SimpleFileOptions::default();
            write_entry(
                &mut archive,
                options,
                "ppt/presentation.xml",
                r#"<p:presentation xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><p:sldIdLst><p:sldId id="256" r:id="rId1"/></p:sldIdLst></p:presentation>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/_rels/presentation.xml.rels",
                r#"<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide" Target="slides/slide1.xml"/></Relationships>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/slides/slide1.xml",
                r#"<p:sld xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><p:cSld><p:spTree><p:pic><p:blipFill><a:blip r:embed="rIdImage"/></p:blipFill><p:spPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="0" cy="1270000"/></a:xfrm></p:spPr></p:pic><p:pic><p:blipFill><a:blip r:embed="rIdImage"/></p:blipFill><p:spPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="1270000" cy="-1"/></a:xfrm></p:spPr></p:pic></p:spTree></p:cSld></p:sld>"#,
            );
            write_entry(
                &mut archive,
                options,
                "ppt/slides/_rels/slide1.xml.rels",
                r#"<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rIdImage" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/image" Target="../media/image1.jpg"/></Relationships>"#,
            );
            archive
                .start_file("ppt/media/image1.jpg", options)
                .expect("start image entry");
            archive
                .write_all(&[
                    0xff, 0xd8, 0xff, 0xc0, 0x00, 0x07, 0x08, 0x00, 0x01, 0x00, 0x01,
                ])
                .expect("write image entry");
            archive.finish().expect("finish PPTX ZIP");
        }
        output.into_inner()
    }

    fn write_entry(
        archive: &mut zip::ZipWriter<&mut Cursor<Vec<u8>>>,
        options: SimpleFileOptions,
        path: &str,
        content: &str,
    ) {
        archive.start_file(path, options).expect("start ZIP entry");
        archive
            .write_all(content.as_bytes())
            .expect("write ZIP entry");
    }
}
