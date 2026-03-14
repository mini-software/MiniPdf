"""Check all text in reference chart PDFs."""
import fitz, os

cases = ['classic110_chart_with_legend']

for case in cases:
    ref = os.path.join('reference_pdfs', f'{case}.pdf')
    if not os.path.exists(ref):
        continue
    
    rd = fitz.open(ref)
    page = rd[0]
    
    print(f"\n=== {case} (reference) - all spans ===")
    blocks = page.get_text("dict", sort=True)
    for block in blocks['blocks']:
        if block['type'] != 0:
            continue
        for line in block['lines']:
            for span in line['spans']:
                text = span['text'].strip()
                if not text:
                    continue
                bbox = span['bbox']
                size = span['size']
                print(f"  size={size:.1f}  bbox=({bbox[0]:.0f},{bbox[1]:.0f},{bbox[2]:.0f},{bbox[3]:.0f})  text={repr(text[:60])}")
    
    print(f"\n--- full page text ---")
    print(page.get_text("text")[:500])
