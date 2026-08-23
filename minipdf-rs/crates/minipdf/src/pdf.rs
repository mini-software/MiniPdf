use std::collections::BTreeMap;

use crate::RegisteredFont;

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct PdfColor {
    pub r: f32,
    pub g: f32,
    pub b: f32,
}

impl PdfColor {
    pub const BLACK: Self = Self::new(0.0, 0.0, 0.0);
    pub const WHITE: Self = Self::new(1.0, 1.0, 1.0);
    pub const LIGHT_GRAY: Self = Self::new(0.92, 0.92, 0.92);
    pub const TABLE_HEADER: Self = Self::new(0.86, 0.91, 0.96);

    pub const fn new(r: f32, g: f32, b: f32) -> Self {
        Self { r, g, b }
    }
}

#[derive(Debug, Clone, Copy)]
pub struct PdfTextStyle {
    pub color: PdfColor,
    pub bold: bool,
    pub italic: bool,
}

#[derive(Debug, Clone)]
enum PdfOp {
    Text {
        text: String,
        x: f32,
        y: f32,
        font_size: f32,
        color: PdfColor,
        bold: bool,
        italic: bool,
    },
    Rect {
        x: f32,
        y: f32,
        width: f32,
        height: f32,
        color: PdfColor,
    },
    Line {
        x1: f32,
        y1: f32,
        x2: f32,
        y2: f32,
        color: PdfColor,
        width: f32,
    },
    Image {
        image_id: usize,
        x: f32,
        y: f32,
        width: f32,
        height: f32,
    },
}

#[derive(Debug, Clone)]
struct PdfJpegImage {
    data: Vec<u8>,
    width: u16,
    height: u16,
}

#[derive(Debug, Clone)]
pub struct PdfPage {
    pub width: f32,
    pub height: f32,
    ops: Vec<PdfOp>,
}

impl PdfPage {
    fn new(width: f32, height: f32) -> Self {
        Self {
            width,
            height,
            ops: Vec::new(),
        }
    }

    pub fn add_text(
        &mut self,
        text: impl Into<String>,
        x: f32,
        y: f32,
        font_size: f32,
        color: PdfColor,
        bold: bool,
    ) {
        self.add_styled_text(
            text,
            x,
            y,
            font_size,
            PdfTextStyle {
                color,
                bold,
                italic: false,
            },
        );
    }

    pub fn add_styled_text(
        &mut self,
        text: impl Into<String>,
        x: f32,
        y: f32,
        font_size: f32,
        style: PdfTextStyle,
    ) {
        self.ops.push(PdfOp::Text {
            text: text.into(),
            x,
            y,
            font_size,
            color: style.color,
            bold: style.bold,
            italic: style.italic,
        });
    }

    pub fn add_rect(&mut self, x: f32, y: f32, width: f32, height: f32, color: PdfColor) {
        self.ops.push(PdfOp::Rect {
            x,
            y,
            width,
            height,
            color,
        });
    }

    pub fn add_line(&mut self, x1: f32, y1: f32, x2: f32, y2: f32, color: PdfColor, width: f32) {
        self.ops.push(PdfOp::Line {
            x1,
            y1,
            x2,
            y2,
            color,
            width,
        });
    }

    pub fn add_image(&mut self, image_id: usize, x: f32, y: f32, width: f32, height: f32) {
        self.ops.push(PdfOp::Image {
            image_id,
            x,
            y,
            width,
            height,
        });
    }
}

#[derive(Debug, Clone)]
pub struct PdfDocument {
    pages: Vec<PdfPage>,
    jpeg_images: Vec<PdfJpegImage>,
    fonts: Vec<RegisteredFont>,
}

#[derive(Debug)]
struct EmbeddedFont {
    registered_index: usize,
    resource_name: String,
    object_id: usize,
    glyphs: BTreeMap<u16, String>,
    cid_by_glyph: BTreeMap<u16, u16>,
}

impl PdfDocument {
    pub fn new() -> Self {
        Self {
            pages: Vec::new(),
            jpeg_images: Vec::new(),
            fonts: crate::registered_fonts(),
        }
    }

    pub fn add_jpeg_image(&mut self, data: Vec<u8>, width: u16, height: u16) -> usize {
        let image_id = self.jpeg_images.len();
        self.jpeg_images.push(PdfJpegImage {
            data,
            width,
            height,
        });
        image_id
    }

