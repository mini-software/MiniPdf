"""Compare Y positions between MiniPdf and Reference PDFs for page 2"""
import fitz

mini = fitz.open(r'D:\git\MiniPdf-v2\tests\Issue_Files\minipdf_docx\SA8000 ch sample.pdf')
ref = fitz.open(r'D:\git\MiniPdf-v2\tests\Issue_Files\reference_docx\SA8000 ch sample.pdf')

def get_lines(page, label):
    """Extract text lines with Y positions"""
    td = page.get_text("dict")
    lines = []
    for block in td["blocks"]:
        if "lines" not in block:
            continue
        for line in block["lines"]:
            text = ""
            y = None
            for span in line["spans"]:
                text += span["text"]
                if y is None:
                    y = span["origin"][1]
            text = text.strip()
            if text:
                lines.append((y, text[:70]))
    return lines

print("=== MiniPdf Page 2 ===")
for y, t in get_lines(mini[1], "mini"):
    print(f"  y={y:7.1f}: {t}")

print("\n=== Reference Page 2 ===")
for y, t in get_lines(ref[1], "ref"):
    print(f"  y={y:7.1f}: {t}")

# Also show page 2 last few lines to see where page break is
print("\n=== MiniPdf Page 3 first 10 lines ===")
for y, t in get_lines(mini[2], "mini")[:10]:
    print(f"  y={y:7.1f}: {t}")

print("\n=== Reference Page 3 first 10 lines ===")
for y, t in get_lines(ref[2], "ref")[:10]:
    print(f"  y={y:7.1f}: {t}")
