"""Check classic13_date_strings text: baseline had 1.0, now 0.9738."""
import fitz

def extract_text_flat(path):
    doc = fitz.open(path)
    pages = []
    for page in doc:
        data = page.get_text("dict", sort=True)
        spans = []
        for block in data.get("blocks", []):
            if block.get("type", 0) != 0: continue
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
        pages.append("\n".join(lines))
    doc.close()
    return pages

mini = extract_text_flat('../MiniPdf.Scripts/pdf_output/classic13_date_strings.pdf')
ref = extract_text_flat('reference_pdfs/classic13_date_strings.pdf')

for pi in range(max(len(mini), len(ref))):
    m = mini[pi] if pi < len(mini) else ""
    r = ref[pi] if pi < len(ref) else ""
    if m == r:
        print(f"Page {pi+1}: IDENTICAL")
    else:
        print(f"\nPage {pi+1}: DIFFERENT")
        ml = m.split('\n')
        rl = r.split('\n')
        from difflib import unified_diff
        diff = list(unified_diff(ml, rl, lineterm='', n=1))
        for line in diff[:30]:
            print(f"  {line[:120]}")
