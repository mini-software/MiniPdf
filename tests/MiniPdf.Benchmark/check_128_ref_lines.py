"""Check reference classic128 line structure."""
import fitz

path = r'D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs\classic128_font_sizes.pdf'
doc = fitz.open(path)
page = doc[0]

data = page.get_text("dict", sort=True)
for block in data.get("blocks", []):
    if block.get("type", 0) != 0:
        continue
    for line in block.get("lines", []):
        spans = line.get("spans", [])
        texts = [s["text"] for s in spans]
        combined = " ".join(texts)
        if "12" in combined or "10" in combined:
            print(f"LINE bbox=({line['bbox'][0]:.1f}, {line['bbox'][1]:.1f}, {line['bbox'][2]:.1f}, {line['bbox'][3]:.1f})")
            for s in spans:
                print(f"  SPAN: origin=({s['origin'][0]:.3f},{s['origin'][1]:.3f}) bbox=({s['bbox'][0]:.1f},{s['bbox'][1]:.1f},{s['bbox'][2]:.1f},{s['bbox'][3]:.1f}) sz={s['size']:.3f} text={s['text']!r}")
            print()

doc.close()
