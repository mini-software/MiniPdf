"""Full text comparison of classic189_alternating_image_text_rows."""
import fitz
import os

mini_dir = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
ref_dir = 'reference_pdfs'
name = 'classic189_alternating_image_text_rows'

for label, d in [('MiniPdf', mini_dir), ('Reference', ref_dir)]:
    doc = fitz.open(os.path.join(d, f'{name}.pdf'))
    for pi, page in enumerate(doc):
        text = page.get_text()
        lines = text.split('\n')
        print(f'\n=== {label} Page {pi+1} ({len(lines)} lines) ===')
        for li, line in enumerate(lines):
            print(f'  {li:3d}: {repr(line)[:120]}')
    doc.close()