    pub fn add_page(&mut self, width: f32, height: f32) -> &mut PdfPage {
        self.pages.push(PdfPage::new(width, height));
        self.pages.last_mut().expect("page was just pushed")
    }

    pub fn pages(&self) -> &[PdfPage] {
        &self.pages
    }

    pub fn page_mut(&mut self, index: usize) -> Option<&mut PdfPage> {
        self.pages.get_mut(index)
    }

    pub fn to_bytes(&self) -> Vec<u8> {
        let mut objects: Vec<Vec<u8>> = Vec::new();
        let page_count = self.pages.len();

        objects.push(b"<< /Type /Catalog /Pages 2 0 R >>".to_vec());
        objects.push(Vec::new());
        objects.push(b"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>".to_vec());
        objects.push(b"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>".to_vec());
        objects.push(b"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Oblique >>".to_vec());
        objects
            .push(b"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-BoldOblique >>".to_vec());

        let mut embedded_fonts = prepare_embedded_fonts(&self.pages, &self.fonts);
        for font in &mut embedded_fonts {
            font.object_id = append_embedded_font_objects(&mut objects, font, &self.fonts);
        }

        let mut image_ids = Vec::with_capacity(self.jpeg_images.len());
        for image in &self.jpeg_images {
            let mut image_object = format!(
                "<< /Type /XObject /Subtype /Image /Width {} /Height {} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {} >>\nstream\n",
                image.width,
                image.height,
                image.data.len()
            )
            .into_bytes();
            image_object.extend_from_slice(&image.data);
            image_object.extend_from_slice(b"\nendstream");
            image_ids.push(push_object(&mut objects, image_object));
        }

        let mut page_ids = Vec::with_capacity(page_count);
        for page in &self.pages {
            let mut content = write_content_stream(page, &self.fonts, &embedded_fonts);
            if content.ends_with(b"\n") {
                content.pop();
            }

            let mut content_object = Vec::new();
            content_object
                .extend_from_slice(format!("<< /Length {} >>\nstream\n", content.len()).as_bytes());
            content_object.extend_from_slice(&content);
            content_object.extend_from_slice(b"\nendstream");
            let content_id = push_object(&mut objects, content_object);

            let xobjects = image_ids
                .iter()
                .enumerate()
                .map(|(index, object_id)| format!("/Im{} {} 0 R", index + 1, object_id))
                .collect::<Vec<_>>()
                .join(" ");
            let embedded_resources = embedded_fonts
                .iter()
                .map(|font| format!("/{} {} 0 R", font.resource_name, font.object_id))
                .collect::<Vec<_>>()
                .join(" ");
            let page_object = format!(
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {:.2} {:.2}] /Resources << /Font << /F1 3 0 R /F2 4 0 R /F3 5 0 R /F4 6 0 R {} >> /XObject << {} >> >> /Contents {} 0 R >>",
                page.width, page.height, embedded_resources, xobjects, content_id
            );
            page_ids.push(push_object(&mut objects, page_object.into_bytes()));
        }

        let kids = page_ids
            .iter()
            .map(|id| format!("{id} 0 R"))
            .collect::<Vec<_>>()
            .join(" ");
        objects[1] = format!("<< /Type /Pages /Kids [{kids}] /Count {page_count} >>").into_bytes();

        write_objects(objects)
    }
}

impl Default for PdfDocument {
    fn default() -> Self {
        Self::new()
    }
}

fn push_object(objects: &mut Vec<Vec<u8>>, object: Vec<u8>) -> usize {
    objects.push(object);
    objects.len()
}

