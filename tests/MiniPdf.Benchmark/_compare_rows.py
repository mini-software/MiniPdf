"""Compare row positions between reference and MINI PDFs."""
import fitz, sys

name = sys.argv[1] if len(sys.argv) > 1 else "classic128_font_sizes"

def get_row_positions(path):
    doc = fitz.open(path)
    page = doc[0]
    data = page.get_text('dict', sort=True)
    positions = []
    for block in data.get('blocks', []):
        if block.get('type', 0) != 0:
            continue
        for line in block.get('lines', []):
            for span in line.get('spans', []):
                text = span.get('text', '').strip()
                if text:
                    origin = span.get('origin', (0, 0))
                    positions.append((round(origin[1], 2), text[:30]))
    return positions

ref_pos = get_row_positions(f'reference_pdfs/{name}.pdf')
mini_pos = get_row_positions(f'../MiniPdf.Scripts/pdf_output/{name}.pdf')

ref_ys = sorted(set(y for y, _ in ref_pos))
mini_ys = sorted(set(y for y, _ in mini_pos))

print(f"REF rows: {len(ref_ys)}, MINI rows: {len(mini_ys)}")
for i in range(min(len(ref_ys), len(mini_ys))):
    ref_y = ref_ys[i]
    mini_y = mini_ys[i]
    delta = mini_y - ref_y
    ref_text = [t for y, t in ref_pos if y == ref_y]
    mini_text = [t for y, t in mini_pos if y == mini_y]
    ref_sp = ref_ys[i] - ref_ys[i-1] if i > 0 else 0
    mini_sp = mini_ys[i] - mini_ys[i-1] if i > 0 else 0
    print(f"  Row {i:2d}: REF_Y={ref_y:8.2f} MINI_Y={mini_y:8.2f} diff={delta:+6.2f}  REF_sp={ref_sp:6.2f} MINI_sp={mini_sp:6.2f}  [{ref_text[0] if ref_text else ''}]")
