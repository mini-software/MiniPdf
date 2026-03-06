"""
Generate 30 classic DOCX test files for the MiniPdf benchmark suite.

Each file tests a different Word document feature, from simple paragraphs
to tables, images, lists, headings, and mixed content.

Prerequisites:
    pip install python-docx Pillow

Usage:
    python generate_classic_docx.py            # output to ./output_docx/
    python generate_classic_docx.py --outdir /path/to/dir
"""

import argparse
import io
import os
import struct
import sys
from pathlib import Path

try:
    from docx import Document
    from docx.shared import Pt, Inches, Cm, RGBColor, Emu
    from docx.enum.text import WD_ALIGN_PARAGRAPH
    from docx.enum.table import WD_TABLE_ALIGNMENT
    from docx.oxml.ns import qn
except ImportError:
    print("ERROR: python-docx not installed. Run: pip install python-docx")
    sys.exit(1)

try:
    from PIL import Image as PILImage
    HAS_PIL = True
except ImportError:
    HAS_PIL = False

OUTPUT_DIR = Path(__file__).parent / "output_docx"


def _create_test_png(width=120, height=80, color=(70, 130, 180)):
    """Create a minimal PNG in memory for embedding."""
    if HAS_PIL:
        img = PILImage.new("RGB", (width, height), color)
        buf = io.BytesIO()
        img.save(buf, format="PNG")
        buf.seek(0)
        return buf
    # Fallback: build a minimal 1x1 PNG manually
    return _minimal_png(color)


def _minimal_png(color):
    """Build a minimal valid 1×1 PNG."""
    import zlib
    buf = io.BytesIO()
    buf.write(b"\x89PNG\r\n\x1a\n")
    # IHDR
    ihdr_data = struct.pack(">IIBBBBB", 1, 1, 8, 2, 0, 0, 0)
    _write_chunk(buf, b"IHDR", ihdr_data)
    # IDAT
    raw = b"\x00" + bytes(color)
    compressed = zlib.compress(raw)
    _write_chunk(buf, b"IDAT", compressed)
    # IEND
    _write_chunk(buf, b"IEND", b"")
    buf.seek(0)
    return buf


def _write_chunk(buf, chunk_type, data):
    import zlib
    buf.write(struct.pack(">I", len(data)))
    buf.write(chunk_type)
    buf.write(data)
    buf.write(struct.pack(">I", zlib.crc32(chunk_type + data) & 0xFFFFFFFF))


# ── Classic docx generators (1–30) ──────────────────────────────────────


def classic01_single_paragraph(path):
    """Single plain-text paragraph."""
    doc = Document()
    doc.add_paragraph("Hello, World! This is a simple single paragraph document created for benchmarking MiniPdf DOCX-to-PDF conversion.")
    doc.save(path)


def classic02_multiple_paragraphs(path):
    """Multiple plain-text paragraphs with implicit spacing."""
    doc = Document()
    for i in range(1, 6):
        doc.add_paragraph(f"This is paragraph {i}. It contains some sample text to test how MiniPdf handles multiple consecutive paragraphs with default spacing.")
    doc.save(path)


def classic03_headings(path):
    """Heading levels 1 through 4."""
    doc = Document()
    doc.add_heading("Heading Level 1", level=1)
    doc.add_paragraph("Content under heading 1.")
    doc.add_heading("Heading Level 2", level=2)
    doc.add_paragraph("Content under heading 2.")
    doc.add_heading("Heading Level 3", level=3)
    doc.add_paragraph("Content under heading 3.")
    doc.add_heading("Heading Level 4", level=4)
    doc.add_paragraph("Content under heading 4.")
    doc.save(path)


def classic04_bold_italic(path):
    """Bold, italic, and bold-italic text."""
    doc = Document()
    p = doc.add_paragraph()
    p.add_run("Normal text. ")
    run_b = p.add_run("Bold text. ")
    run_b.bold = True
    run_i = p.add_run("Italic text. ")
    run_i.italic = True
    run_bi = p.add_run("Bold and italic text.")
    run_bi.bold = True
    run_bi.italic = True
    doc.save(path)


def classic05_font_sizes(path):
    """Various font sizes in one document."""
    doc = Document()
    for size in [8, 10, 12, 14, 18, 24, 36]:
        p = doc.add_paragraph()
        run = p.add_run(f"This text is {size}pt.")
        run.font.size = Pt(size)
    doc.save(path)


def classic06_font_colors(path):
    """Colored text runs."""
    doc = Document()
    colors = [
        ("Red text", RGBColor(255, 0, 0)),
        ("Green text", RGBColor(0, 128, 0)),
        ("Blue text", RGBColor(0, 0, 255)),
        ("Orange text", RGBColor(255, 165, 0)),
        ("Purple text", RGBColor(128, 0, 128)),
    ]
    for text, color in colors:
        p = doc.add_paragraph()
        run = p.add_run(text)
        run.font.color.rgb = color
    doc.save(path)


def classic07_alignment(path):
    """Paragraph alignment: left, center, right, justify."""
    doc = Document()
    lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."

    p = doc.add_paragraph(lorem)
    p.alignment = WD_ALIGN_PARAGRAPH.LEFT
    p_run = p.runs[0] if p.runs else p.add_run("")
    doc.add_paragraph()

    p2 = doc.add_paragraph(lorem)
    p2.alignment = WD_ALIGN_PARAGRAPH.CENTER

    p3 = doc.add_paragraph(lorem)
    p3.alignment = WD_ALIGN_PARAGRAPH.RIGHT

    p4 = doc.add_paragraph(lorem)
    p4.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    doc.save(path)


def classic08_bullet_list(path):
    """Bullet (unordered) list."""
    doc = Document()
    doc.add_heading("Shopping List", level=2)
    items = ["Apples", "Bananas", "Cherries", "Dates", "Elderberries"]
    for item in items:
        doc.add_paragraph(item, style="List Bullet")
    doc.save(path)


def classic09_numbered_list(path):
    """Numbered (ordered) list."""
    doc = Document()
    doc.add_heading("Steps to Success", level=2)
    steps = ["Define the goal", "Research the topic", "Create a plan", "Execute the plan", "Review results"]
    for step in steps:
        doc.add_paragraph(step, style="List Number")
    doc.save(path)


def classic10_simple_table(path):
    """Simple 3x4 table with header row."""
    doc = Document()
    doc.add_heading("Employee Directory", level=2)
    table = doc.add_table(rows=4, cols=3)
    table.style = "Table Grid"
    headers = ["Name", "Department", "Email"]
    for i, h in enumerate(headers):
        table.rows[0].cells[i].text = h
    data = [
        ["Alice Johnson", "Engineering", "alice@example.com"],
        ["Bob Smith", "Marketing", "bob@example.com"],
        ["Carol Williams", "Finance", "carol@example.com"],
    ]
    for ri, row_data in enumerate(data):
        for ci, val in enumerate(row_data):
            table.rows[ri + 1].cells[ci].text = val
    doc.save(path)


def classic11_table_with_shading(path):
    """Table with alternating row shading."""
    doc = Document()
    doc.add_heading("Quarterly Sales", level=2)
    table = doc.add_table(rows=5, cols=4)
    table.style = "Table Grid"
    headers = ["Quarter", "Revenue", "Expenses", "Profit"]
    for i, h in enumerate(headers):
        cell = table.rows[0].cells[i]
        cell.text = h
        _set_cell_shading(cell, "4472C4")
        for run in cell.paragraphs[0].runs:
            run.font.color.rgb = RGBColor(255, 255, 255)
            run.bold = True
    data = [
        ["Q1 2025", "$120,000", "$80,000", "$40,000"],
        ["Q2 2025", "$135,000", "$85,000", "$50,000"],
        ["Q3 2025", "$150,000", "$90,000", "$60,000"],
        ["Q4 2025", "$160,000", "$95,000", "$65,000"],
    ]
    for ri, row_data in enumerate(data):
        for ci, val in enumerate(row_data):
            cell = table.rows[ri + 1].cells[ci]
            cell.text = val
            if ri % 2 == 0:
                _set_cell_shading(cell, "D9E2F3")
    doc.save(path)


def classic12_merged_cells_table(path):
    """Table with merged cells."""
    doc = Document()
    doc.add_heading("Schedule", level=2)
    table = doc.add_table(rows=4, cols=3)
    table.style = "Table Grid"
    table.rows[0].cells[0].text = "Time"
    table.rows[0].cells[1].text = "Monday"
    table.rows[0].cells[2].text = "Tuesday"
    table.rows[1].cells[0].text = "9:00 AM"
    merged = table.rows[1].cells[1].merge(table.rows[1].cells[2])
    merged.text = "Team Meeting"
    table.rows[2].cells[0].text = "10:00 AM"
    table.rows[2].cells[1].text = "Code Review"
    table.rows[2].cells[2].text = "Design Review"
    table.rows[3].cells[0].text = "2:00 PM"
    table.rows[3].cells[1].text = "Sprint Planning"
    table.rows[3].cells[2].text = "Retrospective"
    doc.save(path)


