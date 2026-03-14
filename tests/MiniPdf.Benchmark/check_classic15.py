"""Check classic15 reference column positions."""
import fitz

ref = fitz.open('reference_pdfs/classic15_negative_numbers.pdf')
p = ref[0]
blocks = p.get_text('dict', sort=True)['blocks']
for b in blocks:
    if 'lines' not in b:
        continue
    for l in b['lines']:
        for s in l['spans']:
            bx = s['bbox']
            t = s['text']
            print(f"x={bx[0]:.1f} y={bx[1]:.1f} x2={bx[2]:.1f} text='{t}'")

mini = fitz.open('../MiniPdf.Scripts/pdf_output/classic15_negative_numbers.pdf')
print("\n--- MiniPdf ---")
p2 = mini[0]
blocks2 = p2.get_text('dict', sort=True)['blocks']
for b in blocks2:
    if 'lines' not in b:
        continue
    for l in b['lines']:
        for s in l['spans']:
            bx = s['bbox']
            t = s['text']
            print(f"x={bx[0]:.1f} y={bx[1]:.1f} x2={bx[2]:.1f} text='{t}'")
