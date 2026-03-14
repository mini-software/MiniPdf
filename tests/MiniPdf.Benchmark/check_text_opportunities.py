"""Investigate text differences for top text_data improvement targets."""
import fitz

def extract_lines(path):
    doc = fitz.open(path)
    pages = []
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
        pages.append(lines)
    doc.close()
    return pages

cases = [
    'classic191_payroll_calculator',
    'classic189_alternating_image_text_rows',
    'classic182_dense_long_text_columns',
    'classic49_contact_list',
]

for case in cases:
    mini = extract_lines(f'../MiniPdf.Scripts/pdf_output/{case}.pdf')
    ref = extract_lines(f'reference_pdfs/{case}.pdf')
    print(f"\n=== {case} ===")
    print(f"MiniPdf pages={len(mini)}, Reference pages={len(ref)}")
    
    m_lines = mini[0] if mini else []
    r_lines = ref[0] if ref else []
    
    from difflib import SequenceMatcher
    sm = SequenceMatcher(None, m_lines, r_lines)
    for op, i1, i2, j1, j2 in sm.get_opcodes():
        if op == 'equal':
            continue
        print(f"  {op}:")
        if op in ('replace', 'delete'):
            for line in m_lines[i1:min(i2, i1+3)]:
                print(f"    MINI: {line[:80]}")
        if op in ('replace', 'insert'):
            for line in r_lines[j1:min(j2, j1+3)]:
                print(f"    REF:  {line[:80]}")
        if i2-i1 > 3 or j2-j1 > 3:
            print(f"    ... ({i2-i1} mini lines, {j2-j1} ref lines)")
