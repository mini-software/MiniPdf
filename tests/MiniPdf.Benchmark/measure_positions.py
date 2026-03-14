"""Measure text X/Y positions in classic18 reference vs MiniPdf."""
import fitz, sys

def get_all_spans(pdf_path, page=0):
    doc = fitz.open(pdf_path)
    pg = doc[page]
    blocks = pg.get_text("dict")["blocks"]
    spans = []
    for block in blocks:
        if "lines" not in block:
            continue
        for line in block["lines"]:
            for span in line["spans"]:
                spans.append({
                    "text": span["text"].strip(),
                    "x": round(span["origin"][0], 2),
                    "y": round(span["origin"][1], 2),
                    "size": round(span["size"], 2),
                    "font": span["font"]
                })
    return spans

ref_path = r"..\MiniPdf.Benchmark\reference_pdfs\classic18_large_dataset.pdf"
mini_path = r"..\MiniPdf.Scripts\pdf_output\classic18_large_dataset.pdf"

ref_spans = get_all_spans(ref_path, 0)
mini_spans = get_all_spans(mini_path, 0)

print("=== Font comparison ===")
ref_fonts = set(s["font"] for s in ref_spans)
mini_fonts = set(s["font"] for s in mini_spans)
print(f"Ref fonts: {ref_fonts}")
print(f"Mini fonts: {mini_fonts}")

print(f"\n=== First row text positions ===")
# Get unique Y values
ref_ys = sorted(set(s["y"] for s in ref_spans))
mini_ys = sorted(set(s["y"] for s in mini_spans))

# First row spans
print("\nReference row 1 (header):")
row1_ref = [s for s in ref_spans if abs(s["y"] - ref_ys[0]) < 0.5]
for s in sorted(row1_ref, key=lambda x: x["x"])[:10]:
    print(f"  x={s['x']:7.2f} y={s['y']:7.2f} size={s['size']} text='{s['text'][:20]}'")

print("\nMiniPdf row 1 (header):")
row1_mini = [s for s in mini_spans if abs(s["y"] - mini_ys[0]) < 0.5]
for s in sorted(row1_mini, key=lambda x: x["x"])[:10]:
    print(f"  x={s['x']:7.2f} y={s['y']:7.2f} size={s['size']} text='{s['text'][:20]}'")

# Compare X positions for matching text
print("\n=== X position comparison (row 1) ===")
ref_dict = {s["text"]: s for s in sorted(row1_ref, key=lambda x: x["x"])}
mini_dict = {s["text"]: s for s in sorted(row1_mini, key=lambda x: x["x"])}
for text in ref_dict:
    if text in mini_dict:
        dx = mini_dict[text]["x"] - ref_dict[text]["x"]
        dy = mini_dict[text]["y"] - ref_dict[text]["y"]
        print(f"  '{text[:20]:20s}' ref_x={ref_dict[text]['x']:7.2f} mini_x={mini_dict[text]['x']:7.2f} dx={dx:+.2f} dy={dy:+.2f}")

# Row 2 comparison
print("\n=== Row 2 X positions ===")
row2_ref = [s for s in ref_spans if abs(s["y"] - ref_ys[1]) < 0.5]
row2_mini = [s for s in mini_spans if abs(s["y"] - mini_ys[1]) < 0.5]
for i, (r, m) in enumerate(zip(sorted(row2_ref, key=lambda x: x["x"]), sorted(row2_mini, key=lambda x: x["x"]))):
    dx = m["x"] - r["x"]
    print(f"  col{i}: ref_x={r['x']:7.2f} mini_x={m['x']:7.2f} dx={dx:+.2f}  ref='{r['text'][:10]}' mini='{m['text'][:10]}'")

# Also check classic06
print("\n\n=== classic06_tall_table ===")
ref_path2 = r"..\MiniPdf.Benchmark\reference_pdfs\classic06_tall_table.pdf"
mini_path2 = r"..\MiniPdf.Scripts\pdf_output\classic06_tall_table.pdf"
ref2 = get_all_spans(ref_path2, 0)
mini2 = get_all_spans(mini_path2, 0)
ref2_ys = sorted(set(s["y"] for s in ref2))
mini2_ys = sorted(set(s["y"] for s in mini2))
print(f"Ref first Y: {ref2_ys[0]}, Mini first Y: {mini2_ys[0]}, dy={mini2_ys[0]-ref2_ys[0]:+.2f}")
print(f"Ref row gap: {round(ref2_ys[1]-ref2_ys[0],2)}, Mini row gap: {round(mini2_ys[1]-mini2_ys[0],2)}")
print(f"Ref rows on page 1: {len(ref2_ys)}, Mini rows: {len(mini2_ys)}")

# Check classic148
print("\n=== classic148_frozen_styled_grid ===")
ref_path3 = r"..\MiniPdf.Benchmark\reference_pdfs\classic148_frozen_styled_grid.pdf"
mini_path3 = r"..\MiniPdf.Scripts\pdf_output\classic148_frozen_styled_grid.pdf"
ref3 = get_all_spans(ref_path3, 0)
mini3 = get_all_spans(mini_path3, 0)
ref3_ys = sorted(set(s["y"] for s in ref3))
mini3_ys = sorted(set(s["y"] for s in mini3))
print(f"Ref first Y: {ref3_ys[0]}, Mini first Y: {mini3_ys[0]}, dy={mini3_ys[0]-ref3_ys[0]:+.2f}")
# First row X positions
row1_r3 = sorted([s for s in ref3 if abs(s["y"] - ref3_ys[0]) < 0.5], key=lambda x: x["x"])
row1_m3 = sorted([s for s in mini3 if abs(s["y"] - mini3_ys[0]) < 0.5], key=lambda x: x["x"])
for i in range(min(6, len(row1_r3), len(row1_m3))):
    r, m = row1_r3[i], row1_m3[i]
    dx = m["x"] - r["x"]
    print(f"  col{i}: ref_x={r['x']:7.2f} mini_x={m['x']:7.2f} dx={dx:+.2f}  text='{r['text'][:15]}'")
