import fitz

ref = fitz.open(r'D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs\classic09_long_text.pdf')
print(f"Reference pages: {len(ref)}")
for i in range(min(3, len(ref))):
    page = ref[i]
    print(f"\n=== Page {i+1} (w={page.rect.width:.1f} h={page.rect.height:.1f}) ===")
    blocks = page.get_text("dict", sort=True)["blocks"]
    for b in blocks:
        if b["type"] == 0:  # text
            for line in b["lines"][:5]:
                for span in line["spans"]:
                    txt = span["text"][:80]
                    x0 = span["origin"][0]
                    y0 = span["origin"][1]
                    sz = span["size"]
                    print(f"  ({x0:.1f},{y0:.1f}) sz={sz:.1f}: {txt}")
            if len(b["lines"]) > 5:
                print(f"  ... ({len(b['lines'])} lines total)")

# Check page 1 line positions to understand wrapping
print("\n=== Page 1 all line Y positions ===")
page = ref[0]
blocks = page.get_text("dict", sort=True)["blocks"]
ys = []
for b in blocks:
    if b["type"] == 0:
        for line in b["lines"]:
            for span in line["spans"]:
                ys.append(span["origin"][1])
for y in sorted(set(ys)):
    # Find text at this y
    texts = []
    for b in blocks:
        if b["type"] == 0:
            for line in b["lines"]:
                for span in line["spans"]:
                    if abs(span["origin"][1] - y) < 0.5:
                        texts.append(span["text"][:60])
    print(f"  y={y:.1f}: {'; '.join(texts)}")

ref.close()
