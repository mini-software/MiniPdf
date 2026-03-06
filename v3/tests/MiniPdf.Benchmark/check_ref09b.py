import fitz

ref = fitz.open(r'D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs\classic09_long_text.pdf')
print(f"Reference pages: {len(ref)}")
for i in range(len(ref)):
    page = ref[i]
    blocks = page.get_text("dict", sort=True)["blocks"]
    text_count = sum(1 for b in blocks if b["type"] == 0)
    line_count = sum(len(b["lines"]) for b in blocks if b["type"] == 0)
    all_text = page.get_text().strip()
    preview = all_text[:100] if all_text else "(empty)"
    print(f"  Page {i+1}: {text_count} blocks, {line_count} lines, text: {preview}")

# Check page 1 text lengths
print("\n=== Page 1 text lengths ===")
page = ref[0]
blocks = page.get_text("dict", sort=True)["blocks"]
for b in blocks:
    if b["type"] == 0:
        for line in b["lines"]:
            for span in line["spans"]:
                txt = span["text"]
                x0 = span["origin"][0]
                y0 = span["origin"][1]
                print(f"  ({x0:.1f},{y0:.1f}): len={len(txt)} chars, first30={txt[:30]}")

ref.close()
