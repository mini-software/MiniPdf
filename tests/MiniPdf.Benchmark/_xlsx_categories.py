import json

with open("reports/comparison_report.json", encoding="utf-8") as f:
    data = json.load(f)

below = [x for x in data if x["overall_score"] < 0.99]
below.sort(key=lambda x: x["overall_score"], reverse=True)

# Categorize
text_only = [x for x in below if x.get("text_similarity", 0) < 0.99 and x.get("visual_avg", 0) >= 0.99 and x.get("minipdf_pages") == x.get("reference_pages")]
visual_only = [x for x in below if x.get("visual_avg", 0) < 0.99 and x.get("text_similarity", 0) >= 0.99 and x.get("minipdf_pages") == x.get("reference_pages")]
both = [x for x in below if x.get("text_similarity", 0) < 0.99 and x.get("visual_avg", 0) < 0.99]
page = [x for x in below if x.get("minipdf_pages") != x.get("reference_pages")]

print(f"XLSX below 99: {len(below)}")
print(f"  Text-only: {len(text_only)}")
print(f"  Visual-only: {len(visual_only)}")
print(f"  Both: {len(both)}")
print(f"  Page mismatch: {len(page)}")

# Visual-only cases (most fixable category)
print(f"\nVisual-only cases (text=1.0, vis<0.99):")
for x in sorted(visual_only, key=lambda x: -x["overall_score"])[:15]:
    print(f"  {x['name']:<45} vis={x.get('visual_avg',0):.4f} overall={x['overall_score']:.4f}")

# Text-only cases with smallest delta
print(f"\nText-only cases closest to 0.99:")
for x in sorted(text_only, key=lambda x: -x["overall_score"])[:15]:
    text = x.get("text_similarity", 0)
    vis = x.get("visual_avg", 0)
    print(f"  {x['name']:<45} text={text:.4f} vis={vis:.4f} overall={x['overall_score']:.4f}")
