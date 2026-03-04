"""Analyze comparison_report.json and show all cases scoring below 0.99."""
import json

with open("reports/comparison_report.json", "r", encoding="utf-8") as f:
    data = json.load(f)

# Sort by score ascending
below = [r for r in data if r.get("overall_score", 0) < 0.99]
below.sort(key=lambda x: x["overall_score"])

print(f"Cases below 0.99: {len(below)} / {len(data)}\n")
print(f"{'Name':<55} {'Score':>7}  {'Text':>6}  {'Visual':>6}  {'Pages':>6}")
print("-" * 90)
for r in below:
    name = r["name"]
    score = r.get("overall_score", 0)
    text = r.get("text_score", 0)
    visual = r.get("visual_score", 0)
    pages = r.get("page_score", 0)
    print(f"{name:<55} {score:>7.4f}  {text:>6.3f}  {visual:>6.3f}  {pages:>6.3f}")

print(f"\nTotal below 0.99: {len(below)}")
print(f"Total cases: {len(data)}")
avg = sum(r["overall_score"] for r in data) / len(data)
print(f"Overall average: {avg:.4f}")
