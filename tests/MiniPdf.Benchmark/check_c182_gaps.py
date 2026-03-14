"""Check column text gaps in classic182_dense_long_text_columns."""
import fitz, io, sys
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

for src_label, src_path in [('MiniPdf', '../MiniPdf.Scripts/pdf_output/classic182_dense_long_text_columns.pdf'),
                             ('Ref', 'reference_pdfs/classic182_dense_long_text_columns.pdf')]:
    doc = fitz.open(src_path)
    p = doc[0]
    print(f'\n{src_label}:')
    line_count = 0
    blocks = p.get_text('dict', sort=True)['blocks']
    for b in blocks:
        if 'lines' in b:
            for line in b['lines']:
                spans = [s for s in line['spans'] if s['text'].strip()]
                if len(spans) >= 2:
                    # Show first few spans with bbox
                    for s in spans[:8]:
                        bb = s['bbox']
                        print(f'  x=[{bb[0]:.1f},{bb[2]:.1f}] text="{s["text"].strip()[:30]}"')
                    # Show gaps between consecutive spans
                    for i in range(min(len(spans)-1, 7)):
                        gap = spans[i+1]['bbox'][0] - spans[i]['bbox'][2]
                        print(f'    gap[{i}->{i+1}] = {gap:.2f}pt')
                    print()
                    if line_count > 3: break
                    line_count += 1
    doc.close()
