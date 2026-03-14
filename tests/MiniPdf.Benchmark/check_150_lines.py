"""Check classic150 PyMuPDF line structure for the Styled Text row."""
import fitz

path = r'D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic150_kitchen_sink_styles.pdf'
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
        # Show lines near the "Styled Text" area (Y ~108-118)
        line_y = line["bbox"][1]
        if 100 < line_y < 125:
            print(f"LINE bbox=({line['bbox'][0]:.1f}, {line['bbox'][1]:.1f}, {line['bbox'][2]:.1f}, {line['bbox'][3]:.1f})")
            for s in spans:
                print(f"  SPAN: origin=({s['origin'][0]:.3f},{s['origin'][1]:.3f}) "
                      f"bbox=({s['bbox'][0]:.1f},{s['bbox'][1]:.1f},{s['bbox'][2]:.1f},{s['bbox'][3]:.1f}) "
                      f"sz={s['size']:.1f} text={s['text']!r}")
            print()

doc.close()