fn prepare_embedded_fonts(pages: &[PdfPage], fonts: &[RegisteredFont]) -> Vec<EmbeddedFont> {
    let mut used_glyphs: BTreeMap<usize, BTreeMap<u16, String>> = BTreeMap::new();
    for page in pages {
        for op in &page.ops {
            let PdfOp::Text {
                text, bold, italic, ..
            } = op
            else {
                continue;
            };
            for run in split_font_runs(text, fonts, *bold, *italic) {
                let Some(font_index) = run.font_index else {
                    continue;
                };
                let Some(shaped) = shape_text(&run.text, &fonts[font_index].data) else {
                    continue;
                };
                let glyphs = used_glyphs.entry(font_index).or_default();
                for (glyph_index, glyph) in shaped.glyphs.iter().enumerate() {
                    glyphs.entry(glyph.id).or_insert_with(|| {
                        text_for_cluster(&run.text, &shaped.glyphs, glyph_index)
                    });
                }
            }
        }
    }

    used_glyphs
        .into_iter()
        .enumerate()
        .map(
            |(resource_index, (registered_index, glyphs))| EmbeddedFont {
                registered_index,
                resource_name: format!("FU{}", resource_index + 1),
                object_id: 0,
                cid_by_glyph: glyphs
                    .keys()
                    .enumerate()
                    .map(|(index, glyph_id)| (*glyph_id, index as u16 + 1))
                    .collect(),
                glyphs,
            },
        )
        .collect()
}

fn append_embedded_font_objects(
    objects: &mut Vec<Vec<u8>>,
    font: &EmbeddedFont,
    registered_fonts: &[RegisteredFont],
) -> usize {
    let registered = &registered_fonts[font.registered_index];
    let face = ttf_parser::Face::parse(&registered.data, 0)
        .expect("font was validated while preparing PDF resources");
    let pdf_name = format!("MPFont{}", font.registered_index + 1);
    let mut remapper = subsetter::GlyphRemapper::new();
    for glyph_id in font.glyphs.keys() {
        remapper.remap(*glyph_id);
    }
    let font_data = subsetter::subset(&registered.data, 0, &remapper)
        .expect("font was validated while preparing PDF resources");

    let mut font_file = format!(
        "<< /Length {} /Length1 {} >>\nstream\n",
        font_data.len(),
        font_data.len()
    )
    .into_bytes();
    font_file.extend_from_slice(&font_data);
    font_file.extend_from_slice(b"\nendstream");
    let font_file_id = push_object(objects, font_file);

    let bbox = face.global_bounding_box();
    let descriptor = format!(
        "<< /Type /FontDescriptor /FontName /{pdf_name} /Flags 32 /FontBBox [{} {} {} {}] /ItalicAngle 0 /Ascent {} /Descent {} /CapHeight {} /StemV 80 /FontFile2 {} 0 R >>",
        bbox.x_min,
        bbox.y_min,
        bbox.x_max,
        bbox.y_max,
        face.ascender(),
        face.descender(),
        face.capital_height().unwrap_or_else(|| face.ascender()),
        font_file_id
    );
    let descriptor_id = push_object(objects, descriptor.into_bytes());

    let units_per_em = u32::from(face.units_per_em());
    let widths = font
        .glyphs
        .keys()
        .map(|glyph_id| {
            let advance = u32::from(
                face.glyph_hor_advance(ttf_parser::GlyphId(*glyph_id))
                    .unwrap_or(face.units_per_em()),
            );
            format!(
                "{} [{}]",
                remapper.get(*glyph_id).expect("glyph was remapped"),
                advance * 1000 / units_per_em
            )
        })
        .collect::<Vec<_>>()
        .join(" ");
    let cid_font = format!(
        "<< /Type /Font /Subtype /CIDFontType2 /BaseFont /{pdf_name} /CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >> /FontDescriptor {} 0 R /DW 1000 /W [{}] /CIDToGIDMap /Identity >>",
        descriptor_id, widths
    );
    let cid_font_id = push_object(objects, cid_font.into_bytes());

    let to_unicode = build_to_unicode_cmap(&font.glyphs, &remapper);
    let mut to_unicode_object =
        format!("<< /Length {} >>\nstream\n", to_unicode.len()).into_bytes();
    to_unicode_object.extend_from_slice(to_unicode.as_bytes());
    to_unicode_object.extend_from_slice(b"\nendstream");
    let to_unicode_id = push_object(objects, to_unicode_object);

    let type0_font = format!(
        "<< /Type /Font /Subtype /Type0 /BaseFont /{pdf_name} /Encoding /Identity-H /DescendantFonts [{} 0 R] /ToUnicode {} 0 R >>",
        cid_font_id, to_unicode_id
    );
    push_object(objects, type0_font.into_bytes())
}

