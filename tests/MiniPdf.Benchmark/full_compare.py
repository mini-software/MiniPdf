import json, re

# Parse old benchmark from _latest_bench.txt
old_scores = {}
with open("../../_latest_bench.txt", encoding="utf-8") as f:
    for line in f:
        m = re.search(r'\|\s*\d+\s*\|[^|]*\|\s*(\S+)\s+(\S+)\s*\|', line)
        # Better: parse the table rows
        parts = [p.strip() for p in line.split("|")]
        if len(parts) >= 7 and parts[1].strip().isdigit():
            name_part = parts[2]
            # Remove emoji prefix
            name = re.sub(r'^[^\s]+\s+', '', name_part).strip()
            try:
                overall = float(parts[6].replace("*", "").strip())
                old_scores[name] = overall
            except:
                pass

# Parse new benchmark from JSON
new_scores = {}
with open("reports/comparison_report.json", encoding="utf-8") as f:
    data = json.load(f)
for item in data:
    new_scores[item["name"]] = item["overall_score"]

# Compare
improved = []
regressed = []
unchanged = []
for name in sorted(new_scores.keys()):
    new_s = new_scores[name]
    old_s = old_scores.get(name, new_s)
    delta = new_s - old_s
    if abs(delta) > 0.0002:
        if delta > 0:
            improved.append((delta, name, old_s, new_s))
        else:
            regressed.append((delta, name, old_s, new_s))
    else:
        unchanged.append(name)

print(f"New avg: {sum(new_scores.values())/len(new_scores):.4f}")
print(f"Old avg: {sum(old_scores.values())/len(old_scores):.4f}" if old_scores else "Old: no data")
print(f"Improved: {len(improved)}, Regressed: {len(regressed)}, Unchanged: {len(unchanged)}")

if improved:
    print("\n=== IMPROVED (delta > 0.02%) ===")
    for d, n, o, new in sorted(improved, reverse=True):
        print(f"  {n}: {o:.4f} -> {new:.4f} ({d:+.4f})")

if regressed:
    print("\n=== REGRESSED (delta < -0.02%) ===")
    for d, n, o, new in sorted(regressed):
        print(f"  {n}: {o:.4f} -> {new:.4f} ({d:+.4f})")
