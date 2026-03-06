import json

data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
if isinstance(data, dict):
    items = data.get('results', [])
else:
    items = data
results = []
for r in items:
    f = r['file']
    o = r['overall_score']
    t = r['text_similarity']
    v = r.get('visual_average', r.get('visual_score', 0))
    p = r.get('page_count_score', 1)
    results.append((f, o, t, v, p))

results.sort(key=lambda x: -x[1])

print("=== Near misses (0.98 - 0.99) ===")
for f, o, t, v, p in results:
    if 0.98 <= o < 0.99:
        print(f"  {f}: overall={o:.4f} text={t:.4f} vis={v:.4f} page={p:.1f}")

print("\n=== Close misses (0.96 - 0.98) ===")
for f, o, t, v, p in results:
    if 0.96 <= o < 0.98:
        print(f"  {f}: overall={o:.4f} text={t:.4f} vis={v:.4f} page={p:.1f}")

print("\n=== All below 0.99 count ===")
below = sum(1 for _, o, _, _, _ in results if o < 0.99)
print(f"  Total below 0.99: {below}")
