"""Compare classic128 text extraction: MiniPdf vs reference."""
import fitz

for label, path in [
    ("MiniPdf", r'D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic128_font_sizes.pdf'),
    ("Reference", r'D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs\classic128_font_sizes.pdf'),
]:
    print(f"\n=== {label} ===")
    doc = fitz.open(path)
    page = doc[0]
    data = page.get_text("dict", sort=True)
    spans = []
    for block in data.get("blocks", []):
        if block.get("type", 0) != 0:
            continue
        for line in block.get("lines", []):
            for s in line.get("spans", []):
                text = s.get("text", "").strip()
                if text:
                    spans.append((round(s["bbox"][1], 1), s["bbox"][0], text, s["size"]))
    spans.sort()
    
    # Group by Y with 1.0pt tolerance (same as benchmark)
    current_y = None
    current_tokens = []
    for y, x, text, sz in spans:
        if current_y is None or abs(y - current_y) > 1.0:
            if current_tokens:
                current_tokens.sort()
                line_text = " ".join(t for _, t, _ in current_tokens)
                sizes = set(sz for _, _, sz in current_tokens)
                print(f"  Y={current_y:7.1f} sizes={sizes} => {line_text}")
            current_y = y
            current_tokens = [(x, text, sz)]
        else:
            current_tokens.append((x, text, sz))
    if current_tokens:
        current_tokens.sort()
        line_text = " ".join(t for _, t, _ in current_tokens)
        sizes = set(sz for _, _, sz in current_tokens)
        print(f"  Y={current_y:7.1f} sizes={sizes} => {line_text}")
    doc.close()
