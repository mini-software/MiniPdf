import fitz
doc = fitz.open(r'D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs\classic09_long_text.pdf')

for i in range(min(doc.page_count, 4)):
    page = doc[i]
    blocks = page.get_text('dict')['blocks']
    spans_found = 0
    for b in blocks:
        if b['type'] != 0:
            continue
        for line in b.get('lines', []):
            for span in line['spans']:
                text = span['text']
                if len(text) > 0:
                    bbox = span['bbox']
                    spans_found += 1
                    print(f'Pg{i+1} bbox=({bbox[0]:.0f},{bbox[1]:.0f},{bbox[2]:.0f},{bbox[3]:.0f}) len={len(text)} text={text[:60]}')
    if spans_found == 0:
        print(f'Pg{i+1}: no text spans')

# Check pixel content of pages
print('\n--- Visual content check ---')
for i in range(min(doc.page_count, 4)):
    page = doc[i]
    pix = page.get_pixmap(dpi=72)
    samples = pix.samples
    total_pixels = pix.width * pix.height
    non_white = sum(1 for j in range(0, len(samples), 3) if samples[j] < 250 or samples[j+1] < 250 or samples[j+2] < 250)
    print(f'Pg{i+1}: {pix.width}x{pix.height}, non-white pixels: {non_white}/{total_pixels} ({non_white/total_pixels*100:.1f}%)')
