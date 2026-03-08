import json
with open("reports_docx/comparison_report.json", encoding="utf-8") as f:
    data = json.load(f)
below = [x for x in data if x["overall_score"] < 0.99]
page_diff = [x for x in data if x.get("minipdf_pages",0) != x.get("reference_pages",0)]
print(f"Below 99: {len(below)}")
print(f"Page count mismatches: {len(page_diff)}")
print(f"Average: {sum(x['overall_score'] for x in data)/len(data):.4f}")

print("\nPage mismatches:")
for x in sorted(page_diff, key=lambda x: x["overall_score"])[:15]:
    print(f"  {x['name']:<50} mini={x['minipdf_pages']} ref={x['reference_pages']} score={x['overall_score']:.4f}")

# Compare scores that got worse
import os
old_path = "reports_docx/comparison_report_backup.json"
if not os.path.exists(old_path):
    print("\nNo backup report for comparison")
else:
    with open(old_path, encoding="utf-8") as f:
        old_data = json.load(f)
    old_scores = {x["name"]: x["overall_score"] for x in old_data}
    
    worse = []
    better = []
    for x in data:
        old = old_scores.get(x["name"], 0)
        delta = x["overall_score"] - old
        if delta < -0.005:
            worse.append((x["name"], old, x["overall_score"], delta))
        elif delta > 0.005:
            better.append((x["name"], old, x["overall_score"], delta))
    
    print(f"\nGot worse (>0.005): {len(worse)}")
    for name, old, new, d in sorted(worse, key=lambda x: x[3])[:15]:
        print(f"  {name:<50} {old:.4f} -> {new:.4f} ({d:+.4f})")
    
    print(f"\nGot better (>0.005): {len(better)}")
    for name, old, new, d in sorted(better, key=lambda x: -x[3])[:15]:
        print(f"  {name:<50} {old:.4f} -> {new:.4f} ({d:+.4f})")
