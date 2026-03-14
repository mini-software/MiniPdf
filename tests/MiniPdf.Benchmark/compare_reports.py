"""Compare baseline vs new (column padding fix) benchmarks case by case."""
import json

old = {r['name']: r for r in json.load(open('reports/comparison_report.json', encoding='utf-8'))}
new = {r['name']: r for r in json.load(open('reports/new_report.json', encoding='utf-8'))}

deltas = []
for name in sorted(old.keys()):
    if name not in new:
        continue
    o = old[name]
    n = new[name]
    od = o['overall_score']
    nd = n['overall_score']
    delta = nd - od
    deltas.append((name, od, nd, delta,
                   o['text_similarity'], n['text_similarity'],
                   o.get('visual_avg', 0), n.get('visual_avg', 0)))

print("== Top improvements ==")
deltas.sort(key=lambda x: x[3], reverse=True)
for name, od, nd, delta, ot, nt, ov, nv in deltas[:15]:
    if delta > 0.0001:
        print(f"  {name:45s} {od:.4f} -> {nd:.4f} ({delta:+.4f})  text: {ot:.4f}->{nt:.4f}  vis: {ov:.4f}->{nv:.4f}")

print("\n== Top regressions ==")
deltas.sort(key=lambda x: x[3])
for name, od, nd, delta, ot, nt, ov, nv in deltas[:15]:
    if delta < -0.0001:
        print(f"  {name:45s} {od:.4f} -> {nd:.4f} ({delta:+.4f})  text: {ot:.4f}->{nt:.4f}  vis: {ov:.4f}->{nv:.4f}")

# Summary stats
total_delta = sum(d[3] for d in deltas) / len(deltas)
pos = sum(1 for d in deltas if d[3] > 0.0001)
neg = sum(1 for d in deltas if d[3] < -0.0001)
same = len(deltas) - pos - neg
print(f"\n{pos} improved, {neg} regressed, {same} unchanged")
print(f"Old avg: {sum(d[1] for d in deltas)/len(deltas):.6f}")
print(f"New avg: {sum(d[2] for d in deltas)/len(deltas):.6f}")
print(f"Net delta: {total_delta:+.6f}")
