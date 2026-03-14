import fitz, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

for src in ['../MiniPdf.Scripts/pdf_output/classic09_long_text.pdf', 'reference_pdfs/classic09_long_text.pdf']:
    doc = fitz.open(src)
    p = doc[0]
    blocks = p.get_text('dict', sort=True)['blocks']
    label = 'MiniPdf' if 'pdf_output' in src else 'Reference'
    print(f'\n{label}:')
    for b in blocks:
        if 'lines' in b:
            for line in b['lines']:
                for span in line['spans']:
                    t = span['text'].strip()
                    if len(t) > 20:
                        bbox = span['bbox']
                        print(f'  len={len(t)} bbox=[{bbox[0]:.1f},{bbox[1]:.1f},{bbox[2]:.1f},{bbox[3]:.1f}] text={t[:80]}...')
