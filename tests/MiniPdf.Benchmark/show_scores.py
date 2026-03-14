import json

# Load current results
with open("reports/comparison_report.json", encoding="utf-8") as f:
    current = json.load(f)

# Old scores from last benchmark (0.9689)
old_scores = {}
# Parse from the check_scores.py embedded data
import check_scores
# Can't do that - let me read the old report or use hardcoded data

# Instead, let's look at all cases that changed significantly
# We need to compare current vs old. Let me extract from the report markdown.
import re

# Read old report backup if exists, or just show all current scores sorted by delta
# For now, just show all scores sorted ascending
cases = current.get("cases", current.get("results", []))
if isinstance(cases, dict):
    items = [(k, v) for k, v in cases.items()]
else:
    items = [(c.get("name", c.get("case")), c) for c in cases]

# Just show current scores
scores = []
for name, data in items:
    if isinstance(data, dict):
        score = data.get("score", data.get("combined_score", 0))
    else:
        score = float(data)
    scores.append((score, name))

scores.sort()
print(f"Total cases: {len(scores)}")
print(f"Average: {sum(s for s,_ in scores)/len(scores):.4f}")
print(f"\nAll scores < 0.97:")
for s, n in scores:
    if s < 0.97:
        print(f"  {s:.4f}  {n}")