def classic13_long_document(path):
    """A longer document that spans multiple pages."""
    doc = Document()
    doc.add_heading("Project Report", level=1)
    doc.add_paragraph("This document is designed to span multiple pages to test pagination in MiniPdf.")
    for i in range(1, 16):
        doc.add_heading(f"Section {i}", level=2)
        doc.add_paragraph(
            f"This is section {i} of the report. It contains detailed analysis of the topic at hand. "
            "The quick brown fox jumps over the lazy dog. Pack my box with five dozen liquor jugs. "
            "How vexingly quick daft zebras jump. The five boxing wizards jump quickly. "
            "Sphinx of black quartz, judge my vow." * 2
        )
    doc.save(path)


def classic14_mixed_content(path):
    """A document with headings, paragraphs, and a table mixed together."""
    doc = Document()
    doc.add_heading("Monthly Report", level=1)
    doc.add_paragraph("This report summarizes the key metrics for the month of January 2026.")

    doc.add_heading("Revenue Summary", level=2)
    table = doc.add_table(rows=4, cols=2)
    table.style = "Table Grid"
    table.rows[0].cells[0].text = "Category"
    table.rows[0].cells[1].text = "Amount"
    for i, (cat, amt) in enumerate([("Product Sales", "$85,000"), ("Services", "$42,000"), ("Subscriptions", "$28,000")]):
        table.rows[i + 1].cells[0].text = cat
        table.rows[i + 1].cells[1].text = amt

    doc.add_heading("Key Observations", level=2)
    doc.add_paragraph("Product sales increased by 15% compared to the previous quarter.")
    doc.add_paragraph("Service revenue remained stable with a slight upward trend.")

    doc.add_heading("Action Items", level=2)
    for item in ["Expand marketing campaign", "Hire two additional engineers", "Launch new subscription tier"]:
        doc.add_paragraph(item, style="List Bullet")
    doc.save(path)


def classic15_indentation(path):
    """Paragraphs with various indentation levels."""
    doc = Document()
    doc.add_heading("Indentation Test", level=2)
    levels = [0, 36, 72, 108, 144]
    for pts in levels:
        p = doc.add_paragraph(f"This paragraph is indented by {pts} points from the left margin.")
        p.paragraph_format.left_indent = Pt(pts)
    # First-line indent
    p2 = doc.add_paragraph("This paragraph has a first-line indent of 36 points. The remaining lines wrap normally back to the left margin.")
    p2.paragraph_format.first_line_indent = Pt(36)
    doc.save(path)


def classic16_line_spacing(path):
    """Different line spacing options."""
    doc = Document()
    doc.add_heading("Line Spacing Test", level=2)
    text = "The quick brown fox jumps over the lazy dog. Pack my box with five dozen liquor jugs. How vexingly quick daft zebras jump."
    from docx.shared import Pt as PtShared
    for spacing_val, label in [(1.0, "Single"), (1.5, "1.5 Lines"), (2.0, "Double")]:
        doc.add_paragraph(f"{label} spacing:")
        p = doc.add_paragraph(text)
        p.paragraph_format.line_spacing = spacing_val
        doc.add_paragraph()
    doc.save(path)


def classic17_page_break(path):
    """Document with explicit page breaks."""
    doc = Document()
    doc.add_heading("Page 1", level=1)
    doc.add_paragraph("Content on the first page.")
    doc.add_page_break()
    doc.add_heading("Page 2", level=1)
    doc.add_paragraph("Content on the second page after a page break.")
    doc.add_page_break()
    doc.add_heading("Page 3", level=1)
    doc.add_paragraph("Content on the third page.")
    doc.save(path)


def classic18_embedded_image(path):
    """Document with an embedded PNG image."""
    doc = Document()
    doc.add_heading("Image Test", level=2)
    doc.add_paragraph("Below is an embedded test image:")
    img_buf = _create_test_png(200, 100, (70, 130, 180))
    doc.add_picture(img_buf, width=Inches(3))
    doc.add_paragraph("The image above should be a blue rectangle.")
    doc.save(path)


def classic19_multiple_images(path):
    """Multiple images at different sizes."""
    doc = Document()
    doc.add_heading("Multiple Images", level=2)
    colors = [(220, 50, 50), (50, 180, 50), (50, 50, 220)]
    names = ["Red", "Green", "Blue"]
    for color, name in zip(colors, names):
        doc.add_paragraph(f"{name} image:")
        img_buf = _create_test_png(160, 80, color)
        doc.add_picture(img_buf, width=Inches(2.5))
    doc.save(path)


def classic20_table_with_many_rows(path):
    """Large table with 20 data rows."""
    doc = Document()
    doc.add_heading("Product Catalog", level=2)
    table = doc.add_table(rows=21, cols=4)
    table.style = "Table Grid"
    headers = ["ID", "Product", "Category", "Price"]
    for i, h in enumerate(headers):
        table.rows[0].cells[i].text = h
    products = [
        ("Laptop", "Electronics", "$999"),
        ("Mouse", "Accessories", "$29"),
        ("Keyboard", "Accessories", "$59"),
        ("Monitor", "Electronics", "$349"),
        ("Headphones", "Audio", "$149"),
        ("Webcam", "Electronics", "$79"),
        ("USB Hub", "Accessories", "$25"),
        ("Desk Lamp", "Office", "$45"),
        ("Chair", "Furniture", "$299"),
        ("Standing Desk", "Furniture", "$599"),
        ("Printer", "Electronics", "$199"),
        ("Scanner", "Electronics", "$129"),
        ("Router", "Networking", "$89"),
        ("Cable Kit", "Accessories", "$19"),
        ("Mousepad", "Accessories", "$15"),
        ("Surge Protector", "Electronics", "$35"),
        ("External SSD", "Storage", "$109"),
        ("Flash Drive", "Storage", "$12"),
        ("Drawing Tablet", "Electronics", "$249"),
        ("Microphone", "Audio", "$179"),
    ]
    for ri, (prod, cat, price) in enumerate(products):
        table.rows[ri + 1].cells[0].text = str(ri + 1)
        table.rows[ri + 1].cells[1].text = prod
        table.rows[ri + 1].cells[2].text = cat
        table.rows[ri + 1].cells[3].text = price
    doc.save(path)


def classic21_nested_lists(path):
    """Nested bullet list (simulated with indentation)."""
    doc = Document()
    doc.add_heading("Project Structure", level=2)
    items = [
        (0, "src/"),
        (1, "MiniPdf/"),
        (2, "MiniPdf.cs"),
        (2, "PdfDocument.cs"),
        (2, "PdfWriter.cs"),
        (1, "MiniPdf.Tests/"),
        (2, "DocxToPdfConverterTests.cs"),
        (0, "scripts/"),
        (1, "Run-Benchmark.ps1"),
        (0, "README.md"),
    ]
    for level, text in items:
        p = doc.add_paragraph(text, style="List Bullet")
        p.paragraph_format.left_indent = Pt(18 * level)
    doc.save(path)


def classic22_horizontal_rule(path):
    """Document with horizontal line separators (simulated)."""
    doc = Document()
    doc.add_heading("Section A", level=2)
    doc.add_paragraph("Content for section A goes here with enough text to see the layout.")
    _add_horizontal_rule(doc)
    doc.add_heading("Section B", level=2)
    doc.add_paragraph("Content for section B goes here below the horizontal divider.")
    _add_horizontal_rule(doc)
    doc.add_heading("Section C", level=2)
    doc.add_paragraph("Final section content.")
    doc.save(path)


def classic23_mixed_formatting_runs(path):
    """Single paragraph with many formatting runs."""
    doc = Document()
    doc.add_heading("Mixed Formatting", level=2)
    p = doc.add_paragraph()
    p.add_run("Normal, ")
    r1 = p.add_run("BOLD, ")
    r1.bold = True
    r2 = p.add_run("italic, ")
    r2.italic = True
    r3 = p.add_run("large, ")
    r3.font.size = Pt(18)
    r4 = p.add_run("small, ")
    r4.font.size = Pt(8)
    r5 = p.add_run("RED, ")
    r5.font.color.rgb = RGBColor(255, 0, 0)
    r6 = p.add_run("underlined.")
    r6.underline = True
    doc.save(path)


def classic24_two_column_table_layout(path):
    """Two-column layout using a borderless table."""
    doc = Document()
    doc.add_heading("Two-Column Layout", level=2)
    table = doc.add_table(rows=1, cols=2)
    left_cell = table.rows[0].cells[0]
    right_cell = table.rows[0].cells[1]
    left_cell.text = (
        "Left column content. This is the first column of a two-column layout. "
        "It demonstrates how tables can be used for text layout purposes."
    )
    right_cell.text = (
        "Right column content. This is the second column. "
        "Both columns should render side-by-side in the PDF output."
    )
    doc.save(path)


