import json

with open("reports/comparison_report.json", "r", encoding="utf-8") as f:
    data = json.load(f)

results = [r for r in data if any(r["name"].startswith(f"classic{i}_") for i in range(121, 151))]
results.sort(key=lambda x: x["overall_score"])

for r in results:
    print(f"  {r['name']:50s}  {r['overall_score']:.4f}")

print(f"\n  Count: {len(results)}")
avg = sum(r["overall_score"] for r in results) / len(results) if results else 0
print(f"  Average: {avg:.4f}")
