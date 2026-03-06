import fitz
doc = fitz.open(r'D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs\classic09_long_text.pdf')
print(f'Total pages: {doc.page_count}')
for i in range(doc.page_count):
    page = doc[i]
    text = page.get_text()
    blocks = page.get_text('dict')['blocks']
    # count text blocks vs non-text blocks
    text_blocks = [b for b in blocks if b['type'] == 0]
    img_blocks = [b for b in blocks if b['type'] == 1]
    total_chars = sum(len(s['text']) for b in text_blocks for l in b.get('lines',[]) for s in l['spans'])
    print(f'  Page {i+1}: {len(text_blocks)} text blocks, {len(img_blocks)} img blocks, {total_chars} total chars, {len(text.strip())} stripped text len')
