"""Debug text similarity for classic95 by running the exact same extraction as compare_pdfs.py"""
import fitz, sys, os
import difflib

sys.path.insert(0, os.path.dirname(__file__))
from compare_pdfs import extract_text_pymupdf

text_m = extract_text_pymupdf("../MiniPdf.Scripts/pdf_output/classic95_area_chart.pdf")
text_r = extract_text_pymupdf("reference_pdfs/classic95_area_chart.pdf")

flat_m = "\n---PAGE---\n".join(text_m).strip()
flat_r = "\n---PAGE---\n".join(text_r).strip()

print("=== MiniPdf text ===")
print(flat_m[:2000])
print("\n=== Reference text ===")
print(flat_r[:2000])

sm = difflib.SequenceMatcher(None, flat_m, flat_r)
page_ratio = round(sm.ratio(), 4)

flat_m2 = flat_m.replace("\n---PAGE---\n", "\n")
flat_r2 = flat_r.replace("\n---PAGE---\n", "\n")
sm_flat = difflib.SequenceMatcher(None, flat_m2, flat_r2)
flat_ratio = round(sm_flat.ratio(), 4)

words_m = flat_m2.split()
words_r = flat_r2.split()
sm_words = difflib.SequenceMatcher(None, words_m, words_r)
word_ratio = round(sm_words.ratio(), 4)

final = max(page_ratio, flat_ratio, word_ratio)
print(f"\npage_ratio={page_ratio} flat_ratio={flat_ratio} word_ratio={word_ratio} => text_sim={final}")
