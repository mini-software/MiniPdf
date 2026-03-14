"""Compare committed baseline report vs current report to find differences."""
import json, os

temp = os.environ['TEMP']
with open(os.path.join(temp, 'baseline_report.json'), encoding='utf-8-sig') as f:
    baseline = {x['name']: x for x in json.load(f)}

with open('reports/comparison_report.json', encoding='utf-8') as f:
    current = {x['name']: x for x in json.load(f)}

b_avg = sum(x['overall_score'] for x in baseline.values()) / len(baseline)
c_avg = sum(x['overall_score'] for x in current.values()) / len(current)
print(f"Committed baseline avg: {b_avg:.6f}")
print(f"Current avg:            {c_avg:.6f}")
print(f"Delta:                  {c_avg - b_avg:+.6f}")
print()

diffs = []
for name in baseline:
    if name in current:
        b = baseline[name]['overall_score']
        c = current[name]['overall_score']
        d = c - b
        if abs(d) > 0.0001:
            bt = baseline[name].get('text_similarity', 0)
            ct = current[name].get('text_similarity', 0)
            bv = baseline[name].get('visual_avg', 0)
            cv = current[name].get('visual_avg', 0)
            diffs.append((d, name, b, c, bt, ct, bv, cv))

diffs.sort()
print(f"Cases with change > 0.0001: {len(diffs)}")
print()
print("=== REGRESSIONS (worst first) ===")
for d, name, b, c, bt, ct, bv, cv in diffs:
    if d < 0:
        print(f"  {name:45s}  {b:.4f} -> {c:.4f}  ({d:+.4f})  text: {bt:.4f}->{ct:.4f}  vis: {bv:.4f}->{cv:.4f}")
print()
print("=== IMPROVEMENTS (best first) ===")
for d, name, b, c, bt, ct, bv, cv in reversed(diffs):
    if d > 0:
        print(f"  {name:45s}  {b:.4f} -> {c:.4f}  ({d:+.4f})  text: {bt:.4f}->{ct:.4f}  vis: {bv:.4f}->{cv:.4f}")