def classic25_title_and_subtitle(path):
    """Title page with title and subtitle styles."""
    doc = Document()
    doc.add_paragraph("MiniPdf Benchmark Report", style="Title")
    doc.add_paragraph("Automated DOCX-to-PDF Conversion Quality Assessment", style="Subtitle")
    doc.add_paragraph()
    doc.add_paragraph("Prepared by: MiniPdf Team")
    doc.add_paragraph("Date: March 2026")
    doc.add_page_break()
    doc.add_heading("Introduction", level=1)
    doc.add_paragraph("This document tests the Title and Subtitle styles in MiniPdf conversion.")
    doc.save(path)


def classic26_table_alignment(path):
    """Table with aligned cell content (left, center, right)."""
    doc = Document()
    doc.add_heading("Cell Alignment Test", level=2)
    table = doc.add_table(rows=4, cols=3)
    table.style = "Table Grid"
    headers = [("Left", WD_ALIGN_PARAGRAPH.LEFT),
               ("Center", WD_ALIGN_PARAGRAPH.CENTER),
               ("Right", WD_ALIGN_PARAGRAPH.RIGHT)]
    for ci, (h, align) in enumerate(headers):
        cell = table.rows[0].cells[ci]
        cell.text = h
        cell.paragraphs[0].alignment = align
        for run in cell.paragraphs[0].runs:
            run.bold = True
    data = [
        ["Alice", "Engineering", "$95,000"],
        ["Bob", "Marketing", "$82,000"],
        ["Carol", "Finance", "$88,000"],
    ]
    for ri, row_data in enumerate(data):
        for ci, val in enumerate(row_data):
            cell = table.rows[ri + 1].cells[ci]
            cell.text = val
            cell.paragraphs[0].alignment = headers[ci][1]
    doc.save(path)


def classic27_long_paragraph_wrapping(path):
    """Very long paragraph to test word wrapping."""
    doc = Document()
    doc.add_heading("Word Wrapping Test", level=2)
    long_text = (
        "This is a very long paragraph designed to test how MiniPdf handles word wrapping across "
        "line boundaries. The text should flow naturally from one line to the next without any "
        "awkward breaks or overflow. " * 10
    )
    doc.add_paragraph(long_text)
    doc.save(path)


def classic28_special_characters(path):
    """Document with special characters and symbols."""
    doc = Document()
    doc.add_heading("Special Characters", level=2)
    doc.add_paragraph("Ampersand: &, Less-than: <, Greater-than: >, Quotes: \" '")
    doc.add_paragraph("Copyright: \u00a9, Registered: \u00ae, Trademark: \u2122")
    doc.add_paragraph("Em-dash: \u2014, En-dash: \u2013, Ellipsis: \u2026")
    doc.add_paragraph("Currency: $ \u20ac \u00a3 \u00a5")
    doc.add_paragraph("Math: \u00b1 \u00d7 \u00f7 \u2260 \u2264 \u2265 \u221e")
    doc.save(path)


def classic29_table_with_image(path):
    """Table with an image in one cell."""
    doc = Document()
    doc.add_heading("Product Card", level=2)
    table = doc.add_table(rows=2, cols=2)
    table.style = "Table Grid"
    table.rows[0].cells[0].text = "Product"
    table.rows[0].cells[1].text = "Description"
    # Add image to first column of second row
    img_buf = _create_test_png(100, 60, (34, 139, 34))
    table.rows[1].cells[0].paragraphs[0].add_run().add_picture(img_buf, width=Inches(1.5))
    table.rows[1].cells[1].text = "MiniPdf Widget - A compact, efficient tool for PDF conversion. Lightweight and dependency-free."
    doc.save(path)


def classic30_comprehensive_report(path):
    """A comprehensive document combining many features."""
    doc = Document()
    # Title
    doc.add_paragraph("Annual Technology Report 2026", style="Title")
    doc.add_paragraph("A Comprehensive Overview", style="Subtitle")
    doc.add_page_break()

    # Table of contents placeholder
    doc.add_heading("Table of Contents", level=1)
    for i, title in enumerate(["Executive Summary", "Market Analysis", "Technology Trends", "Financial Overview", "Recommendations"], 1):
        doc.add_paragraph(f"{i}. {title}")
    doc.add_page_break()

    # Executive Summary
    doc.add_heading("1. Executive Summary", level=1)
    doc.add_paragraph(
        "This report provides a comprehensive analysis of the technology landscape in 2026. "
        "Key findings include continued growth in AI adoption, increased focus on sustainability, "
        "and emerging trends in quantum computing."
    )

    # Market Analysis with table
    doc.add_heading("2. Market Analysis", level=1)
    doc.add_paragraph("The following table summarizes market share across key sectors:")
    table = doc.add_table(rows=5, cols=3)
    table.style = "Table Grid"
    for i, h in enumerate(["Sector", "Market Share", "Growth"]):
        cell = table.rows[0].cells[i]
        cell.text = h
        _set_cell_shading(cell, "2F5496")
        for run in cell.paragraphs[0].runs:
            run.font.color.rgb = RGBColor(255, 255, 255)
            run.bold = True
    sectors = [
        ("Cloud Computing", "34%", "+12%"),
        ("AI/ML", "28%", "+23%"),
        ("Cybersecurity", "22%", "+18%"),
        ("IoT", "16%", "+8%"),
    ]
    for ri, (sector, share, growth) in enumerate(sectors):
        table.rows[ri + 1].cells[0].text = sector
        table.rows[ri + 1].cells[1].text = share
        table.rows[ri + 1].cells[2].text = growth

    # Technology Trends
    doc.add_heading("3. Technology Trends", level=1)
    doc.add_paragraph("Key trends identified:")
    for trend in ["Generative AI integration in enterprise software",
                  "Edge computing for real-time processing",
                  "Green technology and sustainable computing",
                  "Zero-trust security architectures",
                  "Low-code/no-code platform expansion"]:
        doc.add_paragraph(trend, style="List Bullet")

    # Image section
    doc.add_heading("4. Visual Summary", level=2)
    doc.add_paragraph("Growth indicator chart (placeholder):")
    img_buf = _create_test_png(300, 150, (46, 84, 150))
    doc.add_picture(img_buf, width=Inches(4))

    # Recommendations
    doc.add_heading("5. Recommendations", level=1)
    for i, rec in enumerate(["Invest in AI-driven automation tools",
                              "Prioritize cloud-native architectures",
                              "Strengthen cybersecurity posture",
                              "Explore quantum computing partnerships"], 1):
        doc.add_paragraph(rec, style="List Number")

    doc.save(path)


# ── Classic docx generators (31–60) ─────────────────────────────────────


def classic31_strikethrough_text(path):
    """Text with strikethrough and double-strikethrough formatting."""
    doc = Document()
    doc.add_heading("Strikethrough Test", level=2)
    p = doc.add_paragraph()
    p.add_run("Normal text. ")
    r1 = p.add_run("Single strikethrough. ")
    r1.font.strike = True
    r2 = p.add_run("Double strikethrough. ")
    r2.font.double_strike = True
    p.add_run("Back to normal.")

    doc.add_paragraph()
    p2 = doc.add_paragraph("Task list:")
    tasks = [
        ("Buy groceries", True),
        ("Write report", True),
        ("Schedule meeting", False),
        ("Review PR", False),
        ("Deploy to production", True),
    ]
    for task, done in tasks:
        tp = doc.add_paragraph()
        r = tp.add_run(task)
        if done:
            r.font.strike = True
    doc.save(path)


def classic32_superscript_subscript(path):
    """Superscript and subscript text."""
    doc = Document()
    doc.add_heading("Superscript and Subscript", level=2)

    p1 = doc.add_paragraph()
    p1.add_run("Einstein's equation: E = mc")
    r_sup = p1.add_run("2")
    r_sup.font.superscript = True

    p2 = doc.add_paragraph()
    p2.add_run("Water: H")
    r_sub1 = p2.add_run("2")
    r_sub1.font.subscript = True
    p2.add_run("O")

    p3 = doc.add_paragraph()
    p3.add_run("Carbon dioxide: CO")
    r_sub2 = p3.add_run("2")
    r_sub2.font.subscript = True

    p4 = doc.add_paragraph()
    p4.add_run("Footnote reference")
    r_fn = p4.add_run("1")
    r_fn.font.superscript = True

    p5 = doc.add_paragraph()
    p5.add_run("x")
    r_n = p5.add_run("n")
    r_n.font.superscript = True
    p5.add_run(" + y")
    r_n2 = p5.add_run("n")
    r_n2.font.superscript = True
    p5.add_run(" = z")
    r_n3 = p5.add_run("n")
    r_n3.font.superscript = True
    doc.save(path)


def classic33_highlighted_text(path):
    """Text with highlight colors."""
    doc = Document()
    doc.add_heading("Highlighted Text", level=2)
    from docx.enum.text import WD_COLOR_INDEX
    highlights = [
        ("Yellow highlight", WD_COLOR_INDEX.YELLOW),
        ("Green highlight", WD_COLOR_INDEX.GREEN),
        ("Turquoise highlight", WD_COLOR_INDEX.TURQUOISE),
        ("Pink highlight", WD_COLOR_INDEX.PINK),
        ("Red highlight", WD_COLOR_INDEX.RED),
        ("Blue highlight", WD_COLOR_INDEX.BLUE),
    ]
    for text, hl_color in highlights:
        p = doc.add_paragraph()
        r = p.add_run(text)
        r.font.highlight_color = hl_color
    doc.save(path)


