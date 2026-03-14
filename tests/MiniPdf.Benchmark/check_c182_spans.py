"""Check all text spans in classic182."""
import fitz, io, sys
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

for src_label, src_path in [('MiniPdf', '../MiniPdf.Scripts/pdf_output/classic182_dense_long_text_columns.pdf'),
                             ('Ref', 'reference_pdfs/classic182_dense_long_text_columns.pdf')]:
    doc = fitz.open(src_path)
    print(f'\n{src_label}: pages={doc.page_count}')
    for pi in range(min(doc.page_count, 1)):
        p = doc[pi]
        blocks = p.get_text('dict', sort=True)['blocks']
        count = 0
        for b in blocks:
            if 'lines' in b:
                for line in b['lines']:
                    for span in line['spans']:
                        t = span['text'].strip()
                        if t and count < 30:
                            bb = span['bbox']
                            print(f'  x=[{bb[0]:.1f},{bb[2]:.1f}] y={bb[1]:.1f} text="{t[:50]}"')
                            count += 1
    doc.close()
