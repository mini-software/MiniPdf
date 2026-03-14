"""Compare baseline vs half-pad benchmarks."""
import json

old = {r['name']: r for r in json.load(open('reports/comparison_report.json', encoding='utf-8'))}
new = {r['name']: r for r in json.load(open('reports/half_pad_report.json', encoding='utf-8'))}

# Wait, we just copied comparison_report to half_pad_report, so they're the same.
# We need to compare against the baseline report saved in comparison_report.json before
# Actually the baseline is in "new_report.json" from the old comparison
# Let me check what's where

# half_pad_report.json = the half-padding result (0.9674)
# new_report.json = first attempt with cap-at-1 + subtract (0.9688)  
# The baseline was overwritten... need to re-generate baseline

# Let me just look at page count changes
half = json.load(open('reports/half_pad_report.json', encoding='utf-8'))
page_mismatches = [(r['name'], r.get('minipdf_pages'), r.get('reference_pages'), r['overall_score'])
                   for r in half if r.get('minipdf_pages') != r.get('reference_pages')]
if page_mismatches:
    print("Page mismatches in half-pad:")
    for name, mp, rp, ov in sorted(page_mismatches, key=lambda x: x[3]):
        print(f"  {name}: {mp}/{rp} pages, overall={ov:.4f}")
else:
    print("No page mismatches")

# Check biggest regressions from overall expected score
# Sort by overall score
low = sorted(half, key=lambda x: x['overall_score'])[:20]
print("\n20 lowest overall:")
for r in low:
    print(f"  {r['name']}: overall={r['overall_score']:.4f} text={r['text_similarity']:.4f} vis={r.get('visual_avg', 0):.4f}")