def classic34_paragraph_borders(path):
    """Paragraphs with border boxes."""
    doc = Document()
    doc.add_heading("Bordered Paragraphs", level=2)

    # Create a paragraph with a box border
    for label, color in [("Important Notice", "FF0000"), ("Info Box", "4472C4"), ("Success", "00B050")]:
        p = doc.add_paragraph(label + ": This paragraph has a colored border around it.")
        pPr = p._element.get_or_add_pPr()
        pBdr = pPr.makeelement(qn("w:pBdr"), {})
        for side in ["top", "left", "bottom", "right"]:
            bdr = pBdr.makeelement(qn(f"w:{side}"), {
                qn("w:val"): "single",
                qn("w:sz"): "8",
                qn("w:space"): "4",
                qn("w:color"): color,
            })
            pBdr.append(bdr)
        pPr.append(pBdr)
        doc.add_paragraph()
    doc.save(path)


def classic35_tab_stops(path):
    """Paragraphs with custom tab stops."""
    doc = Document()
    doc.add_heading("Tab Stop Alignment", level=2)

    # Use tab characters with right-aligned tab stops to simulate a TOC
    entries = [
        ("Chapter 1: Introduction", "1"),
        ("Chapter 2: Getting Started", "5"),
        ("Chapter 3: Advanced Topics", "15"),
        ("Chapter 4: Best Practices", "28"),
        ("Chapter 5: Conclusion", "35"),
    ]
    for title, page in entries:
        p = doc.add_paragraph()
        p.add_run(title)
        p.add_run("\t")
        p.add_run(page)
        # Add a right-aligned tab stop
        pPr = p._element.get_or_add_pPr()
        tabs = pPr.makeelement(qn("w:tabs"), {})
        tab = tabs.makeelement(qn("w:tab"), {
            qn("w:val"): "right",
            qn("w:leader"): "dot",
            qn("w:pos"): "9360",  # ~6.5 inches in twips
        })
        tabs.append(tab)
        pPr.append(tabs)
    doc.save(path)


