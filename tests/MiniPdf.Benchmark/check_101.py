"""Compare classic101 text between MiniPdf and reference."""
import fitz

def extract_page_lines(path, page_num=0):
    doc = fitz.open(path)
    if page_num >= len(doc):
        return []
    page = doc[page_num]
    data = page.get_text("dict", sort=True)
    spans = []
    for block in data.get("blocks", []):
        if block.get("type", 0) != 0:
            continue
        for line in block.get("lines", []):
            for s in line.get("spans", []):
                text = s.get("text", "").strip()
                if text:
                    spans.append((round(s["bbox"][1], 1), s["bbox"][0], text))
    spans.sort()
    lines = []
    current_y = None
    tokens = []
    for y, x, text in spans:
        if current_y is None or abs(y - current_y) > 1.0:
            if tokens:
                tokens.sort()
                lines.append(" ".join(t for _, t in tokens))
            current_y = y
            tokens = [(x, text)]
        else:
            tokens.append((x, text))
    if tokens:
        tokens.sort()
        lines.append(" ".join(t for _, t in tokens))
    doc.close()
    return lines

for label, base in [("MiniPdf", "../MiniPdf.Scripts/pdf_output/"), ("Reference", "reference_pdfs/")]:
    path = f"{base}classic101_percent_stacked_bar.pdf"
    print(f"\n=== {label} ===")
    doc = fitz.open(path)
    for pi in range(len(doc)):
        lines = extract_page_lines(path, pi)
        print(f"  Page {pi+1}: {len(lines)} lines")
        for line in lines[:10]:
            print(f"    {line[:100]}")
    doc.close()
