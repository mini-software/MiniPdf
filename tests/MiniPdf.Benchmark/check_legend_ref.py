"""Compare chart legend text positioning between MiniPdf and reference."""
import fitz, os

cases = [
    'classic100_stacked_bar_chart',
    'classic101_percent_stacked_bar',
    'classic110_chart_with_legend',
]

for case in cases:
    ref = os.path.join('reference_pdfs', f'{case}.pdf')
    if not os.path.exists(ref):
        continue
    
    rd = fitz.open(ref)
    page = rd[0]
    
    print(f"\n=== {case} (reference) ===")
    
    # Get all text blocks with positions
    blocks = page.get_text("dict", sort=True)
    for block in blocks['blocks']:
        if block['type'] != 0:  # text block
            continue
        for line in block['lines']:
            for span in line['spans']:
                text = span['text'].strip()
                if not text:
                    continue
                bbox = span['bbox']
                size = span['size']
                # Look for legend-like text (small font, near top or bottom)
                if size < 10:
                    print(f"  size={size:.1f}  bbox=({bbox[0]:.1f},{bbox[1]:.1f},{bbox[2]:.1f},{bbox[3]:.1f})  text={repr(text[:40])}")