def classic36_wide_table(path):
    """Table with many columns (8 columns)."""
    doc = Document()
    doc.add_heading("Weekly Schedule", level=2)
    table = doc.add_table(rows=6, cols=8)
    table.style = "Table Grid"
    headers = ["Time", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"]
    for i, h in enumerate(headers):
        cell = table.rows[0].cells[i]
        cell.text = h
        _set_cell_shading(cell, "2F5496")
        for run in cell.paragraphs[0].runs:
            run.font.color.rgb = RGBColor(255, 255, 255)
            run.bold = True
    schedule = [
        ["9:00", "Math", "English", "Science", "Math", "Art", "Free", "Free"],
        ["10:00", "English", "Math", "English", "Science", "Music", "Sports", "Free"],
        ["11:00", "Science", "Science", "Math", "English", "PE", "Free", "Free"],
        ["13:00", "History", "Art", "History", "Music", "Lab", "Free", "Free"],
        ["14:00", "PE", "Music", "Art", "History", "Free", "Free", "Free"],
    ]
    for ri, row_data in enumerate(schedule):
        for ci, val in enumerate(row_data):
            table.rows[ri + 1].cells[ci].text = val
    doc.save(path)


def classic37_nested_table(path):
    """Table inside a table cell."""
    doc = Document()
    doc.add_heading("Nested Table Layout", level=2)
    outer = doc.add_table(rows=2, cols=2)
    outer.style = "Table Grid"
    outer.rows[0].cells[0].text = "Section A"
    outer.rows[0].cells[1].text = "Section B"

    # Nested table in bottom-left cell
    cell = outer.rows[1].cells[0]
    cell.paragraphs[0].text = "Details:"
    inner = cell.add_table(rows=3, cols=2)
    inner.style = "Table Grid"
    inner.rows[0].cells[0].text = "Item"
    inner.rows[0].cells[1].text = "Qty"
    inner.rows[1].cells[0].text = "Widget"
    inner.rows[1].cells[1].text = "10"
    inner.rows[2].cells[0].text = "Gadget"
    inner.rows[2].cells[1].text = "5"

    outer.rows[1].cells[1].text = "This cell contains plain text while the adjacent cell has a nested table."
    doc.save(path)


def classic38_table_column_widths(path):
    """Table with explicit column widths."""
    doc = Document()
    doc.add_heading("Custom Column Widths", level=2)
    table = doc.add_table(rows=5, cols=4)
    table.style = "Table Grid"
    widths = [Inches(0.5), Inches(3.0), Inches(1.5), Inches(1.5)]
    for row in table.rows:
        for ci, w in enumerate(widths):
            row.cells[ci].width = w
    headers = ["#", "Description", "Category", "Amount"]
    for i, h in enumerate(headers):
        cell = table.rows[0].cells[i]
        cell.text = h
        for run in cell.paragraphs[0].runs:
            run.bold = True
    data = [
        ["1", "Office supplies and stationery", "Operations", "$245.00"],
        ["2", "Cloud hosting monthly fee", "Technology", "$1,200.00"],
        ["3", "Team lunch and catering", "Meals", "$380.00"],
        ["4", "Conference registration", "Travel", "$599.00"],
    ]
    for ri, row_data in enumerate(data):
        for ci, val in enumerate(row_data):
            table.rows[ri + 1].cells[ci].text = val
    doc.save(path)


def classic39_financial_report(path):
    """Financial report with colored variance values."""
    doc = Document()
    doc.add_heading("Financial Summary Q4 2025", level=1)
    doc.add_paragraph("All amounts in USD thousands.")

    table = doc.add_table(rows=7, cols=4)
    table.style = "Table Grid"
    headers = ["Line Item", "Budget", "Actual", "Variance"]
    for i, h in enumerate(headers):
        cell = table.rows[0].cells[i]
        cell.text = h
        _set_cell_shading(cell, "2F5496")
        for run in cell.paragraphs[0].runs:
            run.font.color.rgb = RGBColor(255, 255, 255)
            run.bold = True

    data = [
        ("Revenue", "$500", "$520", "+$20"),
        ("COGS", "$200", "$210", "-$10"),
        ("Gross Profit", "$300", "$310", "+$10"),
        ("Operating Exp", "$150", "$140", "+$10"),
        ("Net Income", "$150", "$170", "+$20"),
        ("EPS", "$1.50", "$1.70", "+$0.20"),
    ]
    for ri, (item, budget, actual, variance) in enumerate(data):
        row = table.rows[ri + 1]
        row.cells[0].text = item
        row.cells[1].text = budget
        row.cells[2].text = actual
        p = row.cells[3].paragraphs[0]
        r = p.add_run(variance)
        if variance.startswith("+"):
            r.font.color.rgb = RGBColor(0, 128, 0)
        else:
            r.font.color.rgb = RGBColor(255, 0, 0)

    doc.add_paragraph()
    doc.add_paragraph("Note: Positive variance indicates favorable performance.")
    doc.save(path)


def classic40_resume(path):
    """Simple resume/CV document."""
    doc = Document()
    doc.add_paragraph("JOHN DOE", style="Title")
    p_contact = doc.add_paragraph()
    p_contact.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_contact.add_run("john.doe@email.com | +1-555-0100 | New York, NY")

    _add_horizontal_rule(doc)

    doc.add_heading("Professional Summary", level=2)
    doc.add_paragraph(
        "Experienced software engineer with 8+ years of expertise in building scalable "
        "web applications and distributed systems. Proficient in C#, Python, and JavaScript."
    )

    doc.add_heading("Experience", level=2)
    p_title = doc.add_paragraph()
    r_title = p_title.add_run("Senior Software Engineer")
    r_title.bold = True
    p_title.add_run(" - Tech Corp Inc.")
    p_date = doc.add_paragraph()
    r_date = p_date.add_run("January 2020 - Present")
    r_date.italic = True
    for item in ["Led team of 5 engineers on microservices migration",
                 "Reduced API latency by 40% through caching optimization",
                 "Implemented CI/CD pipeline using GitHub Actions"]:
        doc.add_paragraph(item, style="List Bullet")

    p_title2 = doc.add_paragraph()
    r_title2 = p_title2.add_run("Software Engineer")
    r_title2.bold = True
    p_title2.add_run(" - StartupXYZ")
    p_date2 = doc.add_paragraph()
    r_date2 = p_date2.add_run("June 2016 - December 2019")
    r_date2.italic = True
    for item in ["Built RESTful APIs serving 1M+ daily requests",
                 "Developed real-time notification system using WebSockets"]:
        doc.add_paragraph(item, style="List Bullet")

    doc.add_heading("Education", level=2)
    p_edu = doc.add_paragraph()
    r_edu = p_edu.add_run("B.S. Computer Science")
    r_edu.bold = True
    p_edu.add_run(" - State University, 2016")

    doc.add_heading("Skills", level=2)
    doc.add_paragraph("C#, Python, JavaScript, TypeScript, SQL, Docker, Kubernetes, Azure, AWS")
    doc.save(path)


def classic41_business_letter(path):
    """Formal business letter layout."""
    doc = Document()
    # Sender info - right aligned
    for line in ["ACME Corporation", "123 Business Ave, Suite 500", "New York, NY 10001"]:
        p = doc.add_paragraph(line)
        p.alignment = WD_ALIGN_PARAGRAPH.RIGHT

    doc.add_paragraph()
    doc.add_paragraph("March 1, 2026")
    doc.add_paragraph()

    # Recipient
    for line in ["Mr. James Wilson", "Widget Industries", "456 Commerce St", "San Francisco, CA 94102"]:
        doc.add_paragraph(line)
    doc.add_paragraph()

    doc.add_paragraph("Dear Mr. Wilson,")
    doc.add_paragraph()

    doc.add_paragraph(
        "Thank you for your interest in our products. We are pleased to inform you that "
        "your order #ORD-2026-0315 has been processed and is scheduled for delivery by "
        "March 15, 2026."
    )
    doc.add_paragraph(
        "Please find enclosed the detailed invoice and shipping confirmation. If you have "
        "any questions regarding your order, please do not hesitate to contact our customer "
        "service team at support@acme.com or call us at +1-555-0200."
    )
    doc.add_paragraph(
        "We value your business and look forward to a continued partnership."
    )

    doc.add_paragraph()
    doc.add_paragraph("Sincerely,")
    doc.add_paragraph()
    p_sig = doc.add_paragraph()
    r_sig = p_sig.add_run("Sarah Johnson")
    r_sig.bold = True
    doc.add_paragraph("Vice President of Sales")
    doc.add_paragraph("ACME Corporation")
    doc.save(path)


def classic42_meeting_minutes(path):
    """Meeting minutes document."""
    doc = Document()
    doc.add_heading("Meeting Minutes", level=1)
    doc.add_paragraph()

    # Meeting details table
    table = doc.add_table(rows=4, cols=2)
    table.style = "Table Grid"
    details = [
        ("Date", "March 3, 2026"),
        ("Time", "10:00 AM - 11:30 AM"),
        ("Location", "Conference Room B"),
        ("Attendees", "Alice, Bob, Carol, David, Eve"),
    ]
    for ri, (label, value) in enumerate(details):
        cell_l = table.rows[ri].cells[0]
        cell_l.text = label
        for run in cell_l.paragraphs[0].runs:
            run.bold = True
        _set_cell_shading(cell_l, "E2EFDA")
        table.rows[ri].cells[1].text = value

    doc.add_paragraph()
    doc.add_heading("Agenda Items", level=2)
    for i, item in enumerate(["Q4 Review", "Budget Planning", "New Hire Onboarding", "Action Items"], 1):
        doc.add_paragraph(f"{i}. {item}")

    doc.add_heading("Discussion Summary", level=2)
    doc.add_paragraph(
        "Alice presented the Q4 results showing a 15% revenue increase. "
        "Bob proposed reallocating 10% of the marketing budget to R&D. "
        "Carol reported that three new engineering positions have been approved."
    )

    doc.add_heading("Action Items", level=2)
    actions = [
        ("Bob", "Submit revised budget proposal", "March 10"),
        ("Carol", "Post job listings for engineering roles", "March 7"),
        ("David", "Prepare onboarding materials", "March 14"),
        ("Eve", "Schedule follow-up meeting", "March 5"),
    ]
    t2 = doc.add_table(rows=len(actions) + 1, cols=3)
    t2.style = "Table Grid"
    for i, h in enumerate(["Owner", "Action", "Due Date"]):
        cell = t2.rows[0].cells[i]
        cell.text = h
        for run in cell.paragraphs[0].runs:
            run.bold = True
    for ri, (owner, action, due) in enumerate(actions):
        t2.rows[ri + 1].cells[0].text = owner
        t2.rows[ri + 1].cells[1].text = action
        t2.rows[ri + 1].cells[2].text = due
    doc.save(path)


def classic43_invoice_document(path):
    """Invoice document with line items and totals."""
    doc = Document()
    # Company header
    p_company = doc.add_paragraph()
    r_company = p_company.add_run("ACME SOLUTIONS INC.")
    r_company.bold = True
    r_company.font.size = Pt(18)
    doc.add_paragraph("789 Tech Boulevard, Austin, TX 78701")
    doc.add_paragraph("Phone: +1-555-0300 | Email: billing@acme-solutions.com")

    _add_horizontal_rule(doc)

    p_inv = doc.add_paragraph()
    r_inv = p_inv.add_run("INVOICE")
    r_inv.bold = True
    r_inv.font.size = Pt(14)
    p_inv.alignment = WD_ALIGN_PARAGRAPH.CENTER

    doc.add_paragraph()
    # Invoice details
    details_table = doc.add_table(rows=3, cols=4)
    details_table.rows[0].cells[0].text = "Invoice #:"
    details_table.rows[0].cells[1].text = "INV-2026-0087"
    details_table.rows[0].cells[2].text = "Date:"
    details_table.rows[0].cells[3].text = "March 1, 2026"
    details_table.rows[1].cells[0].text = "Due Date:"
    details_table.rows[1].cells[1].text = "March 31, 2026"
    details_table.rows[1].cells[2].text = "Terms:"
    details_table.rows[1].cells[3].text = "Net 30"
    details_table.rows[2].cells[0].text = "Bill To:"
    details_table.rows[2].cells[1].text = "Widget Industries"
    details_table.rows[2].cells[2].text = "Ship To:"
    details_table.rows[2].cells[3].text = "Same as billing"

    doc.add_paragraph()

    # Line items
    items_table = doc.add_table(rows=6, cols=5)
    items_table.style = "Table Grid"
    headers = ["Item", "Description", "Qty", "Unit Price", "Total"]
    for i, h in enumerate(headers):
        cell = items_table.rows[0].cells[i]
        cell.text = h
        _set_cell_shading(cell, "2F5496")
        for run in cell.paragraphs[0].runs:
            run.font.color.rgb = RGBColor(255, 255, 255)
            run.bold = True

    line_items = [
        ("SVC-001", "Consulting Services (40 hrs)", "40", "$150.00", "$6,000.00"),
        ("LIC-002", "Enterprise License (Annual)", "5", "$499.00", "$2,495.00"),
        ("HW-003", "Server Hardware", "2", "$2,499.00", "$4,998.00"),
        ("SUP-004", "Premium Support Plan", "1", "$1,800.00", "$1,800.00"),
        ("TRN-005", "On-site Training (2 days)", "1", "$3,000.00", "$3,000.00"),
    ]
    for ri, items in enumerate(line_items):
        for ci, val in enumerate(items):
            items_table.rows[ri + 1].cells[ci].text = val

    doc.add_paragraph()
    # Totals - right aligned
    for label, amount in [("Subtotal:", "$18,293.00"), ("Tax (8.25%):", "$1,509.17"), ("Total Due:", "$19,802.17")]:
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.RIGHT
        p.add_run(f"{label}  ")
        r_amt = p.add_run(amount)
        if label.startswith("Total"):
            r_amt.bold = True
            r_amt.font.size = Pt(14)
    doc.save(path)


def classic44_memo(path):
    """Internal memo document."""
    doc = Document()
    p_memo = doc.add_paragraph()
    r_memo = p_memo.add_run("MEMORANDUM")
    r_memo.bold = True
    r_memo.font.size = Pt(16)
    p_memo.alignment = WD_ALIGN_PARAGRAPH.CENTER

    _add_horizontal_rule(doc)

    fields = [
        ("TO:", "All Department Heads"),
        ("FROM:", "Maria Garcia, CEO"),
        ("DATE:", "March 3, 2026"),
        ("RE:", "Quarterly Performance Review Process Changes")
    ]
    for label, value in fields:
        p = doc.add_paragraph()
        r_label = p.add_run(label + "  ")
        r_label.bold = True
        p.add_run(value)

    _add_horizontal_rule(doc)

    doc.add_paragraph(
        "Effective immediately, we are implementing several changes to our quarterly "
        "performance review process. These changes are designed to streamline evaluation "
        "procedures and provide more actionable feedback to team members."
    )
    doc.add_paragraph()
    doc.add_heading("Key Changes", level=2)
    changes = [
        "Reviews will now be conducted bi-monthly instead of quarterly",
        "Self-assessment forms must be submitted 5 business days before the review",
        "360-degree feedback will be incorporated for all managerial positions",
        "New rating scale: 1-5 (replacing the current A-F system)",
        "All reviews must be completed within a 2-week window",
    ]
    for change in changes:
        doc.add_paragraph(change, style="List Bullet")

    doc.add_paragraph()
    doc.add_paragraph(
        "Please share this information with your teams and direct any questions "
        "to the HR department at hr@company.com."
    )
    doc.save(path)


def classic45_project_plan(path):
    """Project plan with timeline table."""
    doc = Document()
    doc.add_heading("Project Plan: Website Redesign", level=1)
    doc.add_paragraph("Project Manager: Sarah Chen | Start Date: March 2026")

    doc.add_heading("Project Overview", level=2)
    doc.add_paragraph(
        "This project aims to redesign the company website to improve user experience, "
        "modernize the visual design, and optimize for mobile devices."
    )

    doc.add_heading("Timeline", level=2)
    table = doc.add_table(rows=8, cols=5)
    table.style = "Table Grid"
    headers = ["Phase", "Task", "Owner", "Start", "End"]
    for i, h in enumerate(headers):
        cell = table.rows[0].cells[i]
        cell.text = h
        _set_cell_shading(cell, "4472C4")
        for run in cell.paragraphs[0].runs:
            run.font.color.rgb = RGBColor(255, 255, 255)
            run.bold = True

    tasks = [
        ("Discovery", "User research & interviews", "UX Team", "Mar 1", "Mar 14"),
        ("Discovery", "Competitive analysis", "Marketing", "Mar 1", "Mar 7"),
        ("Design", "Wireframes", "Design Team", "Mar 15", "Mar 28"),
        ("Design", "Visual mockups", "Design Team", "Mar 29", "Apr 11"),
        ("Development", "Frontend build", "Dev Team", "Apr 12", "May 9"),
        ("Development", "Backend integration", "Dev Team", "Apr 19", "May 16"),
        ("Launch", "QA testing & deployment", "QA Team", "May 17", "May 30"),
    ]
    for ri, (phase, task, owner, start, end) in enumerate(tasks):
        row = table.rows[ri + 1]
        row.cells[0].text = phase
        row.cells[1].text = task
        row.cells[2].text = owner
        row.cells[3].text = start
        row.cells[4].text = end

    doc.add_heading("Budget", level=2)
    budget_table = doc.add_table(rows=5, cols=2)
    budget_table.style = "Table Grid"
    budget_table.rows[0].cells[0].text = "Category"
    budget_table.rows[0].cells[1].text = "Amount"
    for run in budget_table.rows[0].cells[0].paragraphs[0].runs:
        run.bold = True
    for run in budget_table.rows[0].cells[1].paragraphs[0].runs:
        run.bold = True
    budget = [("Design", "$15,000"), ("Development", "$45,000"), ("QA & Testing", "$8,000"), ("Total", "$68,000")]
    for ri, (cat, amt) in enumerate(budget):
        budget_table.rows[ri + 1].cells[0].text = cat
        budget_table.rows[ri + 1].cells[1].text = amt
        if cat == "Total":
            for cell in budget_table.rows[ri + 1].cells:
                for run in cell.paragraphs[0].runs:
                    run.bold = True
    doc.save(path)


def classic46_comparison_table(path):
    """Product comparison table with checkmarks and crosses."""
    doc = Document()
    doc.add_heading("Product Comparison", level=1)

    table = doc.add_table(rows=9, cols=4)
    table.style = "Table Grid"
    headers = ["Feature", "Basic", "Pro", "Enterprise"]
    for i, h in enumerate(headers):
        cell = table.rows[0].cells[i]
        cell.text = h
        cell.paragraphs[0].alignment = WD_ALIGN_PARAGRAPH.CENTER
        _set_cell_shading(cell, "2F5496")
        for run in cell.paragraphs[0].runs:
            run.font.color.rgb = RGBColor(255, 255, 255)
            run.bold = True

    features = [
        ("Cloud Storage", "5 GB", "50 GB", "Unlimited"),
        ("Users", "1", "10", "Unlimited"),
        ("API Access", "No", "Yes", "Yes"),
        ("Priority Support", "No", "Yes", "Yes"),
        ("Custom Branding", "No", "No", "Yes"),
        ("SLA Guarantee", "No", "99.9%", "99.99%"),
        ("Data Export", "CSV", "CSV, JSON", "All formats"),
        ("Price/month", "$9", "$29", "$99"),
    ]
    for ri, (feature, basic, pro, ent) in enumerate(features):
        row = table.rows[ri + 1]
        row.cells[0].text = feature
        for ci, val in enumerate([basic, pro, ent], 1):
            cell = row.cells[ci]
            cell.text = val
            cell.paragraphs[0].alignment = WD_ALIGN_PARAGRAPH.CENTER
    doc.save(path)


def classic47_data_dictionary(path):
    """Database data dictionary document."""
    doc = Document()
    doc.add_heading("Data Dictionary", level=1)
    doc.add_paragraph("Database: CustomerDB | Version: 2.1 | Last Updated: March 2026")

    tables_spec = [
        ("customers", [
            ("id", "INT", "PRIMARY KEY", "Unique customer identifier"),
            ("name", "VARCHAR(100)", "NOT NULL", "Full name"),
            ("email", "VARCHAR(255)", "UNIQUE", "Email address"),
            ("created_at", "DATETIME", "DEFAULT NOW()", "Account creation date"),
            ("status", "ENUM", "DEFAULT 'active'", "Account status"),
        ]),
        ("orders", [
            ("id", "INT", "PRIMARY KEY", "Order identifier"),
            ("customer_id", "INT", "FOREIGN KEY", "Reference to customers.id"),
            ("total", "DECIMAL(10,2)", "NOT NULL", "Order total amount"),
            ("status", "VARCHAR(20)", "DEFAULT 'pending'", "Order status"),
            ("created_at", "DATETIME", "DEFAULT NOW()", "Order creation date"),
        ]),
    ]

    for table_name, columns in tables_spec:
        doc.add_heading(f"Table: {table_name}", level=2)
        t = doc.add_table(rows=len(columns) + 1, cols=4)
        t.style = "Table Grid"
        for i, h in enumerate(["Column", "Type", "Constraints", "Description"]):
            cell = t.rows[0].cells[i]
            cell.text = h
            _set_cell_shading(cell, "4472C4")
            for run in cell.paragraphs[0].runs:
                run.font.color.rgb = RGBColor(255, 255, 255)
                run.bold = True
        for ri, (col, dtype, constraint, desc) in enumerate(columns):
            row = t.rows[ri + 1]
            row.cells[0].text = col
            row.cells[1].text = dtype
            row.cells[2].text = constraint
            row.cells[3].text = desc
        doc.add_paragraph()
    doc.save(path)


def classic48_multi_level_headings(path):
    """Document with 4 levels of heading depth and content."""
    doc = Document()
    doc.add_heading("Software Architecture Document", level=1)

    for i in range(1, 4):
        doc.add_heading(f"{i}. Module {i}", level=1)
        doc.add_paragraph(f"Module {i} provides core functionality for the system.")

        for j in range(1, 3):
            doc.add_heading(f"{i}.{j} Component {j}", level=2)
            doc.add_paragraph(f"Component {i}.{j} handles specific operations within Module {i}.")

            for k in range(1, 3):
                doc.add_heading(f"{i}.{j}.{k} Sub-component", level=3)
                doc.add_paragraph(f"Detailed description of sub-component {i}.{j}.{k}.")

                doc.add_heading(f"{i}.{j}.{k}.1 Implementation Notes", level=4)
                doc.add_paragraph("Implementation uses factory pattern with dependency injection.")
    doc.save(path)


def classic49_cjk_document(path):
    """Document with Chinese, Japanese, and Korean text."""
    doc = Document()
    doc.add_heading("CJK Text Sample", level=1)

    doc.add_heading("Chinese (Simplified)", level=2)
    doc.add_paragraph("MiniPdf 是一个轻量级的 .NET 库，用于将 Office 文档转换为 PDF 格式。")
    doc.add_paragraph("它不依赖 Microsoft Office，可以在任何平台上运行。")

    doc.add_heading("Chinese (Traditional)", level=2)
    doc.add_paragraph("MiniPdf 是一個輕量級的 .NET 庫，用於將 Office 文檔轉換為 PDF 格式。")

    doc.add_heading("Japanese", level=2)
    doc.add_paragraph("MiniPdf は軽量な .NET ライブラリで、Office ドキュメントを PDF に変換します。")

    doc.add_heading("Korean", level=2)
    doc.add_paragraph("MiniPdf는 Office 문서를 PDF로 변환하는 경량 .NET 라이브러리입니다.")

    doc.add_heading("Mixed CJK Table", level=2)
    table = doc.add_table(rows=5, cols=3)
    table.style = "Table Grid"
    table.rows[0].cells[0].text = "Language"
    table.rows[0].cells[1].text = "Hello"
    table.rows[0].cells[2].text = "Thank You"
    data = [
        ("Chinese", "你好", "谢谢"),
        ("Japanese", "こんにちは", "ありがとう"),
        ("Korean", "안녕하세요", "감사합니다"),
        ("English", "Hello", "Thank you"),
    ]
    for ri, (lang, hello, thanks) in enumerate(data):
        table.rows[ri + 1].cells[0].text = lang
        table.rows[ri + 1].cells[1].text = hello
        table.rows[ri + 1].cells[2].text = thanks
    doc.save(path)


def classic50_long_table_with_formatting(path):
    """Large table with 30 rows and alternating row colors."""
    doc = Document()
    doc.add_heading("Server Inventory Report", level=1)
    table = doc.add_table(rows=31, cols=5)
    table.style = "Table Grid"
    headers = ["Server ID", "Hostname", "IP Address", "OS", "Status"]
    for i, h in enumerate(headers):
        cell = table.rows[0].cells[i]
        cell.text = h
        _set_cell_shading(cell, "2F5496")
        for run in cell.paragraphs[0].runs:
            run.font.color.rgb = RGBColor(255, 255, 255)
            run.bold = True

    oses = ["Ubuntu 22.04", "Windows Server 2022", "RHEL 9", "Debian 12", "CentOS Stream 9"]
    statuses = ["Online", "Online", "Online", "Maintenance", "Online",
                "Online", "Offline", "Online", "Online", "Online"]
    for ri in range(30):
        row = table.rows[ri + 1]
        row.cells[0].text = f"SRV-{ri + 1:03d}"
        row.cells[1].text = f"server-{ri + 1:03d}.local"
        row.cells[2].text = f"10.0.{ri // 256}.{ri % 256 + 1}"
        row.cells[3].text = oses[ri % len(oses)]
        status = statuses[ri % len(statuses)]
        p = row.cells[4].paragraphs[0]
        r = p.add_run(status)
        if status == "Online":
            r.font.color.rgb = RGBColor(0, 128, 0)
        elif status == "Offline":
            r.font.color.rgb = RGBColor(255, 0, 0)
        else:
            r.font.color.rgb = RGBColor(255, 165, 0)
        if ri % 2 == 0:
            for ci in range(5):
                _set_cell_shading(row.cells[ci], "D9E2F3")
    doc.save(path)


def classic51_underline_styles(path):
    """Various underline styles."""
    doc = Document()
    doc.add_heading("Underline Styles", level=2)
    from docx.enum.text import WD_UNDERLINE
    styles = [
        ("Single underline", WD_UNDERLINE.SINGLE),
        ("Double underline", WD_UNDERLINE.DOUBLE),
        ("Thick underline", WD_UNDERLINE.THICK),
        ("Dotted underline", WD_UNDERLINE.DOTTED),
        ("Dash underline", WD_UNDERLINE.DASH),
        ("Wavy underline", WD_UNDERLINE.WAVY),
    ]
    for text, style in styles:
        p = doc.add_paragraph()
        r = p.add_run(text)
        r.underline = style
    doc.save(path)


def classic52_spacing_before_after(path):
    """Paragraphs with explicit before/after spacing."""
    doc = Document()
    doc.add_heading("Paragraph Spacing Test", level=2)

    spacings = [(0, 0), (6, 6), (12, 12), (18, 6), (6, 18), (24, 24)]
    for before, after in spacings:
        p = doc.add_paragraph(f"SpaceBefore={before}pt, SpaceAfter={after}pt. "
                              "Sample text to show paragraph spacing effects.")
        p.paragraph_format.space_before = Pt(before)
        p.paragraph_format.space_after = Pt(after)
    doc.save(path)


def classic53_table_merged_complex(path):
    """Complex table with merged cells in rows and columns."""
    doc = Document()
    doc.add_heading("Course Schedule", level=1)

    table = doc.add_table(rows=6, cols=5)
    table.style = "Table Grid"
    headers = ["", "Period 1", "Period 2", "Period 3", "Period 4"]
    for i, h in enumerate(headers):
        cell = table.rows[0].cells[i]
        cell.text = h
        _set_cell_shading(cell, "4472C4")
        for run in cell.paragraphs[0].runs:
            run.font.color.rgb = RGBColor(255, 255, 255)
            run.bold = True

    days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]
    for ri, day in enumerate(days):
        table.rows[ri + 1].cells[0].text = day
        _set_cell_shading(table.rows[ri + 1].cells[0], "D9E2F3")

    # Fill in schedule
    schedule_data = [
        ["Math", "English", "Science", "Art"],
        ["English", "Math", "History", "PE"],
        ["Science", "Science", "Math", "Music"],
        ["History", "Art", "English", "Lab"],
        ["PE", "Music", "Math", "English"],
    ]
    for ri, row_data in enumerate(schedule_data):
        for ci, val in enumerate(row_data):
            table.rows[ri + 1].cells[ci + 1].text = val

    # Merge Wednesday periods 2-3 for "Science Lab"
    merged = table.rows[3].cells[1].merge(table.rows[3].cells[2])
    merged.text = "Science Lab (Double Period)"
    doc.save(path)


