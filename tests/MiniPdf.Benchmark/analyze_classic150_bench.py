"""Replicate benchmark text extraction for classic150 to understand why text_sim=0.9677."""
import fitz
import os

def extract_text_pymupdf(pdf_path):
    pages = []
    doc = fitz.open(pdf_path)
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
        pages.append("\n".join(lines))
    doc.close()
    return pages

mini_dir = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
ref_dir = 'reference_pdfs'
name = 'classic150_kitchen_sink_styles'

mini_pages = extract_text_pymupdf(os.path.join(mini_dir, f'{name}.pdf'))
ref_pages = extract_text_pymupdf(os.path.join(ref_dir, f'{name}.pdf'))

print(f'Pages: mini={len(mini_pages)}, ref={len(ref_pages)}')

for pi in range(max(len(mini_pages), len(ref_pages))):
    mt = mini_pages[pi] if pi < len(mini_pages) else ''
    rt = ref_pages[pi] if pi < len(ref_pages) else ''
    
    m_lines = mt.split('\n')
    r_lines = rt.split('\n')
    
    print(f'\nPage {pi+1}: mini_lines={len(m_lines)}, ref_lines={len(r_lines)}')
    
    # Show line-by-line comparison
    maxlines = max(len(m_lines), len(r_lines))
    for li in range(maxlines):
        ml = m_lines[li] if li < len(m_lines) else '<missing>'
        rl = r_lines[li] if li < len(r_lines) else '<missing>'
        marker = '  ' if ml == rl else '!!'
        print(f'  {marker} M: {repr(ml)[:90]}')
        if ml != rl:
            print(f'  {marker} R: {repr(rl)[:90]}')
