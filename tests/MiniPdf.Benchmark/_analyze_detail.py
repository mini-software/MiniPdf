"""Detailed analysis of cases below 0.99, showing actual sub-scores."""
import json

with open("reports/comparison_report.json", "r", encoding="utf-8") as f:
    data = json.load(f)

below = [r for r in data if r.get("overall_score", 0) < 0.99]
below.sort(key=lambda x: x["overall_score"])

print(f"Cases below 0.99: {len(below)} / {len(data)}\n")
print(f"{'Name':<50} {'Overall':>7} {'TextSim':>7} {'Visual':>7} {'Pages':>8}")
print("-" * 85)
for r in below:
    name = r["name"]
    score = r.get("overall_score", 0)
    text = r.get("text_similarity", 0)
    visual = r.get("visual_avg", 0)
    mp = r.get("minipdf_pages", 0)
    rp = r.get("reference_pages", 0)
    page_info = f"{mp}/{rp}"
    print(f"{name:<50} {score:>7.4f} {text:>7.3f} {visual:>7.3f} {page_info:>8}")

# Group by likely issue category
print("\n\n=== CATEGORY ANALYSIS ===")
page_mismatch = [r for r in below if r.get("minipdf_pages",0) != r.get("reference_pages",0)]
low_text = [r for r in below if r.get("text_similarity",1) < 0.95]
low_visual = [r for r in below if r.get("visual_avg",1) < 0.95]

print(f"\nPage count mismatch ({len(page_mismatch)}):")
for r in page_mismatch:
    print(f"  {r['name']}: {r['minipdf_pages']}p vs {r['reference_pages']}p (score={r['overall_score']:.4f})")

print(f"\nLow text similarity < 0.95 ({len(low_text)}):")
for r in sorted(low_text, key=lambda x: x.get("text_similarity",0)):
    print(f"  {r['name']}: text={r.get('text_similarity',0):.3f} (score={r['overall_score']:.4f})")

print(f"\nLow visual < 0.95 ({len(low_visual)}):")
for r in sorted(low_visual, key=lambda x: x.get("visual_avg",0)):
    print(f"  {r['name']}: visual={r.get('visual_avg',0):.3f} (score={r['overall_score']:.4f})")
