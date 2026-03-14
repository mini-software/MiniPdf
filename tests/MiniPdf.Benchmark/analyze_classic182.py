"""Detailed analysis of classic182_dense_long_text_columns text differences."""
import fitz
import os

def get_text_blocks(pdf_path):
    doc = fitz.open(pdf_path)
    for pi, page in enumerate(doc):
        blocks = page.get_text("blocks")
        print(f"  Page {pi+1}: {len(blocks)} blocks")
        for bi, b in enumerate(blocks):
            x0, y0, x1, y1, text, block_no, block_type = b
            text_preview = text.strip()[:80].replace('\n', '|')
            print(f"    [{bi}] ({x0:.0f},{y0:.0f})-({x1:.0f},{y1:.0f}) type={block_type} '{text_preview}'")
    doc.close()

mini_dir = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
ref_dir = 'reference_pdfs'
name = 'classic182_dense_long_text_columns'

print(f'=== MiniPdf: {name} ===')
get_text_blocks(os.path.join(mini_dir, f'{name}.pdf'))
print(f'\n=== Reference: {name} ===')
get_text_blocks(os.path.join(ref_dir, f'{name}.pdf'))
