import json

with open("reports/comparison_report.json") as f:
    results = json.load(f)

# Cases between 0.97 and 0.99
near = [r for r in results if 0.97 <= r["overall_score"] < 0.99]
near.sort(key=lambda r: r["overall_score"], reverse=True)

for r in near:
    name = r["name"]
    score = r["overall_score"]
    txt = r["text_similarity"]
    vis_avg = r.get("visual_avg", 0)
    diff = r.get("text_diff", "")
    
    print(f"\n{'='*60}")
    print(f"{name}: score={score:.4f} txt={txt:.3f} vis={vis_avg:.3f}")
    
    if txt < 1.0 and diff:
        lines = diff.split("\n")
        for line in lines[:20]:
            print(f"  {line}")
        if len(lines) > 20:
            print(f"  ... ({len(lines)-20} more lines)")
    elif txt >= 1.0:
        print(f"  [Text perfect - visual is the bottleneck]")
