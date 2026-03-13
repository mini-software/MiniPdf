"""Compare MiniPdf vs LibreOffice PDF for Invoice.docx"""
import sys, os, json
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', '..', 'MiniPdf.Benchmark'))

from compare_pdfs import compare_single
import fitz  # PyMuPDF

MINIPDF_PDF = os.path.join(os.path.dirname(__file__), 'output', 'Invoice.pdf')
REFERENCE_PDF = os.path.join(os.path.dirname(__file__), 'output_reference', 'Invoice.pdf')
REPORT_DIR = os.path.join(os.path.dirname(__file__), 'output_report')

os.makedirs(os.path.join(REPORT_DIR, 'images'), exist_ok=True)

# Run comparison
result = compare_single(MINIPDF_PDF, REFERENCE_PDF, os.path.join(REPORT_DIR, 'images'), 'Invoice')

# Print report
print("=" * 60)
print("Invoice.docx  MiniPdf vs LibreOffice Comparison")
print("=" * 60)
for key, val in result.items():
    if isinstance(val, float):
        print(f"  {key}: {val:.4f}")
    else:
        print(f"  {key}: {val}")
print("=" * 60)

# Extract text from both for debugging
print("\n--- MiniPdf extracted text ---")
doc = fitz.open(MINIPDF_PDF)
for i, page in enumerate(doc):
    print(f"[Page {i+1}]")
    print(page.get_text())
doc.close()

print("\n--- LibreOffice extracted text ---")
doc = fitz.open(REFERENCE_PDF)
for i, page in enumerate(doc):
    print(f"[Page {i+1}]")
    print(page.get_text())
doc.close()

print(f"\nComparison images saved to: {os.path.join(REPORT_DIR, 'images')}")