def classic54_multi_font_family(path):
    """Text with different font families."""
    doc = Document()
    doc.add_heading("Font Family Showcase", level=2)
    fonts = [
        ("Arial", "The quick brown fox jumps over the lazy dog."),
        ("Times New Roman", "The quick brown fox jumps over the lazy dog."),
        ("Courier New", "The quick brown fox jumps over the lazy dog."),
        ("Calibri", "The quick brown fox jumps over the lazy dog."),
        ("Verdana", "The quick brown fox jumps over the lazy dog."),
        ("Georgia", "The quick brown fox jumps over the lazy dog."),
    ]
    for font_name, text in fonts:
        p = doc.add_paragraph()
        r = p.add_run(f"{font_name}: {text}")
        r.font.name = font_name
    doc.save(path)


def classic55_background_shading_paragraph(path):
    """Paragraphs with background shading colors."""
    doc = Document()
    doc.add_heading("Paragraph Shading", level=2)

    shadings = [
        ("Light blue background", "D9E2F3"),
        ("Light green background", "E2EFDA"),
        ("Light yellow background", "FFF2CC"),
        ("Light red background", "FCE4EC"),
        ("Light gray background", "F2F2F2"),
    ]
    for text, color in shadings:
        p = doc.add_paragraph(text + " - This paragraph has a colored background.")
        # Apply shading via XML
        pPr = p._element.get_or_add_pPr()
        shd = pPr.makeelement(qn("w:shd"), {
            qn("w:val"): "clear",
            qn("w:color"): "auto",
            qn("w:fill"): color,
        })
        pPr.append(shd)
    doc.save(path)


