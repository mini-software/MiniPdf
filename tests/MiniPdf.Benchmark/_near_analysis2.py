import json

data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
results = []
for r in data:
    name = r['name']
    o = r['overall_score']
    t = r['text_similarity']
    v = r['visual_avg']
    mp = r.get('minipdf_pages', 0)
    rp = r.get('reference_pages', 0)
    p = 1.0 if mp == rp else 0.0
    results.append((name, o, t, v, p))

results.sort(key=lambda x: -x[1])

print("=== Near misses (0.98 - 0.99) ===")
for f, o, t, v, p in results:
    if 0.98 <= o < 0.99:
        print(f"  {f}: overall={o:.4f} text={t:.4f} vis={v:.4f} page={p:.1f}")

print("\n=== Close (0.96 - 0.98) ===")
for f, o, t, v, p in results:
    if 0.96 <= o < 0.98:
        print(f"  {f}: overall={o:.4f} text={t:.4f} vis={v:.4f} page={p:.1f}")

print("\n=== Mid (0.93 - 0.96) ===")
for f, o, t, v, p in results:
    if 0.93 <= o < 0.96:
        print(f"  {f}: overall={o:.4f} text={t:.4f} vis={v:.4f} page={p:.1f}")
