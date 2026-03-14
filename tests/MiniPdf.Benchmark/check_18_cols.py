"""Analyze classic18 column widths between MiniPdf and Reference."""
import fitz, os

for label, path in [('MiniPdf', os.path.join('..', 'MiniPdf.Scripts', 'pdf_output', 'classic18_large_dataset.pdf')),
                     ('Ref', os.path.join('reference_pdfs', 'classic18_large_dataset.pdf'))]:
    doc = fitz.open(path)
    page = doc[0]
    
    print(f"\n=== {label} - page 1 spans (first few rows) ===")
    blocks = page.get_text("dict", sort=True)
    row_count = 0
    prev_y = -1
    for block in blocks['blocks']:
        if block['type'] != 0:
            continue
        for line in block['lines']:
            for span in line['spans']:
                if not span['text'].strip():
                    continue
                y = round(span['bbox'][1])
                if y != prev_y:
                    if row_count > 3:
                        break
                    row_count += 1
                    prev_y = y
                    print(f"  Row Y={y}:")
                x = span['bbox'][0]
                text = span['text'].strip()[:20]
                print(f"    x={x:.1f} text={repr(text)}")
            if row_count > 3:
                break
        if row_count > 3:
            break
    
    # Get page dimensions
    print(f"  Page: {page.rect.width:.1f} x {page.rect.height:.1f}")