def classic56_images_and_tables_mixed(path):
    """Document with alternating images and tables."""
    doc = Document()
    doc.add_heading("Product Catalog", level=1)

    products = [
        ("Widget Pro", (50, 120, 200), "$49.99", "Premium widget with advanced features"),
        ("Gadget Max", (200, 80, 50), "$79.99", "Industrial-grade gadget for heavy use"),
        ("Connector Plus", (50, 180, 50), "$19.99", "Universal connector with fast transfer"),
    ]

    for name, color, price, desc in products:
        doc.add_heading(name, level=2)
        img_buf = _create_test_png(180, 90, color)
        doc.add_picture(img_buf, width=Inches(2.5))

        table = doc.add_table(rows=3, cols=2)
        table.style = "Table Grid"
        table.rows[0].cells[0].text = "Product"
        table.rows[0].cells[1].text = name
        table.rows[1].cells[0].text = "Price"
        table.rows[1].cells[1].text = price
        table.rows[2].cells[0].text = "Description"
        table.rows[2].cells[1].text = desc
        doc.add_paragraph()
    doc.save(path)


def classic57_right_to_left_text(path):
    """Document with right-to-left text simulation."""
    doc = Document()
    doc.add_heading("Text Direction Test", level=2)

    doc.add_paragraph("Left-to-right (default):")
    doc.add_paragraph("This is standard left-to-right English text.")

    doc.add_paragraph()
    doc.add_paragraph("Right-aligned text (simulating RTL):")
    p = doc.add_paragraph("This text is right-aligned to simulate right-to-left layout.")
    p.alignment = WD_ALIGN_PARAGRAPH.RIGHT

    doc.add_paragraph()
    doc.add_paragraph("Hebrew sample (RTL):")
    p2 = doc.add_paragraph("\u05E9\u05DC\u05D5\u05DD \u05E2\u05D5\u05DC\u05DD")  # Shalom Olam
    p2.alignment = WD_ALIGN_PARAGRAPH.RIGHT

    doc.add_paragraph()
    doc.add_paragraph("Arabic sample (RTL):")
    p3 = doc.add_paragraph("\u0645\u0631\u062D\u0628\u0627 \u0628\u0627\u0644\u0639\u0627\u0644\u0645")  # Marhaba bilalam
    p3.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    doc.save(path)


def classic58_dense_paragraph_document(path):
    """Long document with many paragraphs spanning multiple pages."""
    doc = Document()
    doc.add_heading("Research Paper: Modern Software Engineering", level=1)
    doc.add_paragraph("Author: Dr. Jane Smith | Published: March 2026")
    doc.add_paragraph()

    sections = [
        ("Abstract", 2),
        ("1. Introduction", 3),
        ("2. Literature Review", 4),
        ("3. Methodology", 3),
        ("4. Results", 4),
        ("5. Discussion", 3),
        ("6. Conclusion", 2),
    ]
    filler = (
        "Modern software engineering practices emphasize continuous integration, "
        "automated testing, and agile methodologies. The rapid evolution of cloud computing "
        "and containerization has transformed how teams build and deploy applications. "
        "Microservices architecture enables independent scaling and deployment of components. "
    )
    for title, para_count in sections:
        doc.add_heading(title, level=2)
        for _ in range(para_count):
            doc.add_paragraph(filler * 3)
    doc.save(path)