fn build_to_unicode_cmap(
    glyphs: &BTreeMap<u16, String>,
    remapper: &subsetter::GlyphRemapper,
) -> String {
    let mut cmap = String::from(
        "/CIDInit /ProcSet findresource begin\n12 dict begin\nbegincmap\n/CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def\n/CMapName /Adobe-Identity-UCS def\n/CMapType 2 def\n1 begincodespacerange\n<0000> <FFFF>\nendcodespacerange\n",
    );
    for chunk in glyphs.iter().collect::<Vec<_>>().chunks(100) {
        cmap.push_str(&format!("{} beginbfchar\n", chunk.len()));
        for (glyph_id, text) in chunk {
            let cid = remapper.get(**glyph_id).expect("glyph was remapped");
            cmap.push_str(&format!("<{cid:04X}> <{}>\n", utf16be_hex(text, false)));
        }
        cmap.push_str("endbfchar\n");
    }
    cmap.push_str("endcmap\nCMapName currentdict /CMap defineresource pop\nend\nend");
    cmap
}

#[derive(Debug)]
struct FontRun {
    text: String,
    font_index: Option<usize>,
}

#[derive(Debug)]
struct ShapedGlyph {
    id: u16,
    cluster: usize,
    x_advance: i32,
    y_advance: i32,
    x_offset: i32,
    y_offset: i32,
}

#[derive(Debug)]
struct ShapedText {
    glyphs: Vec<ShapedGlyph>,
    units_per_em: i32,
}

fn split_font_runs(text: &str, fonts: &[RegisteredFont], bold: bool, italic: bool) -> Vec<FontRun> {
    let mut runs: Vec<FontRun> = Vec::new();
    for ch in text.chars() {
        let font_index = if ch.is_whitespace() || ch.is_ascii_punctuation() || ch == '\u{fe0f}' {
            runs.last().and_then(|run| run.font_index)
        } else {
            select_font(fonts, ch, bold, italic)
        };

        if let Some(run) = runs.last_mut().filter(|run| run.font_index == font_index) {
            run.text.push(ch);
        } else {
            runs.push(FontRun {
                text: ch.to_string(),
                font_index,
            });
        }
    }
    runs
}

fn select_font(fonts: &[RegisteredFont], ch: char, bold: bool, italic: bool) -> Option<usize> {
    fonts
        .iter()
        .enumerate()
        .filter(|(_, font)| font_supports(font, ch))
        .min_by_key(|(_, font)| font_preference(&font.name, ch, bold, italic))
        .map(|(index, _)| index)
}

fn font_preference(name: &str, ch: char, bold: bool, italic: bool) -> u8 {
    let name = name.to_ascii_lowercase();
    let codepoint = ch as u32;
    let preferred = if matches!(codepoint, 0x1100..=0x11ff | 0x3130..=0x318f | 0xac00..=0xd7af) {
        "malgunsl"
    } else if matches!(codepoint, 0x0e00..=0x0e7f) {
        "micross"
    } else if matches!(codepoint, 0x0900..=0x097f) {
        "nirmala"
    } else if matches!(codepoint, 0x2e80..=0x9fff | 0xf900..=0xfaff)
        || matches!(codepoint, 0x2190..=0x2bff)
    {
        "notosanssc"
    } else if codepoint >= 0x1f000 {
        "seguiemj"
    } else if bold && italic {
        "calibriz"
    } else if bold {
        "calibrib"
    } else if italic {
        "calibrii"
    } else {
        "calibri"
    };

    if name == preferred {
        0
    } else if name.starts_with(preferred) {
        1
    } else if name == "calibri" {
        2
    } else {
        10
    }
}

fn font_supports(font: &RegisteredFont, ch: char) -> bool {
    is_embeddable_truetype(&font.data)
        && ttf_parser::Face::parse(&font.data, 0)
            .ok()
            .and_then(|face| face.glyph_index(ch))
            .is_some()
}

fn is_embeddable_truetype(data: &[u8]) -> bool {
    data.starts_with(b"\0\x01\0\0") || data.starts_with(b"true") || data.starts_with(b"ttcf")
}

