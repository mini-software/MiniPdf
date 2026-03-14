"""Detailed analysis of classic191_payroll_calculator differences."""
import fitz

def extract_detailed(path):
    doc = fitz.open(path)
    result = []
    for pi, page in enumerate(doc):
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
        result.append((pi, lines))
    doc.close()
    return result

mini = extract_detailed(r'..\MiniPdf.Scripts\pdf_output\classic191_payroll_calculator.pdf')
ref = extract_detailed(r'reference_pdfs\classic191_payroll_calculator.pdf')

print(f"MiniPdf: {len(mini)} pages, Reference: {len(ref)} pages")
for pi in range(min(len(mini), len(ref))):
    m = mini[pi][1]
    r = ref[pi][1]
    if m == r:
        print(f"\n  Page {pi+1}: IDENTICAL ({len(m)} lines)")
    else:
        print(f"\n  Page {pi+1}: MiniPdf={len(m)} lines, Ref={len(r)} lines")
        # Show first few diffs
        from difflib import unified_diff
        diff = list(unified_diff(m, r, lineterm='', n=0))
        for line in diff[:20]:
            print(f"    {line[:100]}")
        if len(diff) > 20:
            print(f"    ... ({len(diff)} total diff lines)")
