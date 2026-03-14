"""Check the raw PDF rendering for classic15 Big Loss row."""
import fitz

doc = fitz.open('../MiniPdf.Scripts/pdf_output/classic15_negative_numbers.pdf')
p = doc[0]
data = p.get_text('dict', sort=True)
blocks = data['blocks']
for b in blocks:
    if 'lines' not in b:
        continue
    for l in b['lines']:
        for s in l['spans']:
            bx = s['bbox']
            text = s['text']
            if 'Big' in text or '9999' in text or 'Small' in text:
                origin = s.get('origin', (0, 0))
                print(f"bbox=[{bx[0]:.1f},{bx[1]:.1f},{bx[2]:.1f},{bx[3]:.1f}] "
                      f"origin=({origin[0]:.1f},{origin[1]:.1f}) "
                      f"size={s['size']:.1f} text={repr(text)}")

# Also dump the raw content stream
print("\n--- Page content stream (first 2000 chars) ---")
xref = p.xref
stream = doc.xref_stream(xref)
if stream:
    text = stream.decode('latin-1')[:2000]
    for line in text.split('\n'):
        if 'TJ' in line or 'Tj' in line or 'Tm' in line or 'Td' in line or 'Tf' in line:
            print(line[:200])