fn shape_text(text: &str, font_data: &[u8]) -> Option<ShapedText> {
    let face = rustybuzz::Face::from_slice(font_data, 0)?;
    let units_per_em = face.units_per_em();
    let mut buffer = rustybuzz::UnicodeBuffer::new();
    buffer.push_str(text);
    buffer.guess_segment_properties();
    let output = rustybuzz::shape(&face, &[], buffer);
    let glyphs = output
        .glyph_infos()
        .iter()
        .zip(output.glyph_positions())
        .filter_map(|(info, position)| {
            Some(ShapedGlyph {
                id: u16::try_from(info.glyph_id).ok()?,
                cluster: usize::try_from(info.cluster).ok()?,
                x_advance: position.x_advance,
                y_advance: position.y_advance,
                x_offset: position.x_offset,
                y_offset: position.y_offset,
            })
        })
        .collect();
    Some(ShapedText {
        glyphs,
        units_per_em,
    })
}

pub(crate) fn styled_text_width(text: &str, font_size: f32, bold: bool, italic: bool) -> f32 {
    let fonts = crate::registered_fonts();
    split_font_runs(text, &fonts, bold, italic)
        .into_iter()
        .map(|run| {
            let Some(font_index) = run.font_index else {
                return run.text.chars().count() as f32 * font_size * 0.5;
            };
            let Some(shaped) = shape_text(&run.text, &fonts[font_index].data) else {
                return run.text.chars().count() as f32 * font_size * 0.5;
            };
            let scale = font_size / shaped.units_per_em as f32;
            shaped
                .glyphs
                .iter()
                .map(|glyph| glyph.x_advance as f32 * scale)
                .sum()
        })
        .sum()
}

fn text_for_cluster(text: &str, glyphs: &[ShapedGlyph], index: usize) -> String {
    let start = glyphs[index].cluster.min(text.len());
    let end = glyphs
        .iter()
        .map(|glyph| glyph.cluster)
        .filter(|cluster| *cluster > start)
        .min()
        .unwrap_or(text.len());
    text.get(start..end).unwrap_or("\u{fffd}").to_owned()
}

fn utf16be_hex(text: &str, include_bom: bool) -> String {
    let mut result = String::new();
    if include_bom {
        result.push_str("FEFF");
    }
    for code_unit in text.encode_utf16() {
        result.push_str(&format!("{code_unit:04X}"));
    }
    result
}

fn write_objects(objects: Vec<Vec<u8>>) -> Vec<u8> {
    let mut pdf = Vec::new();
    pdf.extend_from_slice(b"%PDF-1.4\n%\xE2\xE3\xCF\xD3\n");

    let mut offsets = Vec::with_capacity(objects.len());
    for (index, object) in objects.iter().enumerate() {
        offsets.push(pdf.len());
        pdf.extend_from_slice(format!("{} 0 obj\n", index + 1).as_bytes());
        pdf.extend_from_slice(object);
        pdf.extend_from_slice(b"\nendobj\n");
    }

    let xref_start = pdf.len();
    pdf.extend_from_slice(format!("xref\n0 {}\n", objects.len() + 1).as_bytes());
    pdf.extend_from_slice(b"0000000000 65535 f \n");
    for offset in offsets {
        pdf.extend_from_slice(format!("{offset:010} 00000 n \n").as_bytes());
    }
    pdf.extend_from_slice(
        format!(
            "trailer\n<< /Size {} /Root 1 0 R >>\nstartxref\n{}\n%%EOF\n",
            objects.len() + 1,
            xref_start
        )
        .as_bytes(),
    );
    pdf
}

