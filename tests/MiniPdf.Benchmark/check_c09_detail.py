import fitz
doc = fitz.open('../MiniPdf.Scripts/pdf_output/classic09_long_text.pdf')
p = doc[0]
for b in p.get_text('dict', sort=True)['blocks']:
    if 'lines' in b:
        for line in b['lines']:
            for span in line['spans']:
                t = span['text']
                bb = span['bbox']
                print(f'  len={len(t.strip())} bbox=[{bb[0]:.1f},{bb[1]:.1f},{bb[2]:.1f},{bb[3]:.1f}] size={span["size"]:.1f} text={t.strip()[:40]}')
doc.close()

# Also check raw stream for Tz
doc = fitz.open('../MiniPdf.Scripts/pdf_output/classic09_long_text.pdf')
p = doc[0]
xrefs = p.get_contents()
for xref in xrefs:
    raw = doc.xref_stream(xref).decode('latin-1')
    lines = raw.split('\n')
    for i, l in enumerate(lines):
        if 'Tz' in l:
            print(f'  Tz at line {i}: {l[:80]}')
doc.close()
