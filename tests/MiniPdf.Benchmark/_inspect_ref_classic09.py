"""Inspect reference and MiniPdf PDFs for classic09_long_text"""
import fitz  # PyMuPDF

ref_path = "reference_pdfs/classic09_long_text.pdf"
mini_path = "../MiniPdf.Scripts/pdf_output/classic09_long_text.pdf"

for label, path in [("REFERENCE", ref_path), ("MINIPDF", mini_path)]:
    doc = fitz.open(path)
    print(f"\n=== {label}: {doc.page_count} pages ===")
    for i in range(min(doc.page_count, 15)):
        page = doc[i]
        text = page.get_text().strip()
        w, h = page.rect.width, page.rect.height
        text_preview = text[:120].replace('\n', '|') if text else "(empty)"
        print(f"  Page {i+1}: {w:.0f}x{h:.0f} chars={len(text)} text={text_preview}")
    doc.close()