fn write_content_stream(
    page: &PdfPage,
    fonts: &[RegisteredFont],
    embedded_fonts: &[EmbeddedFont],
) -> Vec<u8> {
    let mut content = String::new();
    for op in &page.ops {
        match op {
            PdfOp::Text {
                text,
                x,
                y,
                font_size,
                color,
                bold,
                italic,
            } => {
                let built_in_font = match (*bold, *italic) {
                    (false, false) => "F1",
                    (true, false) => "F2",
                    (false, true) => "F3",
                    (true, true) => "F4",
                };
                let mut cursor_x = *x;
                let mut cursor_y = *y;
                for run in split_font_runs(text, fonts, *bold, *italic) {
                    let Some(font_index) = run.font_index else {
                        content.push_str(&format!(
                            "BT /{built_in_font} {:.2} Tf {:.3} {:.3} {:.3} rg {:.2} {:.2} Td ({}) Tj ET\n",
                            font_size,
                            clamp_color(color.r),
                            clamp_color(color.g),
                            clamp_color(color.b),
                            cursor_x,
                            cursor_y,
                            escape_pdf_text(&run.text)
                        ));
                        cursor_x += run.text.chars().count() as f32 * font_size * 0.5;
                        continue;
                    };
                    let Some(resource) = embedded_fonts
                        .iter()
                        .find(|font| font.registered_index == font_index)
                    else {
                        continue;
                    };
                    let Some(shaped) = shape_text(&run.text, &fonts[font_index].data) else {
                        continue;
                    };
                    let scale = font_size / shaped.units_per_em as f32;
                    content.push_str(&format!(
                        "/Span << /ActualText <{}> >> BDC\n",
                        utf16be_hex(&run.text, true)
                    ));
                    for glyph in shaped.glyphs {
                        let glyph_x = cursor_x + glyph.x_offset as f32 * scale;
                        let glyph_y = cursor_y + glyph.y_offset as f32 * scale;
                        content.push_str(&format!(
                            "BT /{} {:.2} Tf {:.3} {:.3} {:.3} rg 1 0 0 1 {:.2} {:.2} Tm <{:04X}> Tj ET\n",
                            resource.resource_name,
                            font_size,
                            clamp_color(color.r),
                            clamp_color(color.g),
                            clamp_color(color.b),
                            glyph_x,
                            glyph_y,
                            resource
                                .cid_by_glyph
                                .get(&glyph.id)
                                .expect("shaped glyph was registered")
                        ));
                        cursor_x += glyph.x_advance as f32 * scale;
                        cursor_y += glyph.y_advance as f32 * scale;
                    }
                    content.push_str("EMC\n");
                }
            }
            PdfOp::Rect {
                x,
                y,
                width,
                height,
                color,
            } => {
                content.push_str(&format!(
                    "{:.3} {:.3} {:.3} rg {:.2} {:.2} {:.2} {:.2} re f\n",
                    clamp_color(color.r),
                    clamp_color(color.g),
                    clamp_color(color.b),
                    x,
                    y,
                    width,
                    height
                ));
            }
            PdfOp::Line {
                x1,
                y1,
                x2,
                y2,
                color,
                width,
            } => {
                content.push_str(&format!(
                    "{:.3} {:.3} {:.3} RG {:.2} w {:.2} {:.2} m {:.2} {:.2} l S\n",
                    clamp_color(color.r),
                    clamp_color(color.g),
                    clamp_color(color.b),
                    width,
                    x1,
                    y1,
                    x2,
                    y2
                ));
            }
            PdfOp::Image {
                image_id,
                x,
                y,
                width,
                height,
            } => {
                content.push_str(&format!(
                    "q {:.2} 0 0 {:.2} {:.2} {:.2} cm /Im{} Do Q\n",
                    width,
                    height,
                    x,
                    y,
                    image_id + 1
                ));
            }
        }
    }
    content.into_bytes()
}

fn clamp_color(value: f32) -> f32 {
    value.clamp(0.0, 1.0)
}

fn escape_pdf_text(text: &str) -> String {
    let mut result = String::new();
    for ch in text.chars() {
        match ch {
            '(' => result.push_str("\\("),
            ')' => result.push_str("\\)"),
            '\\' => result.push_str("\\\\"),
            '\n' | '\r' | '\t' => result.push(' '),
            ch if ch.is_control() => result.push(' '),
            ch if ch.is_ascii() => result.push(ch),
            _ => result.push('?'),
        }
    }
    result
}

#[cfg(test)]
mod tests {
    use super::PdfDocument;

    #[test]
    fn writes_jpeg_image_xobject_and_draw_operation() {
        let mut document = PdfDocument::new();
        let image_id = document.add_jpeg_image(vec![0xff, 0xd8, 0xff, 0xd9], 2, 3);
        document
            .add_page(100.0, 100.0)
            .add_image(image_id, 10.0, 20.0, 30.0, 40.0);

        let pdf = String::from_utf8_lossy(&document.to_bytes()).into_owned();
        assert!(pdf.contains("/Filter /DCTDecode"));
        assert!(pdf.contains("/Width 2 /Height 3"));
        assert!(pdf.contains("30.00 0 0 40.00 10.00 20.00 cm /Im1 Do"));
    }
}
