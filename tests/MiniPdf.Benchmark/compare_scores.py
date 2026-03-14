import re
import sys

# Read the full benchmark output
with open(sys.argv[1], encoding='utf-8') as f:
    lines = f.readlines()

new_scores = {}
for line in lines:
    m = re.search(r'Comparing:\s*(classic\S+)\s+\.\.\.\s+score=([\d.]+)', line)
    if m:
        new_scores[m.group(1)] = float(m.group(2))

# Read old report  
old_scores = {}
with open('reports/comparison_report.md', encoding='utf-8') as f:
    for line in f:
        m = re.search(r'\|\s*\d+\s*\|.*?(classic\S+)\s*\|\s*([\d.]+)\s*\|\s*([\d.]+)\s*\|\s*(\d+)/(\d+)\s*\|\s*\*\*([\d.]+)\*\*', line)
        if m:
            old_scores[m.group(1)] = float(m.group(6))

print(f'Old avg: {sum(old_scores.values())/len(old_scores):.4f} ({len(old_scores)} cases)')
print(f'New avg: {sum(new_scores.values())/len(new_scores):.4f} ({len(new_scores)} cases)')

# Find regressions and improvements
changes = []
for name in sorted(new_scores):
    if name in old_scores:
        delta = new_scores[name] - old_scores[name]
        if abs(delta) > 0.002:
            changes.append((delta, name, old_scores[name], new_scores[name]))

changes.sort()
print(f'\n=== Regressions (>0.2%) ===')
for delta, name, old, new in changes:
    if delta < -0.002:
        print(f'  {delta:+.4f}  {name}: {old:.4f} -> {new:.4f}')

print(f'\n=== Improvements (>0.2%) ===')
for delta, name, old, new in changes:
    if delta > 0.002:
        print(f'  {delta:+.4f}  {name}: {old:.4f} -> {new:.4f}')
