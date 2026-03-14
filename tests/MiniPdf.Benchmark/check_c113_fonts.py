"""Check font sizes and Y positions of all text in classic113."""
import fitz, io, sys
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

for src_label, src_path in [('MiniPdf', '../MiniPdf.Scripts/pdf_output/classic113_chart_sheet.pdf'),
                             ('Ref', 'reference_pdfs/classic113_chart_sheet.pdf')]:
    doc = fitz.open(src_path)
    p = doc[0]
    print(f'\n{src_label}: page={p.rect.width:.0f}x{p.rect.height:.0f}')
    blocks = p.get_text('dict', sort=True)['blocks']
    for b in blocks:
        if 'lines' in b:
            for line in b['lines']:
                for span in line['spans']:
                    t = span['text'].strip()
                    if t:
                        bb = span['bbox']
                        print(f'  y={bb[1]:6.1f} size={span["size"]:5.1f} text={t}')
    doc.close()
