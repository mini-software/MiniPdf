"""Find cases with text_similarity close to 1.0 (0.95-0.999) where small fixes could push to 1.0."""
import sys, io, json
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Restore baseline scores first by re-running benchmark
# For now, just use the current report

# Focus on cases with text_sim in 0.95..0.999
targets = []
for r in d:
    ts = r['text_similarity']
    if 0.92 < ts < 1.0:
        va = r.get('visual_avg', 0)
        ov = r['overall_score']
        text_gain = (1.0 - ts) * 0.4 / 191
        targets.append((r['name'], ts, va, ov, text_gain, r.get('text_diff', '')))

targets.sort(key=lambda x: -x[4])

print("Cases with text_sim 0.92-0.999 (potential text improvement):")
print(f"{'Case':45s} {'text':>8} {'vis':>8} {'overall':>8} {'gain':>8}")
for name, ts, va, ov, tg, diff in targets:
    print(f"  {name:45s} {ts:.4f} {va:.4f} {ov:.4f} {tg:.6f}")
    # Show first few lines of diff
    if diff:
        lines = diff.split('\n')[:10]
        for line in lines:
            if line.startswith('+') or line.startswith('-'):
                print(f"    {line[:100]}")
    print()
