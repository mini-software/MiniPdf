import sys
sys.path.insert(0, '.')
from compare_pdfs import extract_text_pymupdf

name = sys.argv[1] if len(sys.argv) > 1 else "classic128_font_sizes"

mini = extract_text_pymupdf(f"../MiniPdf.Scripts/pdf_output/{name}.pdf")
ref = extract_text_pymupdf(f"reference_pdfs/{name}.pdf")

print("=== MINI (compare_pdfs extraction) ===")
for line in mini:
    print(f"  [{line}]")
print()
print("=== REF (compare_pdfs extraction) ===")
for line in ref:
    print(f"  [{line}]")
