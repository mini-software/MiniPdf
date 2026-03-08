"""Analyze XLSX near-miss cases (0.97-0.99) and classic09_long_text."""
import json
import fitz

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

# Near-miss cases (overall 0.97-0.99)
near = sorted([r for r in data if 0.97 <= r['overall_score'] < 0.99], key=lambda x: x['overall_score'])
print(f"XLSX near-miss (0.97-0.99): {len(near)} cases")
for r in near:
    ts = r['text_similarity']
    vs = r['visual_avg']
    ps = 1.0 if r['minipdf_pages'] == r['reference_pages'] else 0.0
    # What's holding it back?
    blockers = []
    if ts < 0.99: blockers.append(f"text={ts:.4f}")
    if vs < 0.99: blockers.append(f"vis={vs:.4f}")
    if ps < 1.0: blockers.append(f"pages={r['minipdf_pages']}vs{r['reference_pages']}")
    print(f"  {r['name']}: overall={r['overall_score']:.4f} [{', '.join(blockers)}]")

# classic09 analysis
print(f"\n{'='*60}")
print("classic09_long_text analysis")
c09 = [r for r in data if 'classic09' in r['name']][0]
print(f"  text={c09['text_similarity']:.4f} flat={c09['flat_text_similarity']:.4f} word={c09['word_text_similarity']:.4f}")
print(f"  visual_avg={c09['visual_avg']:.4f}")
print(f"  pages: mini={c09['minipdf_pages']} ref={c09['reference_pages']}")
print(f"  visual_scores: {c09['visual_scores']}")

# Extract text from classic09
try:
    mini = fitz.open("../MiniPdf.Scripts/pdf_output/classic09_long_text.pdf")
    ref = fitz.open("reference_pdfs/classic09_long_text.pdf")
    mini_text = ""
    ref_text = ""
    for p in mini:
        mini_text += p.get_text()
    for p in ref:
        ref_text += p.get_text()
    print(f"\n  Mini text chars: {len(mini_text)}")
    print(f"  Ref text chars: {len(ref_text)}")
    print(f"\n  Mini first 300 chars:\n    {repr(mini_text[:300])}")
    print(f"\n  Ref first 300 chars:\n    {repr(ref_text[:300])}")
    mini.close()
    ref.close()
except Exception as e:
    print(f"  Error: {e}")

# Cases closest to 0.99 that could cross threshold
print(f"\n{'='*60}")
print("XLSX cases at 0.98-0.99 (closest to threshold)")
close = sorted([r for r in data if 0.98 <= r['overall_score'] < 0.99], key=lambda x: -x['overall_score'])
for r in close:
    ts = r['text_similarity']
    vs = r['visual_avg']
    blockers = []
    if ts < 0.99: blockers.append(f"text={ts:.4f}")
    if vs < 0.99: blockers.append(f"vis={vs:.4f}")
    need_text_for_99 = (0.99 - vs*0.4 - 0.2) / 0.4 if vs < 1.0 else (0.99 - 0.6) / 0.4
    need_vis_for_99 = (0.99 - ts*0.4 - 0.2) / 0.4 if ts < 1.0 else (0.99 - 0.6) / 0.4
    print(f"  {r['name']}: overall={r['overall_score']:.4f} [{', '.join(blockers)}] need_text>={max(0,need_text_for_99):.3f} need_vis>={max(0,need_vis_for_99):.3f}")
