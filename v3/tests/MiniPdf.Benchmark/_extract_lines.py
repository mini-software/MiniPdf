"""Extract text using the exact compare_pdfs.py method and show line-by-line."""
import fitz
import sys

name = sys.argv[1] if len(sys.argv) > 1 else "classic128_font_sizes"

def extract(path):
    pages = []
    doc = fitz.open(path)
    for page in doc:
        data = page.get_text("dict", sort=True)
        spans = []
        for block in data.get("blocks", []):
            if block.get("type", 0) != 0:
                continue
            for line in block.get("lines", []):
                for span in line.get("spans", []):
                    text = span.get("text", "").strip()
                    if text:
                        spans.append((round(span["bbox"][1], 1), span["bbox"][0], text))
        spans.sort()
        lines = []
        current_y = None
        current_tokens = []
        for y, x, text in spans:
            if current_y is None or abs(y - current_y) > 1.0:
                if current_tokens:
                    current_tokens.sort()
                    lines.append(" ".join(t for _, t in current_tokens))
                current_y = y
                current_tokens = [(x, text)]
            else:
                current_tokens.append((x, text))
        if current_tokens:
            current_tokens.sort()
            lines.append(" ".join(t for _, t in current_tokens))
        pages.append(lines)
    doc.close()
    return pages

mini = extract(f"../MiniPdf.Scripts/pdf_output/{name}.pdf")
ref = extract(f"reference_pdfs/{name}.pdf")

print("=== MINI text (line by line) ===")
for i, page in enumerate(mini):
    print(f"--- Page {i+1} ---")
    for j, line in enumerate(page):
        print(f"  {j:3d}: [{line}]")

print("\n=== REF text (line by line) ===")
for i, page in enumerate(ref):
    print(f"--- Page {i+1} ---") 
    for j, line in enumerate(page):
        print(f"  {j:3d}: [{line}]")
