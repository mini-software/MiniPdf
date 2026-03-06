import json

with open("reports/comparison_report.json", encoding="utf-8") as f:
    results = json.load(f)

over = [r for r in results if r["overall_score"] >= 0.99]
under = sorted(
    [r for r in results if r["overall_score"] < 0.99],
    key=lambda r: r["overall_score"],
    reverse=True,
)
avg = sum(r["overall_score"] for r in results) / len(results)
print(f">=0.99: {len(over)}, <0.99: {len(under)}, avg: {avg:.4f}")

# Show classic36
for r in results:
    if "classic36" in r["name"]:
        name = r["name"]
        score = r["overall_score"]
        txt = r["text_similarity"]
        vis = r.get("visual_similarity", 0)
        print(f"  {name}: overall={score:.4f} txt={txt:.3f} vis={vis:.3f}")

print(f"\nTop 30 closest to 0.99:")
for r in under[:30]:
    name = r["name"]
    score = r["overall_score"]
    txt = r["text_similarity"]
    vis_avg = r.get("visual_avg", 0)
    pages = 1.0 if r["minipdf_pages"] == r["reference_pages"] else 0.5
    print(f"  {name}: overall={score:.4f} txt={txt:.3f} vis={vis_avg:.3f} pg={pages}")
