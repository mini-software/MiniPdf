"""Compare extracted text for first few lines of SA8000 MiniPdf vs Reference"""
import fitz  # PyMuPDF

mini = fitz.open(r'D:\git\MiniPdf-v2\tests\Issue_Files\minipdf_docx\SA8000 ch sample.pdf')
ref = fitz.open(r'D:\git\MiniPdf-v2\tests\Issue_Files\reference_docx\SA8000 ch sample.pdf')

print("=== MiniPdf Page 1 first 500 chars ===")
mt = mini[0].get_text()
print(repr(mt[:500]))

print("\n=== Reference Page 1 first 500 chars ===")
rt = ref[0].get_text()
print(repr(rt[:500]))

# Also check raw text spans for first block
print("\n=== MiniPdf Page 1 text blocks (first 5) ===")
for i, block in enumerate(mini[0].get_text("blocks")[:5]):
    print(f"  B{i}: bbox={block[:4]}: {repr(block[4][:100])}")

print("\n=== Reference Page 1 text blocks (first 5) ===")
for i, block in enumerate(ref[0].get_text("blocks")[:5]):
    print(f"  B{i}: bbox={block[:4]}: {repr(block[4][:100])}")

# Check text spans for P1 line
print("\n=== MiniPdf Page 1 text dict (first 3 blocks) ===")
td = mini[0].get_text("dict")
for i, block in enumerate(td["blocks"][:3]):
    if "lines" in block:
        for j, line in enumerate(block["lines"]):
            for k, span in enumerate(line["spans"]):
                print(f"  B{i}L{j}S{k}: origin={span['origin']}, size={span['size']:.1f}, text={repr(span['text'][:80])}")