def classic59_numbered_and_bullet_mixed(path):
    """Mixed numbered and bullet lists."""
    doc = Document()
    doc.add_heading("Installation Guide", level=1)

    doc.add_heading("Prerequisites", level=2)
    for item in [".NET 8.0 SDK or later", "Visual Studio Code", "Git", "Python 3.10+"]:
        doc.add_paragraph(item, style="List Bullet")

    doc.add_heading("Installation Steps", level=2)
    for step in ["Clone the repository from GitHub",
                 "Open the project in Visual Studio Code",
                 "Restore NuGet packages",
                 "Build the solution",
                 "Run the test suite"]:
        doc.add_paragraph(step, style="List Number")

    doc.add_heading("Configuration Options", level=2)
    for item in ["Set output directory in appsettings.json",
                 "Configure font embedding preferences",
                 "Enable or disable image compression"]:
        doc.add_paragraph(item, style="List Bullet")

    doc.add_heading("Troubleshooting", level=2)
    for step in ["Verify .NET SDK installation with dotnet --version",
                 "Clear NuGet cache if packages fail to restore",
                 "Check file permissions on output directory"]:
        doc.add_paragraph(step, style="List Number")
    doc.save(path)


def classic60_comprehensive_styled_report(path):
    """Comprehensive report using many styling features together."""
    doc = Document()
    # Title page
    doc.add_paragraph("Technology Trends Report", style="Title")
    doc.add_paragraph("Q1 2026 Analysis", style="Subtitle")
    p_author = doc.add_paragraph()
    p_author.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_author.add_run("Prepared by MiniPdf Analytics Team")
    p_date = doc.add_paragraph()
    p_date.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r_date = p_date.add_run("March 2026")
    r_date.italic = True
    doc.add_page_break()

    # Executive Summary
    doc.add_heading("Executive Summary", level=1)
    p_summary = doc.add_paragraph()
    p_summary.add_run("Key Findings: ").bold = True
    p_summary.add_run(
        "The technology sector continues to show strong growth driven by AI adoption, "
        "cloud migration, and digital transformation initiatives."
    )

    # Market Overview with styled table
    doc.add_heading("Market Overview", level=1)
    table = doc.add_table(rows=6, cols=4)
    table.style = "Table Grid"
    headers = ["Sector", "Q4 2025", "Q1 2026", "Change"]
    for i, h in enumerate(headers):
        cell = table.rows[0].cells[i]
        cell.text = h
        _set_cell_shading(cell, "2F5496")
        for run in cell.paragraphs[0].runs:
            run.font.color.rgb = RGBColor(255, 255, 255)
            run.bold = True

    sectors = [
        ("Cloud Computing", "$180B", "$195B", "+8.3%"),
        ("AI/ML", "$95B", "$112B", "+17.9%"),
        ("Cybersecurity", "$72B", "$78B", "+8.3%"),
        ("IoT", "$45B", "$48B", "+6.7%"),
        ("Blockchain", "$12B", "$11B", "-8.3%"),
    ]
    for ri, (sector, q4, q1, change) in enumerate(sectors):
        row = table.rows[ri + 1]
        row.cells[0].text = sector
        row.cells[1].text = q4
        row.cells[2].text = q1
        p = row.cells[3].paragraphs[0]
        r = p.add_run(change)
        if change.startswith("+"):
            r.font.color.rgb = RGBColor(0, 128, 0)
        else:
            r.font.color.rgb = RGBColor(255, 0, 0)
        if ri % 2 == 0:
            for ci in range(4):
                _set_cell_shading(row.cells[ci], "D9E2F3")

    # Trends
    doc.add_heading("Key Trends", level=1)
    doc.add_heading("Artificial Intelligence", level=2)
    for item in ["Large Language Models becoming mainstream",
                 "AI-assisted coding tools adoption growing",
                 "Regulation frameworks being established"]:
        doc.add_paragraph(item, style="List Bullet")

    doc.add_heading("Cloud & Infrastructure", level=2)
    for item in ["Multi-cloud strategies dominating",
                 "Serverless architecture expanding",
                 "Edge computing gaining traction"]:
        doc.add_paragraph(item, style="List Bullet")

    # Visual section
    doc.add_heading("Visual Summary", level=1)
    doc.add_paragraph("Growth indicator (placeholder):")
    img_buf = _create_test_png(300, 120, (46, 84, 150))
    doc.add_picture(img_buf, width=Inches(4))

    # Recommendations with numbered list
    doc.add_heading("Recommendations", level=1)
    for rec in ["Accelerate AI integration strategy",
                "Diversify cloud vendor dependencies",
                "Invest in employee upskilling programs",
                "Strengthen data governance frameworks",
                "Evaluate emerging quantum computing capabilities"]:
        doc.add_paragraph(rec, style="List Number")

    doc.add_paragraph()
    p_footer = doc.add_paragraph()
    p_footer.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r_footer = p_footer.add_run("--- End of Report ---")
    r_footer.italic = True
    r_footer.font.color.rgb = RGBColor(128, 128, 128)
    doc.save(path)


# ── Helpers ──────────────────────────────────────────────────────────────

def _set_cell_shading(cell, hex_color):
    """Set a table cell's background shading."""
    shading = cell._element.get_or_add_tcPr()
    shd = shading.makeelement(qn("w:shd"), {
        qn("w:val"): "clear",
        qn("w:color"): "auto",
        qn("w:fill"): hex_color,
    })
    shading.append(shd)


def _add_horizontal_rule(doc):
    """Add a paragraph that looks like a horizontal rule."""
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(6)
    p.paragraph_format.space_after = Pt(6)
    pPr = p._element.get_or_add_pPr()
    pBdr = pPr.makeelement(qn("w:pBdr"), {})
    bottom = pBdr.makeelement(qn("w:bottom"), {
        qn("w:val"): "single",
        qn("w:sz"): "6",
        qn("w:space"): "1",
        qn("w:color"): "auto",
    })
    pBdr.append(bottom)
    pPr.append(pBdr)


# ── Main ─────────────────────────────────────────────────────────────────

ALL_GENERATORS = [
    classic01_single_paragraph,
    classic02_multiple_paragraphs,
    classic03_headings,
    classic04_bold_italic,
    classic05_font_sizes,
    classic06_font_colors,
    classic07_alignment,
    classic08_bullet_list,
    classic09_numbered_list,
    classic10_simple_table,
    classic11_table_with_shading,
    classic12_merged_cells_table,
    classic13_long_document,
    classic14_mixed_content,
    classic15_indentation,
    classic16_line_spacing,
    classic17_page_break,
    classic18_embedded_image,
    classic19_multiple_images,
    classic20_table_with_many_rows,
    classic21_nested_lists,
    classic22_horizontal_rule,
    classic23_mixed_formatting_runs,
    classic24_two_column_table_layout,
    classic25_title_and_subtitle,
    classic26_table_alignment,
    classic27_long_paragraph_wrapping,
    classic28_special_characters,
    classic29_table_with_image,
    classic30_comprehensive_report,
    classic31_strikethrough_text,
    classic32_superscript_subscript,
    classic33_highlighted_text,
    classic34_paragraph_borders,
    classic35_tab_stops,
    classic36_wide_table,
    classic37_nested_table,
    classic38_table_column_widths,
    classic39_financial_report,
    classic40_resume,
    classic41_business_letter,
    classic42_meeting_minutes,
    classic43_invoice_document,
    classic44_memo,
    classic45_project_plan,
    classic46_comparison_table,
    classic47_data_dictionary,
    classic48_multi_level_headings,
    classic49_cjk_document,
    classic50_long_table_with_formatting,
    classic51_underline_styles,
    classic52_spacing_before_after,
    classic53_table_merged_complex,
    classic54_multi_font_family,
    classic55_background_shading_paragraph,
    classic56_images_and_tables_mixed,
    classic57_right_to_left_text,
    classic58_dense_paragraph_document,
    classic59_numbered_and_bullet_mixed,
    classic60_comprehensive_styled_report,
]


def main():
    parser = argparse.ArgumentParser(description="Generate classic DOCX test files")
    parser.add_argument("--outdir", default=str(OUTPUT_DIR), help="Output directory")
    args = parser.parse_args()

    outdir = Path(args.outdir)
    outdir.mkdir(parents=True, exist_ok=True)

    print(f"Generating {len(ALL_GENERATORS)} classic DOCX files to {outdir}/")
    print()

    passed = 0
    failed = 0
    for i, gen_func in enumerate(ALL_GENERATORS, 1):
        name = gen_func.__name__
        filename = f"docx_{name}.docx"
        filepath = outdir / filename
        try:
            gen_func(str(filepath))
            size_kb = filepath.stat().st_size / 1024
            print(f"  OK  {filename} ({size_kb:.1f} KB)")
            passed += 1
        except Exception as e:
            print(f"  ERR {filename}: {e}")
            failed += 1

    print(f"\nDone! Passed: {passed}, Failed: {failed}, Total: {len(ALL_GENERATORS)}")


if __name__ == "__main__":
    main()
