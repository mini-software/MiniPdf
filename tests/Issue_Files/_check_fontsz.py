"""Check font sizes and line heights in both PDFs for ISO45001 section"""
import fitz

mini = fitz.open(r'D:\git\MiniPdf-v2\tests\Issue_Files\minipdf_docx\SA8000 ch sample.pdf')
ref = fitz.open(r'D:\git\MiniPdf-v2\tests\Issue_Files\reference_docx\SA8000 ch sample.pdf')

def show_spans(page, label, max_spans=40):
    td = page.get_text("dict")
    count = 0
    for block in td["blocks"]:
        if "lines" not in block:
            continue
        for line in block["lines"]:
            for span in line["spans"]:
                text = span['text'].strip()
                if not text:
                    continue
                y = span['origin'][1]
                sz = span['size']
                print(f"  y={y:7.1f} sz={sz:5.1f}: {text[:65]}")
                count += 1
                if count >= max_spans:
                    return

print("=== MiniPdf Page 2 (ISO45001 section) ===")
show_spans(mini[1], "mini", 50)

print("\n=== Reference Page 2 (ISO45001 section) ===")
show_spans(ref[1], "ref", 50)
