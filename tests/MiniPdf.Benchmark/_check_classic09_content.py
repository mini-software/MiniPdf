import fitz

ref_path = 'reference_pdfs/classic09_long_text.pdf'
doc = fitz.open(ref_path)
print(f"Reference: {len(doc)} pages")
for i in range(len(doc)):
    page = doc[i]
    # Get all content types
    text = page.get_text()
    drawings = page.get_drawings()
    images = page.get_images()
    # Get page rect
    rect = page.rect
    print(f"Page {i+1}: rect={rect}, text_len={len(text.strip())}, drawings={len(drawings)}, images={len(images)}")
    if i < 2 or i == len(doc)-1:
        # Detailed content
        blocks = page.get_text('dict')['blocks']
        for b in blocks:
            if 'lines' in b:
                for l in b['lines']:
                    for s in l['spans']:
                        if s['text'].strip():
                            print(f"  TEXT: x={s['bbox'][0]:.1f} y={s['bbox'][1]:.1f} '{s['text'][:40]}'")
doc.close()

# Also check MiniPdf
mp_path = '../MiniPdf.Scripts/pdf_output/classic09_long_text.pdf'
doc2 = fitz.open(mp_path)
print(f"\nMiniPdf: {len(doc2)} pages")
for i in range(len(doc2)):
    page = doc2[i]
    text = page.get_text()
    drawings = page.get_drawings()
    print(f"Page {i+1}: text_len={len(text.strip())}, drawings={len(drawings)}")
doc2.close()
