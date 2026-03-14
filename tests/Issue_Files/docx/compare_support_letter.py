"""Compare Support_Letter MiniPdf vs LibreOffice PDF"""
import sys, os, json
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', '..', 'MiniPdf.Benchmark'))

from compare_pdfs import compare_single
import fitz

MINIPDF_PDF = os.path.join(os.path.dirname(__file__), 'output', 'Support_Letter.pdf')
REFERENCE_PDF = os.path.join(os.path.dirname(__file__), 'output_reference', 'Support_Letter.pdf')
REPORT_DIR = os.path.join(os.path.dirname(__file__), 'output_report')
IMAGES_DIR = os.path.join(REPORT_DIR, 'images')
os.makedirs(IMAGES_DIR, exist_ok=True)

result = compare_single(MINIPDF_PDF, REFERENCE_PDF, IMAGES_DIR, 'Support_Letter')

print('=' * 60)
print('Support_Letter.docx  MiniPdf vs LibreOffice Comparison')
print('=' * 60)
for key, val in result.items():
    if key == 'text_diff':
        continue
    if isinstance(val, float):
        print(f'  {key}: {val:.4f}')
    elif isinstance(val, list) and key != 'diff_images':
        print(f'  {key}: {[round(v,4) if isinstance(v,float) else v for v in val]}')
    elif key != 'diff_images':
        print(f'  {key}: {val}')
print('=' * 60)

# Print text diff
if result.get('text_diff'):
    print('\n--- TEXT DIFF ---')
    print(result['text_diff'][:3000])

# Extract text from both
print('\n--- MiniPdf extracted text ---')
doc = fitz.open(MINIPDF_PDF)
for i, page in enumerate(doc):
    print(f'[Page {i+1}]')
    print(page.get_text())
doc.close()

print('\n--- LibreOffice extracted text ---')
doc = fitz.open(REFERENCE_PDF)
for i, page in enumerate(doc):
    print(f'[Page {i+1}]')
    print(page.get_text())
doc.close()
