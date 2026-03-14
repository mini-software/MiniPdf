"""Check classic150 line splitting with current factor=5.0."""
import fitz

for label, path in [
    ("MiniPdf", r'D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic150_kitchen_sink_styles.pdf'),
    ("Reference", r'D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs\classic150_kitchen_sink_styles.pdf'),
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
    
    current_y = None
    current_tokens = []
    for y, x, text, sz in spans:
        if current_y is None or abs(y - current_y) > 1.0:
            if current_tokens:
                current_tokens.sort()
                line_text = " ".join(t for _, t, _ in current_tokens)
                sizes = set(round(sz, 1) for _, _, sz in current_tokens)
                if any("Styled" in t or "styled" in t.lower() for _, t, _ in current_tokens) or len(sizes) > 1:
                    print(f"  Y={current_y:7.1f} sizes={sizes} => {line_text[:80]}")
            current_y = y
            current_tokens = [(x, text, sz)]
        else:
            current_tokens.append((x, text, sz))
    if current_tokens:
        current_tokens.sort()
        line_text = " ".join(t for _, t, _ in current_tokens)
        sizes = set(round(sz, 1) for _, _, sz in current_tokens)
        if any("Styled" in t or "styled" in t.lower() for _, t, _ in current_tokens) or len(sizes) > 1:
            print(f"  Y={current_y:7.1f} sizes={sizes} => {line_text[:80]}")
    doc.close()
