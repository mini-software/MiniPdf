"""Compare row positions between reference and MINI PDFs for classic132."""
import fitz

def get_row_positions(path):
    doc = fitz.open(path)
    page = doc[0]
    data = page.get_text('dict', sort=True)
    y_positions = []
    for block in data.get('blocks', []):
        if block.get('type', 0) != 0:
            continue
        for line in block.get('lines', []):
            for span in line.get('spans', []):
                text = span.get('text', '').strip()
                if text:
                    origin = span.get('origin', (0, 0))
                    bbox = span['bbox']
                    y_positions.append((round(origin[1], 2), text[:30]))
    return y_positions

ref_positions = get_row_positions('reference_pdfs/classic132_striped_table.pdf')
mini_positions = get_row_positions('../MiniPdf.Scripts/pdf_output/classic132_striped_table.pdf')

print("=== Reference row Y positions ===")
for y, text in ref_positions:
    print(f"  Y={y:8.2f} [{text}]")

print("\n=== MINI row Y positions ===")
for y, text in mini_positions:
    print(f"  Y={y:8.2f} [{text}]")

# Compare row spacing
if len(ref_positions) > 1 and len(mini_positions) > 1:
    ref_ys = sorted(set(y for y, _ in ref_positions))
    mini_ys = sorted(set(y for y, _ in mini_positions))
    print(f"\n=== Row Y deltas ===")
    print(f"REF unique Y: {len(ref_ys)}, MINI unique Y: {len(mini_ys)}")
    for i in range(min(len(ref_ys), len(mini_ys))):
        delta = mini_ys[i] - ref_ys[i]
        spacing_ref = ref_ys[i] - ref_ys[i-1] if i > 0 else 0
        spacing_mini = mini_ys[i] - mini_ys[i-1] if i > 0 else 0
        print(f"  Row {i}: REF_Y={ref_ys[i]:8.2f} MINI_Y={mini_ys[i]:8.2f} diff={delta:+.2f} REF_spacing={spacing_ref:.2f} MINI_spacing={spacing_mini:.2f}")
