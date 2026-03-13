"""Investigate the spacing issue around checkmarks in SA8000"""
import fitz

mini = fitz.open(r'D:\git\MiniPdf-v2\tests\Issue_Files\minipdf_docx\SA8000 ch sample.pdf')

# Find the line with item 1 checkmark
td = mini[0].get_text("dict")
for block in td["blocks"]:
    if "lines" not in block:
        continue
    for line in block["lines"]:
        text = "".join(s["text"] for s in line["spans"])
        if "签定劳动合同" in text:
            print(f"Line text: {repr(text)}")
            for i, span in enumerate(line["spans"]):
                print(f"  Span {i}: x={span['origin'][0]:.1f} y={span['origin'][1]:.1f} "
                      f"size={span['size']:.1f} text={repr(span['text'][:40])}")
            # Also show raw character positions
            chars = mini[0].get_text("rawdict")
            for rb in chars["blocks"]:
                if "lines" not in rb:
                    continue
                for rl in rb["lines"]:
                    rt = "".join(s["text"] for s in rl["spans"])
                    if "签定劳动合同" in rt:
                        for ri, rs in enumerate(rl["spans"]):
                            if "（" in rs["text"] or "√" in rs["text"] or "）" in rs["text"]:
                                print(f"\n  Raw span {ri}: text={repr(rs['text'][:30])}")
                                if "chars" in rs:
                                    for ci, ch in enumerate(rs["chars"][:20]):
                                        print(f"    char[{ci}]: '{ch['c']}' x={ch['origin'][0]:.1f} bbox={tuple(round(v,1) for v in ch['bbox'])}")
            break
