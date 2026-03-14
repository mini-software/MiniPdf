"""Compare classic09 to see how many repeated chars fit per cell."""
import fitz

for label, path in [('MiniPdf', '../MiniPdf.Scripts/pdf_output/classic09_long_text.pdf'),
                     ('Ref', 'reference_pdfs/classic09_long_text.pdf')]:
    doc = fitz.open(path)
    page = doc[0]
    print(f'\n=== {label} page 1 ===')
    
    blocks = page.get_text("dict")["blocks"]
    for b in blocks:
        if b['type'] == 0:
            for line in b['lines']:
                for span in line['spans']:
                    text = span['text']
                    bbox = span['bbox']
                    if len(text) > 5:
                        print(f'  "{text[:50]}..." len={len(text)} bbox=({bbox[0]:.1f},{bbox[1]:.1f},{bbox[2]:.1f},{bbox[3]:.1f}) w={bbox[2]-bbox[0]:.1f}')
                    else:
                        print(f'  "{text}" len={len(text)} bbox=({bbox[0]:.1f},{bbox[1]:.1f},{bbox[2]:.1f},{bbox[3]:.1f})')
    doc.close()
