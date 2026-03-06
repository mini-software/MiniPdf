import fitz

mini = fitz.open(r'D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic09_long_text.pdf')
ref = fitz.open(r'D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs\classic09_long_text.pdf')
print(f"MiniPdf pages: {len(mini)}, Reference pages: {len(ref)}")

for i in range(min(3, len(mini))):
    page = mini[i]
    text = page.get_text().strip()
    preview = text[:120] if text else "(empty)"
    blocks = page.get_text("dict", sort=True)["blocks"]
    lines = sum(len(b["lines"]) for b in blocks if b["type"] == 0)
    print(f"  MiniPdf Page {i+1}: {lines} lines, text: {preview}")

mini.close()
ref.close()
