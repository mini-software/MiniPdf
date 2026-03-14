"""Show ALL lines for classic101 comparison."""
import fitz

def extract_all(path):
    doc = fitz.open(path)
    all_lines = []
    for page in doc:
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
        current_y = None
        tokens = []
        for y, x, text in spans:
            if current_y is None or abs(y - current_y) > 1.0:
                if tokens:
                    tokens.sort()
                    all_lines.append(" ".join(t for _, t in tokens))
                current_y = y
                tokens = [(x, text)]
            else:
                tokens.append((x, text))
        if tokens:
            tokens.sort()
            all_lines.append(" ".join(t for _, t in tokens))
    doc.close()
    return all_lines

mini = extract_all(r'..\MiniPdf.Scripts\pdf_output\classic101_percent_stacked_bar.pdf')
ref = extract_all(r'reference_pdfs\classic101_percent_stacked_bar.pdf')

print("=== MiniPdf ===")
for line in mini:
    print(f"  {line}")
print(f"\n=== Reference ===")
for line in ref:
    print(f"  {line}")
