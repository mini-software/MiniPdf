import fitz
import sys

def analyze_pdf(path, label):
    doc = fitz.open(path)
    print(f"\n=== {label}: {doc.page_count} pages ===")
    for i, page in enumerate(doc):
        text = page.get_text().strip()
        lines = text.split('\n') if text else []
        w, h = page.rect.width, page.rect.height
        orient = "landscape" if w > h else "portrait"
        first_line = lines[0][:60] if lines else "(empty)"
        last_line = lines[-1][:60] if lines else "(empty)"
        print(f"  p{i+1}: {w:.0f}x{h:.0f} {orient} lines={len(lines)} first=[{first_line}] last=[{last_line}]")
    doc.close()

analyze_pdf(r"D:\git\MiniPdf\tests\Issue_Files\minipdf_xlsx\payroll-calculator_f.pdf", "MiniPdf")
analyze_pdf(r"D:\git\MiniPdf\tests\Issue_Files\reference_xlsx\payroll-calculator_f.pdf", "Reference")
