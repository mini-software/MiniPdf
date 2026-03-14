"""Full text comparison of classic182_dense_long_text_columns."""
import fitz
import os

mini_dir = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
ref_dir = 'reference_pdfs'
name = 'classic182_dense_long_text_columns'

for label, d in [('MiniPdf', mini_dir), ('Reference', ref_dir)]:
    doc = fitz.open(os.path.join(d, f'{name}.pdf'))
    for pi, page in enumerate(doc):
        text = page.get_text()
        lines = text.split('\n')
        print(f'\n=== {label} Page {pi+1} ({len(lines)} lines) ===')
        for li, line in enumerate(lines):
            print(f'  {li:3d}: {repr(line)[:100]}')
    doc.close()
