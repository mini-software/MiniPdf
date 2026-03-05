"""Check raw text operations in MiniPdf PDF."""
import fitz, sys

name = sys.argv[1] if len(sys.argv) > 1 else "classic42_boolean_values"
path = sys.argv[2] if len(sys.argv) > 2 else f"../MiniPdf.Scripts/pdf_output/{name}.pdf"
doc = fitz.open(path)
page = doc[0]
# Use rawdict to get individual character positions
blocks = page.get_text("rawdict")["blocks"]
for b in blocks:
    if "lines" in b:
        for l in b["lines"]:
            for s in l["spans"]:
                chars = s.get("chars", [])
                if chars:
                    text = "".join(c["c"] for c in chars)
                    first_x = chars[0]["bbox"][0]
                    last_x = chars[-1]["bbox"][2]
                    # Find gaps between characters
                    gaps = []
                    for ci in range(1, len(chars)):
                        gap = chars[ci]["bbox"][0] - chars[ci-1]["bbox"][2]
                        if abs(gap) > 0.1:  # any non-trivial gap
                            gaps.append((ci, round(gap, 2), chars[ci-1]["c"], chars[ci]["c"]))
                    print(f"  x={first_x:.1f}-{last_x:.1f} \"{text}\"")
                    if gaps:
                        for gi, gv, gc1, gc2 in gaps:
                            print(f"    GAP at char {gi}: {gv}pt between '{gc1}' and '{gc2}'")
