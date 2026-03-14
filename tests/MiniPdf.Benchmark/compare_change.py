"""Compare current vs baseline report."""
import json, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

old = json.load(open('reports/new_report.json', encoding='utf-8'))  # using an earlier saved report
# Actually let's compare against our saved baseline
# First save current as chart_offset_report.json
cur = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Build lookup
old_map = {}
for f in ['reports/new_report.json']:
    try:
        old_data = json.load(open(f, encoding='utf-8'))
        for r in old_data:
            old_map[r['name']] = r['overall_score']
    except:
        pass

# Use hardcoded baseline score for comparison
baseline = 0.969069

# Find biggest changes
diffs = []
for r in cur:
    name = r['name']
    score = r['overall_score']
    # Get old score from old_map if available
    old_score = old_map.get(name, None)
    if old_score is not None:
        d = score - old_score  
        diffs.append((name, old_score, score, d))

diffs.sort(key=lambda x: x[3])
print("Worst regressions:")
for name, old, new, d in diffs[:15]:
    print(f"  {name:50s} {old:.4f} -> {new:.4f} ({d:+.4f})")
print("\nBest improvements:")
for name, old, new, d in diffs[-15:]:
    print(f"  {name:50s} {old:.4f} -> {new:.4f} ({d:+.4f})")
