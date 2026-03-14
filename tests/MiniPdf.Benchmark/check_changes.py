"""Compare current results with baseline to find changes."""
import json

d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Known baseline scores from _latest_bench.txt
# Let's just find cases that changed significantly
# Check chart-related cases since that's where legend was fixed

chart_cases = {}
for c in d:
    name = c['name']
    ts = max(c['text_similarity'], c.get('flat_text_similarity', 0), c.get('word_text_similarity', 0))
    vs = c['visual_avg']
    overall = c['overall_score']
    chart_cases[name] = (ts, vs, overall)

# Compare with known baseline values for chart cases
# From the analysis, these chart cases were affected
import re

# Read baseline from _latest_bench.txt
baseline = {}
with open(r'D:\git\MiniPdf\_latest_bench.txt', encoding='utf-8') as f:
    for line in f:
        m = re.match(r'\|\s*\d+\s*\|\s*[^\|]*?(classic\w+)\s*\|\s*([\d.]+)\s*\|\s*([\d.]+)\s*\|\s*\d+/\d+\s*\|\s*\*\*([\d.]+)\*\*\s*\|', line)
        if m:
            name = m.group(1)
            baseline[name] = {
                'text': float(m.group(2)),
                'vis': float(m.group(3)),
                'overall': float(m.group(4))
            }

# Find biggest changes
changes = []
for c in d:
    name = c['name']
    ts = max(c['text_similarity'], c.get('flat_text_similarity', 0), c.get('word_text_similarity', 0))
    vs = c['visual_avg']
    overall = c['overall_score']
    
    if name in baseline:
        b = baseline[name]
        delta = overall - b['overall']
        if abs(delta) > 0.001:
            changes.append((name, b['text'], ts, b['vis'], vs, b['overall'], overall, delta))

changes.sort(key=lambda x: x[7])
print(f"{'Name':<45} {'b_text':>6} {'c_text':>6} {'b_vis':>6} {'c_vis':>6} {'b_ovr':>6} {'c_ovr':>6} {'delta':>7}")
for name, bt, ct, bv, cv, bo, co, delta in changes:
    print(f"{name:<45} {bt:6.4f} {ct:6.4f} {bv:6.4f} {cv:6.4f} {bo:6.4f} {co:6.4f} {delta:+7.4f}")
