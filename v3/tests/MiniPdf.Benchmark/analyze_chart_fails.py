import json

with open("reports/comparison_report.json") as f:
    results = json.load(f)

chart_cases = [r for r in results if r["overall_score"] < 0.99 and any(x in r["name"] for x in ["chart", "bar", "line", "pie", "area", "scatter", "radar", "doughnut", "bubble", "stock", "combo", "dashboard"])]

print(f"Chart-related cases below 0.99: {len(chart_cases)}\n")

for r in sorted(chart_cases, key=lambda x: x["overall_score"], reverse=True):
    name = r["name"]
    score = r["overall_score"]
    txt = r["text_similarity"]
    vis = r.get("visual_avg", 0)
    diff = r.get("text_diff", "")
    
    print(f"\n{'='*60}")
    print(f"{name}: score={score:.4f} txt={txt:.3f} vis={vis:.3f}")
    
    # Show diff (max 20 lines)
    if diff:
        lines = diff.split("\n")
        for line in lines[:25]:
            print(f"  {line}")
        if len(lines) > 25:
            print(f"  ... ({len(lines)-25} more lines)")
