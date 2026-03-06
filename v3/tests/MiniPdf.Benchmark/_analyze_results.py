#!/usr/bin/env python3
"""Analyze benchmark results - find cases below 0.99 and categorize issues."""
import json, sys

with open('reports/comparison_report.json', encoding='utf-8') as f:
    results = json.load(f)
below99 = [(r['name'], r['overall_score'], r.get('text_similarity',0), r.get('visual_avg',0), r.get('text_diff','')) 
           for r in results if r['overall_score'] < 0.99]
below99.sort(key=lambda x: x[1])

print(f'Total below 0.99: {len(below99)}/{len(results)}')
print(f'Total at/above 0.99: {len(results)-len(below99)}/{len(results)}')
print()

# Group by score range
ranges = {'<0.80': [], '0.80-0.89': [], '0.90-0.94': [], '0.95-0.98': []}
for item in below99:
    s = item[1]
    if s < 0.80: ranges['<0.80'].append(item)
    elif s < 0.90: ranges['0.80-0.89'].append(item)
    elif s < 0.95: ranges['0.90-0.94'].append(item)
    else: ranges['0.95-0.98'].append(item)

for k,v in ranges.items():
    print(f'  {k}: {len(v)}')
print()

print('=== Worst 30 ===')
for n,s,t,v,d in below99[:30]:
    print(f'  {s:.4f} text={t:.4f} vis={v:.4f} {n}')
print()

# Show text diffs for worst 10
print('=== Text diffs for worst 10 ===')
for n,s,t,v,d in below99[:10]:
    print(f'\n--- {n} (score={s:.4f}) ---')
    if d:
        lines = d.split('\n')[:20]
        for line in lines:
            print(f'  {line}')
