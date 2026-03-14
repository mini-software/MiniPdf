"""Full text comparison of classic140_rotated_text."""
import fitz
import os

mini_dir = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
ref_dir = 'reference_pdfs'
name = 'classic140_rotated_text'

for label, d in [('MiniPdf', mini_dir), ('Reference', ref_dir)]:
    doc = fitz.open(os.path.join(d, f'{name}.pdf'))
    for pi, page in enumerate(doc):
        text = page.get_text()
        lines = text.split('\n')
        print(f'\n=== {label} Page {pi+1} ({len(lines)} lines) ===')
        for li, line in enumerate(lines):
            print(f'  {li:3d}: {repr(line)[:120]}')
        
        # Also get text with position info
        blocks = page.get_text("dict")["blocks"]
        print(f'\n  Spans:')
        for b in blocks:
            if b['type'] != 0:
                continue
            for line in b['lines']:
                dir_vec = line['dir']
                for span in line['spans']:
                    text = span['text'][:40]
                    origin = span['origin']
                    bbox = span['bbox']
                    print(f'    dir={dir_vec} origin=({origin[0]:.0f},{origin[1]:.0f}) bbox=({bbox[0]:.0f},{bbox[1]:.0f},{bbox[2]:.0f},{bbox[3]:.0f}) "{text}"')
    doc.close()
