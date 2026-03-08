import json, zipfile, os
import subprocess

docx_path = r'd:\git\MiniPdf\tests\MiniPdf.Scripts\output_docx\docx_classic88_presentation_handout.docx'
minipdf_path = r'd:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output_docx\docx_classic88_presentation_handout.pdf'
ref_path = r'd:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs_docx\docx_classic88_presentation_handout.pdf'

# Extract text from both PDFs
for label, path in [('MiniPdf', minipdf_path), ('Reference', ref_path)]:
    try:
        result = subprocess.run(['pdftotext', '-layout', path, '-'], capture_output=True, text=True, timeout=10)
        text = result.stdout
        print(f'=== {label} text (first 1000 chars) ===')
        print(text[:1000])
        print(f'Total: {len(text)} chars')
    except Exception as e:
        print(f'{label}: error {e}')
    print()

# Check DOCX structure
print('=== DOCX Structure ===')
with zipfile.ZipFile(docx_path) as z:
    with z.open('word/document.xml') as f:
        content = f.read().decode('utf-8')
    
    # Count tables and rows
    print(f'Tables: {content.count("<w:tbl>")}')
    print(f'Rows: {content.count("<w:tr>")}')
    print(f'Paragraphs: {content.count("<w:p>")}')
    
    # Find shading colors
    import re
    shd_fills = set(re.findall(r'w:fill="([^"]+)"', content))
    print(f'Shading fills: {shd_fills}')
    
    # Extract some actual text
    texts = re.findall(r'<w:t[^>]*>([^<]+)</w:t>', content)
    print(f'\nText content ({len(texts)} runs):')
    for t in texts[:30]:
        print(f'  "{t}"')
