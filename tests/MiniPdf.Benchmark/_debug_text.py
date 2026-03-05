"""Debug text similarity for a specific file."""
import fitz, difflib, sys

def extract_text_pymupdf(pdf_path):
    pages = []
    doc = fitz.open(pdf_path)
    for page in doc:
        data = page.get_text("dict", sort=True)
        spans = []
        for block in data.get("blocks", []):
            if block.get("type", 0) != 0:
                continue
            for line in block.get("lines", []):
                for span in line.get("spans", []):
                    text = span.get("text", "").strip()
                    if text:
                        spans.append((round(span["bbox"][1], 1), span["bbox"][0], text))
        spans.sort()
        lines = []
        current_y = None
        current_tokens = []
        for y, x, text in spans:
            if current_y is None or abs(y - current_y) > 1.0:
                if current_tokens:
                    current_tokens.sort()
                    lines.append(" ".join(t for _, t in current_tokens))
                current_tokens = [(x, text)]
                current_y = y
            else:
                current_tokens.append((x, text))
        if current_tokens:
            current_tokens.sort()
            lines.append(" ".join(t for _, t in current_tokens))
        pages.append("\n".join(lines))
    return pages

name = sys.argv[1] if len(sys.argv) > 1 else "classic129_alignment_combos"
text_m = extract_text_pymupdf(f"../MiniPdf.Scripts/pdf_output/{name}.pdf")
text_r = extract_text_pymupdf(f"reference_pdfs/{name}.pdf")

flat_m = "\n---PAGE---\n".join(text_m).strip()
flat_r = "\n---PAGE---\n".join(text_r).strip()

print("=== MINI text ===")
print(flat_m[:500])
print()
print("=== REF text ===")
print(flat_r[:500])
print()
print(f"MINI len={len(flat_m)}, REF len={len(flat_r)}")
sm = difflib.SequenceMatcher(None, flat_m, flat_r)
print(f"Char-level similarity: {sm.ratio():.4f}")
flat_m_no = flat_m.replace("\n---PAGE---\n", "\n")
flat_r_no = flat_r.replace("\n---PAGE---\n", "\n")
sm2 = difflib.SequenceMatcher(None, flat_m_no, flat_r_no)
print(f"Flat similarity: {sm2.ratio():.4f}")
words_m = flat_m_no.split()
words_r = flat_r_no.split()
sm3 = difflib.SequenceMatcher(None, words_m, words_r)
print(f"Word similarity: {sm3.ratio():.4f}")
print(f"text_similarity = {max(sm.ratio(), sm2.ratio(), sm3.ratio()):.4f}")
