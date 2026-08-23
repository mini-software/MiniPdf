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

#[derive(Debug, Default, Clone)]
pub struct PdfDocument {
    pages: Vec<PdfPage>,
    jpeg_images: Vec<PdfJpegImage>,
}

impl PdfDocument {
    pub fn new() -> Self {
        Self {
            pages: Vec::new(),
            jpeg_images: Vec::new(),
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
            objects.push(image_object);
        }

        let mut page_ids = Vec::with_capacity(page_count);
        for (page_index, page) in self.pages.iter().enumerate() {
            let content_id = 7 + self.jpeg_images.len() + page_index * 2;
            let page_id = content_id + 1;
            page_ids.push(page_id);

            let mut content = write_content_stream(page);
            if content.ends_with(b"\n") {
                content.pop();
            }

            let mut content_object = Vec::new();
            content_object
                .extend_from_slice(format!("<< /Length {} >>\nstream\n", content.len()).as_bytes());
            content_object.extend_from_slice(&content);
            content_object.extend_from_slice(b"\nendstream");
            objects.push(content_object);

            let xobjects = self
                .jpeg_images
                .iter()
                .enumerate()
                .map(|(index, _)| format!("/Im{} {} 0 R", index + 1, index + 7))
                .collect::<Vec<_>>()
                .join(" ");
            let page_object = format!(
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {:.2} {:.2}] /Resources << /Font << /F1 3 0 R /F2 4 0 R /F3 5 0 R /F4 6 0 R >> /XObject << {} >> >> /Contents {} 0 R >>",
                page.width, page.height, xobjects, content_id
            );
            objects.push(page_object.into_bytes());
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

fn write_content_stream(page: &PdfPage) -> Vec<u8> {
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
                let font = match (*bold, *italic) {
                    (false, false) => "F1",
                    (true, false) => "F2",
                    (false, true) => "F3",
                    (true, true) => "F4",
                };
                content.push_str(&format!(
                    "BT /{font} {:.2} Tf {:.3} {:.3} {:.3} rg {:.2} {:.2} Td ({}) Tj ET\n",
                    font_size,
                    clamp_color(color.r),
                    clamp_color(color.g),
                    clamp_color(color.b),
                    x,
                    y,
                    escape_pdf_text(text)
                ));
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
